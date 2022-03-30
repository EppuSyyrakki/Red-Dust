using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace RedDust.Motion
{
	/// <summary>
	/// Handles messaging with the NavMeshAgent, updates all animator parameters.
	/// </summary>
	[RequireComponent(typeof(Rigidbody), typeof(NavMeshAgent), typeof(Animator))]
	public class MotionControl : MonoBehaviour
	{
		private const int ANIM_LAYER_COMBAT = Values.Animation.CombatLayer;
		private const float MAX_SPEED = Values.Navigation.MaxSpeed;
		private const float STOP_DISTANCE = Values.Navigation.StopDistance;
		private const float MOVE_THRESHOLD = Values.Navigation.MovingThreshold;
		private const float WALK_MULTIPLIER = Values.Navigation.WalkMulti;
		private const float CROUCH_MULTIPLIER = Values.Navigation.CrouchMulti;
		private const float TARGET_THRESHOLD = Values.Navigation.MoveTargetTreshold;
		private const string ANIM_VELOCITY = Values.Animation.Velocity;
		private const string ANIM_TURNING = Values.Animation.Turning;
		private const string ANIM_CROUCHED = Values.Animation.Crouched;		

		[SerializeField, Range(2, Values.Navigation.MaxSpeed)]
		float moveSpeed = 5f;

		[SerializeField]
		private float turningSpeed = 2f;

		[SerializeField]
		private float aimLayerBlendTime = 0.3f;

		private NavMeshAgent navMeshAgent;
		private Animator animator;
		private bool isSneaking;
		private float forwardSpeed;
		private MovementIndicator indicator = null;
		private bool indicatorEnabled = false;
		private Coroutine aimBlend;

		/// <summary>
		/// Broadcasts location of aimed object. Used by Actions.
		/// </summary>
		public event Action<Vector3> Aiming;
		/// <summary>
		/// Broadcasts if aiming was started (true) or finished (false). Used by Actions.
		/// </summary>
		public event Action<bool> AimingEnabled;
		
		public float MoveSpeed => moveSpeed;
		public bool IsSneaking => isSneaking;
		public bool IsMoving => forwardSpeed > MOVE_THRESHOLD;

		//private void Start()
		//{
		//	BlendCombat(true);  // TEMPORARY FOR RIGGING
		//}

		#region Unity messages

		private void Awake()
		{			
			navMeshAgent = GetComponent<NavMeshAgent>();
			navMeshAgent.SetDestination(transform.position);
			navMeshAgent.speed = moveSpeed;
			animator = GetComponent<Animator>();
			indicator = GetComponentInChildren<MovementIndicator>(true);	
		}

		private void OnDisable()
		{
			animator.enabled = false;
			navMeshAgent.enabled = false;
		}

        private void Update()
		{
			UpdateAnimator();
		}

		#endregion Unity messages

		#region Private methods

		private void UpdateAnimator()
		{
			var velocity = navMeshAgent.velocity;
			Vector3 local = transform.InverseTransformDirection(velocity);
			forwardSpeed = local.z / MAX_SPEED;
			float turningSpeed = Mathf.Lerp(local.x, 0, 40f * Time.deltaTime);
			Debug.Log(local);


			animator.SetFloat(ANIM_VELOCITY, forwardSpeed);
			animator.SetFloat(ANIM_TURNING, turningSpeed);

			if (indicatorEnabled && IsAtDestination()) 
			{ 
				DisableMoveIndicator(); 
			}
		}

		private void SetSpeed(float speed)
		{
			navMeshAgent.speed = speed;
		}

		private void EnableMoveIndicator(Vector3 destination)
		{
			indicatorEnabled = true;
			indicator.transform.SetParent(null);
			indicator.transform.position = destination;
			indicator.gameObject.SetActive(false);
		}

		private void DisableMoveIndicator()
		{
			indicatorEnabled = false;
			indicator.transform.position = transform.position;
			indicator.transform.SetParent(transform);
			indicator.gameObject.SetActive(false);
		}

		#endregion Private methods

		#region Public API

		/// <summary>
		/// Sets the current destination near the current position. Any queued Actions will get executed
		/// after this. To cancel all actions, use Character.CancelActions().
		/// </summary>
		public void Stop()
		{
			Vector3 toTarget = (navMeshAgent.destination - transform.position).normalized;
			Vector3 stopPosition = transform.position + toTarget * STOP_DISTANCE;
			SetDestination(stopPosition);
			DisableMoveIndicator();
		}

		public void SetDestination(Vector3 destination, bool useIndicator = false)
		{
			navMeshAgent.SetDestination(destination);
			
			if (useIndicator) { EnableMoveIndicator(destination); }
		}

		public bool IsAtDestination()
		{
			return navMeshAgent.remainingDistance < TARGET_THRESHOLD;
		}

		/// <summary>
		/// Make the character turn towards a position.
		/// </summary>
		/// <param name="target">The target position</param>
		/// <returns>True if facing target, false if still turning</returns>
		public bool TurnTowards(Vector3 target)
		{
			target.y = transform.position.y;	// eliminate getting the dot wrong from height difference.
			Vector3 direction = target - transform.position;
			float dot = Vector3.Dot(transform.forward, direction);

			if (Mathf.Approximately(dot, 1)) { return true; }

			float speed = Time.deltaTime * turningSpeed;
			float angle = Vector3.SignedAngle(transform.forward, direction, Vector3.up) * speed;
			transform.Rotate(Vector3.up, angle);
			return false;
		}

		public void Walk()
		{
			SetSpeed(MoveSpeed * WALK_MULTIPLIER);
		}

		public void Run()
		{
			SetSpeed(MoveSpeed);
		}

		public void ToggleSneak()
		{
			if (!isSneaking)
			{
				SetSpeed(MoveSpeed * CROUCH_MULTIPLIER);
				isSneaking = true;
			}
			else
			{
				SetSpeed(MoveSpeed);
				isSneaking = false;
			}

			animator.SetBool(ANIM_CROUCHED, isSneaking);
		}

		public void SetStoppingDistance(float stoppingDistance)
		{
			navMeshAgent.stoppingDistance = Mathf.Clamp(stoppingDistance, 0, 10f);
		}

		public NavMeshPathStatus GetPathStatus()
		{
			return navMeshAgent.path.status;
		}

		public void Warp(Vector3 position, Quaternion rotation)
		{
			navMeshAgent.Warp(position);
			transform.rotation = rotation;
		}

		public void Aim(Vector3 target)
		{
			Aiming?.Invoke(target);
		}

		public void SetMoveIndicatorColor(Color color)
		{
			indicator.Color = color;
		}

		public void BlendCombat(bool blendIn)
		{
			var current = animator.GetLayerWeight(ANIM_LAYER_COMBAT);

			if ((blendIn && current == 1) || (!blendIn && current == 0)) { return; }

			if (aimBlend != null) { StopCoroutine(aimBlend); }

			float target = blendIn ? 1 : 0;
			aimBlend = StartCoroutine(BlendLayerTo(current, target, ANIM_LAYER_COMBAT, aimLayerBlendTime));
			AimingEnabled?.Invoke(blendIn);
		}

		private IEnumerator BlendLayerTo(float currentWeight, float targetWeight, int layer, float time)
		{
			float t = 0;

			while (t < time)
			{
				if (Mathf.Approximately(currentWeight, targetWeight))
				{
					currentWeight = targetWeight;
				}

				t += Time.deltaTime;
				animator.SetLayerWeight(layer, Mathf.Lerp(currentWeight, targetWeight, t / time));
				yield return null;
			}
		}

		#endregion Public API

		//#region ISaveable implementation

		//public object CaptureState()
		//{
		//	return new MoverSaveData(transform.position, transform.rotation);
		//}

		//public void RestoreState(object state)
		//{
		//	MoverSaveData data = (MoverSaveData)state;
		//	Warp(data.Location.Position.ToVector(), data.Location.Rotation.ToQuaternion());
		//}

		//#endregion
	}
}