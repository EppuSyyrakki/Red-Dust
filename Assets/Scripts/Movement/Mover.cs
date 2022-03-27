using UnityEngine;
using UnityEngine.AI;

namespace RedDust.Movement
{
	/// <summary>
	/// Handles messaging with the NavMeshAgent and updates all movement-related animator parameters.
	/// </summary>
	[RequireComponent(typeof(Rigidbody), typeof(NavMeshAgent), typeof(Animator))]
	public class Mover : MonoBehaviour
	{		
		[SerializeField, Range(2, Values.Navigation.MaxSpeed)]
		float moveSpeed = 5f;

		[SerializeField]
		private float turningSpeed = 2f;

		private NavMeshAgent navMeshAgent;
		private Animator animator;
		private bool isSneaking;
		private float forwardSpeed;
		private MovementIndicator indicator = null;
		private bool indicatorEnabled = false;

		public float MoveSpeed => moveSpeed;
		public bool IsSneaking => isSneaking;
		public bool IsMoving => forwardSpeed > Values.Navigation.MovingThreshold;

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
			Vector3 local = transform.InverseTransformDirection(navMeshAgent.velocity);
			forwardSpeed = local.z / Values.Navigation.MaxSpeed;
			float turningSpeed = Mathf.Lerp(local.normalized.x, 0, 10f * Time.deltaTime);
			animator.SetFloat(Values.Animation.Velocity, forwardSpeed);
			animator.SetFloat(Values.Animation.Turning, turningSpeed);

			if (indicatorEnabled && IsAtDestination()) { EnableMoveIndicator(false); }
		}

		private void SetSpeed(float speed)
		{
			navMeshAgent.speed = speed;
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
			Vector3 stopPosition = transform.position + toTarget * Values.Navigation.StopDistance;
			SetDestination(stopPosition);
			EnableMoveIndicator(false);
		}

		public void SetDestination(Vector3 destination, bool useIndicator = false)
		{
			navMeshAgent.SetDestination(destination);
			
			if (useIndicator) { EnableMoveIndicator(true); }
		}

		public bool IsAtDestination()
		{
			return navMeshAgent.remainingDistance < Values.Navigation.MoveTargetTreshold;
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
			SetSpeed(MoveSpeed * Values.Navigation.WalkMulti);
		}

		public void Run()
		{
			SetSpeed(MoveSpeed);
		}

		public void ToggleSneak()
		{
			if (!isSneaking)
			{
				SetSpeed(MoveSpeed * Values.Navigation.CrouchMulti);
				isSneaking = true;
			}
			else
			{
				SetSpeed(MoveSpeed);
				isSneaking = false;
			}

			animator.SetBool(Values.Animation.Crouched, isSneaking);
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

		private void EnableMoveIndicator(bool enable)
		{
			Transform t = indicator.transform;		

			if (enable) 
			{ 
				t.SetParent(null);
				t.position = navMeshAgent.destination;
			}
			else 
			{
				t.position = transform.position;
				t.SetParent(transform); 
			}

			indicatorEnabled = enable;
			indicator.gameObject.SetActive(enable);			
		}

		public void SetMoveIndicatorColor(Color color)
		{
			indicator.Color = color;
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