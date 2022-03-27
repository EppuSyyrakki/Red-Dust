using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Jobs;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;
using Messaging;
using RedDust.Messages;

namespace RedDust.Combat
{
	/// <summary>
	/// Hosting all Projectiles and calculating their raycasts and positions via the Jobs System.
	/// 
	/// // TODO: Change projectileQueue to drawing from an object pool
	/// </summary>
	public class ProjectileScheduler
	{
		public ProjectileScheduler()
		{
			projectiles = new List<Projectile>(totalMax);
			createQueue = new Queue<ProjectileMsg>(totalMax);
			removeQueue = new Queue<Projectile>(totalMax);
			spawnSub = Game.Instance.Bus.Subscribe<ProjectileMsg>(OnCreateProjectile);
		}

		public void UnsubscribeSpawnMsg() { Game.Instance.Bus.Unsubscribe(spawnSub); }

		private const int totalMax = Values.Combat.MaxProjectiles;
		private const int hitsPerResult = Values.Combat.MaxRaycastHits;
		private const float multiHitStep = Values.Combat.MultiHitMinStep;
		private List<Projectile> projectiles;
		private Queue<ProjectileMsg> createQueue;
		private Queue<Projectile> removeQueue;
		private ISubscription<ProjectileMsg> spawnSub;
		private ProjectileJob projectileJob;
		private JobHandle raycastHandle;
		private JobHandle moveHandle;

		/// <summary>
		/// Projectile positions (from), directions and velocities (distance) before the movement.
		/// </summary>
		private NativeArray<RaycastCommand> info;
		/// <summary>
		/// Projectile positions (from) and directions after movement. Velocity is recalculated when these are given
		/// to the Projectile.Set() method.
		/// </summary>
		private NativeArray<RaycastCommand> result;
		private NativeArray<RaycastHit> hits;		
		/// <summary>
		/// Total distance travelled by the projectiles. Used to remove "old" projectiles.
		/// </summary>
		private NativeArray<float> travelled;
		/// <summary>
		/// How many points of armor are ignored. Reduced by of armor each time passing through one.
		/// </summary>
		private NativeArray<int> penetration;
		/// <summary>
		/// How many points of damage is caused. Reduced by armor each time passing through one.
		/// </summary>
		private NativeArray<int> damage;
		/// <summary>
		/// Copy of info. RaycastCommandMultihit needs a copy as it drops other RaycastCommand info except "from",
		/// and we need to use that data further down the line.
		/// </summary>
		private NativeArray<RaycastCommand> raycasts;

		public void RunUpdate()
		{
			// Create projectiles and then run all projectiles raycasts and movement jobs
			if (createQueue.Count > 0) { RunCreateQueue(); }
			if (projectiles.Count > 0) { ScheduleJobs(); }
		}

		public void RunLateUpdate()
		{
			// Copy results from jobs to projectiles and remove any flagged projectiles
			if (projectiles.Count > 0) { CompleteJobs(); }
			if (removeQueue.Count > 0) { RunRemoveQueue(); }
		}

		private void RunCreateQueue()
		{
			for (int i = 0; i < createQueue.Count; i++)
			{
				var msg = createQueue.Dequeue();
				Projectile p = Object.Instantiate(msg.Prefab);				
				p.Init(msg.Origin, msg.Dir, msg.SenderId, msg.WpnPenetration, msg.Effects);
				projectiles.Add(p);
				p.Finished += OnProjectileFinished;
			}
		}

		private void RunRemoveQueue()
		{
			for (int i = 0; i < removeQueue.Count; i++)
			{
				var p = removeQueue.Dequeue();
				projectiles.Remove(p);
				Object.Destroy(p.gameObject);
			}
		}

		private void OnCreateProjectile(ProjectileMsg msg)
		{
			if (projectiles.Count >= totalMax)
			{
				Debug.LogWarning("ProjectileScheduler: max projectile count reached: " + totalMax);
				return;
			}

			createQueue.Enqueue(msg);
		}

		private void OnProjectileFinished(Projectile p)
		{
			p.Finished -= OnProjectileFinished;
			removeQueue.Enqueue(p);
		}

