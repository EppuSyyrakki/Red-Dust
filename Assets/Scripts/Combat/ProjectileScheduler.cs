﻿using System.Collections;
using System.Collections.Generic;
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
	/// Hosting all Projectiles and other attacks in the game and calculating them via Jobs.
	/// 
	/// // TODO: Change projectileQueue to drawing from an object pool
	/// </summary>
	public class ProjectileScheduler
	{
		public ProjectileScheduler()
		{
			projectiles = new List<Projectile>(totalMax);
			spawnQueue = new Queue<ProjectileMsg>(totalMax);
			deSpawnQueue = new Queue<Projectile>(totalMax);

			spawnSub = Game.Instance.Bus.Subscribe<ProjectileMsg>(OnSpawnProjectile);
		}

		~ProjectileScheduler()
		{
			Game.Instance.Bus.Unsubscribe(spawnSub);
		}

		private readonly int totalMax = Values.Combat.MaxProjectiles;
		private readonly int raycastsMax = Values.Combat.MaxRaycastHits;
		private List<Projectile> projectiles;
		private Queue<ProjectileMsg> spawnQueue;
		private Queue<Projectile> deSpawnQueue;
		private ISubscription<ProjectileMsg> spawnSub;
		private ProjectileJob projectileJob;
		private JobHandle raycastHandle;
		private JobHandle moveHandle;

		private NativeArray<RaycastCommand> info;
		private NativeArray<RaycastCommand> result;
		private NativeArray<RaycastHit> hits;		
		private NativeArray<float> travelled;
		private NativeArray<int> penetration;

		public bool HasJobs => projectiles.Count > 0;

		public bool HasSpawnQueue => spawnQueue.Count > 0;
		public bool HasDeSpawnQueue => deSpawnQueue.Count > 0;

		public void RunSpawn()
		{
			for (int i = 0; i < spawnQueue.Count; i++)
			{
				var msg = spawnQueue.Dequeue();
				Projectile p = Object.Instantiate(msg.Prefab);				
				p.Init(msg.Origin, msg.Dir, msg.SenderId);
				projectiles.Add(p);
				p.Finished += OnProjectileFinished;
			}
		}

		public void RunDeSpawn()
		{
			for (int i = 0; i < deSpawnQueue.Count; i++)
			{
				var p = deSpawnQueue.Dequeue();
				projectiles.Remove(p);
				Object.Destroy(p.gameObject);
			}
		}

		private void OnSpawnProjectile(ProjectileMsg msg)
		{
			if (projectiles.Count >= totalMax)
			{
				Debug.Log("ProjectileScheduler: max projectile count reached: " + totalMax);
				return;
			}

			spawnQueue.Enqueue(msg);
		}

		private void OnProjectileFinished(Projectile p)
		{
			p.Finished -= OnProjectileFinished;
			deSpawnQueue.Enqueue(p);
		}

		/// <summary>
		/// Schedule all bullet raycast jobs here and call this in Game.Update().
		/// </summary>
		public void ScheduleJobs()
		{
			info = new NativeArray<RaycastCommand>(projectiles.Count, Allocator.TempJob);
			hits = new NativeArray<RaycastHit>(projectiles.Count * raycastsMax, Allocator.TempJob);
			result = new NativeArray<RaycastCommand>(projectiles.Count, Allocator.TempJob);
			travelled = new NativeArray<float>(projectiles.Count, Allocator.TempJob);
			penetration = new NativeArray<int>(projectiles.Count, Allocator.TempJob);

			for (int i = 0; i < projectiles.Count; i++)
			{
				info[i] = projectiles[i].Info;
				travelled[i] = projectiles[i].Travelled;
				penetration[i] = projectiles[i].Penetration;
			}

			raycastHandle = RaycastCommand.ScheduleBatch(info, hits, 1);

			projectileJob = new ProjectileJob()
			{
				info = info,
				result = result,
				travelled = travelled
			};

			moveHandle = projectileJob.Schedule(info.Length, 64, raycastHandle);
		}

		/// <summary>
		/// This assigns the job results to the projectiles. Call in Game.LateUpdate();
		/// </summary>
		public void CompleteJobs()
		{
			raycastHandle.Complete();
			moveHandle.Complete();

			for (int i = 0; i < projectiles.Count; i++)
			{
				projectiles[i].Set(result[i], penetration[i], travelled[i]);
			}

			info.Dispose();
			hits.Dispose();
			result.Dispose();
			travelled.Dispose();
			penetration.Dispose();
		}		
	}

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
					info[i].from + movement,
					info[i].direction,
					0,
					info[i].layerMask,
					info[i].maxHits
				);

		}
	}
}