		/// <summary>
		/// Schedule all bullet raycast and movement jobs. Call this in Game.Update().
		/// </summary>
		private void ScheduleJobs()
		{
			info = new NativeArray<RaycastCommand>(projectiles.Count, Allocator.TempJob);
			hits = new NativeArray<RaycastHit>(projectiles.Count * hitsPerResult, Allocator.TempJob);
			result = new NativeArray<RaycastCommand>(projectiles.Count, Allocator.TempJob);
			travelled = new NativeArray<float>(projectiles.Count, Allocator.TempJob);
			penetration = new NativeArray<int>(projectiles.Count, Allocator.TempJob);
			damage = new NativeArray<int>(projectiles.Count, Allocator.TempJob);

			for (int i = 0; i < projectiles.Count; i++)
			{
				info[i] = projectiles[i].Info;
				travelled[i] = projectiles[i].Travelled;
				penetration[i] = projectiles[i].Penetration;
				damage[i] = projectiles[i].Damage;
			}

			// A copy of 'info' is needed as the hack only returns the 'from' property. We don't want the original
			// to be altered as its used further down the line.
			raycasts = new NativeArray<RaycastCommand>(info, Allocator.TempJob);

			// BUG: original RaycastCommand only returns a single hit. Multihit is a "hack"
			// TIP: Can't access colliders inside a job, but RaycastHit.normal is Vector3.zero for rays
			// that hit nothing.
			raycastHandle = RaycastCommandMultihit.ScheduleBatch(raycasts, hits, 8, hitsPerResult, default, multiHitStep);
			projectileJob = new ProjectileJob() 
			{ 
				info = info, result = result, travelled = travelled
			};
			moveHandle = projectileJob.Schedule(info.Length, 64);
		}

		/// <summary>
		/// This assigns the job results to the projectiles. Call in Game.LateUpdate();
		/// </summary>
		private void CompleteJobs()
		{
			raycastHandle.Complete();
			moveHandle.Complete();
			// A list for the RaycastHits that actually hit something
			List<RaycastHit> validHits = new List<RaycastHit>(hitsPerResult);

			// Loop through all the hits. result.Length must be used as the outer loop because for every
			// result there is hitsPerResult amount of RaycastHits.
			for (int i = 0; i < result.Length; i++)
			{
				Debug.DrawLine(info[i].from, result[i].from, Color.red);
				// i * hitsPerResult is where hits of result[i] begin and continue for hitsPerResult
				int hitIndex = i * hitsPerResult;

				for (int j = hitIndex; j < hitIndex + hitsPerResult; j++)
				{
					// If there's no further hits for result[i], the collider is null. No need to process further hits.
					if (hits[j].collider == null) { break; }

					validHits.Add(hits[j]);	// This one actually hit something
				}

				if (validHits.Count > 0)	// There is an actual hit so order the list by distance
				{
					var orderedHits = validHits.OrderBy(r => r.distance).ToList();
					HandleHits(i, orderedHits); // Do the stuff with the hits, depending on collider
					validHits.Clear();
				}
				
				projectiles[i].Set(result[i], penetration[i], damage[i], travelled[i]);
			}

			info.Dispose();
			hits.Dispose();
			result.Dispose();
			travelled.Dispose();
			penetration.Dispose();
			damage.Dispose();
			raycasts.Dispose();
		}

		/// <summary>
		/// Process hits from a raycast. Hits have already been ordered from closest to furthest.
		/// </summary>
		/// <param name="i">Index of relevant projectile in the NativeArrays</param>
		/// <param name="hit">The RaycastHit from the cast.</param>
		private void HandleHits(int i, List<RaycastHit> hits)
		{			
			foreach (var hit in hits)
			{
				Collider col = hit.collider;
				int layer = col.gameObject.layer;

				if ((1 << layer & Values.Layer.Indestructible) > 0
					|| (1 << layer & Values.Layer.Ground) > 0)
				{
					penetration[i] = 0;
					damage[i] = 0;
					result[i] = new RaycastCommand(hit.point, result[i].direction, 0, layer, 0);
				}
				else if ((1 << layer & Values.Layer.Destructible) > 0 
					|| (1 << layer & Values.Layer.Character) > 0)
				{
					var health = col.GetComponent<Health>();
					damage[i] -= health.Status.armor;
					health.TakeDamage(damage[i]);
					penetration[i] -= health.Status.armor;	
				}

				if (projectiles[i].Hit.normal != Vector3.zero) { continue; }
				
				// the projectile hasn't registered a hit before
				projectiles[i].Hit = hit;

				if (projectiles[i].Effects[0] == null) { continue; }

				foreach(var effectPrefab in projectiles[i].Effects)
				{
					var effect = Object.Instantiate(effectPrefab, hit.collider.transform);
					effect.Hit = hit;
				}
			}				
		}
	}

	// TODO: This job could easily be accomodated to account for drop, to handle stuff like grenades and such.
	[BurstCompile]
	public struct ProjectileJob : IJobParallelFor
	{
		[ReadOnly]
		public NativeArray<RaycastCommand> info;

		public NativeArray<RaycastCommand> result;
		public NativeArray<float> travelled;		

		public void Execute(int i)
		{
			Vector3 movement = info[i].direction * info[i].distance;
			travelled[i] = travelled[i] + movement.magnitude;
			result[i] = new RaycastCommand
				(
					info[i].from + movement, info[i].direction, 0, info[i].layerMask, info[i].maxHits
				);
		}
	}
}