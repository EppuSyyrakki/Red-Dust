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
		[SerializeField, Range(2, Game.Navigation.MaxSpeed)]
		float moveSpeed = 5f;

		[SerializeField]
		private float turningSpeed = 2f;

		private NavMeshAgent _navMeshAgent;
		private Animator _animator;
		private bool _isSneaking;
		private float _forwardSpeed;

		public float MoveSpeed => moveSpeed;
		public bool IsSneaking => _isSneaking;
		public bool IsMoving => _forwardSpeed > Game.Navigation.MovingThreshold;

		#region Unity messages

		private void Awake()
		{
			_navMeshAgent = GetComponent<NavMeshAgent>();
			_navMeshAgent.SetDestination(transform.position);
			_navMeshAgent.speed = moveSpeed;
			_animator = GetComponent<Animator>();
		}

		private void OnDisable()
		{
			_animator.enabled = false;
			_navMeshAgent.enabled = false;
		}

		private void Update()
		{
			UpdateAnimator();
		}

		#endregion Unity messages

		#region Private methods

		private void UpdateAnimator()
		{
			Vector3 local = transform.InverseTransformDirection(_navMeshAgent.velocity);
			_forwardSpeed = local.z / Game.Navigation.MaxSpeed;
			float turningSpeed = Mathf.Lerp(local.normalized.x, 0, 10f * Time.deltaTime);
			_animator.SetFloat(Game.Animation.Velocity, _forwardSpeed);
			_animator.SetFloat(Game.Animation.Turning, turningSpeed);
		}

		private void SetSpeed(float speed)
		{
			_navMeshAgent.speed = speed;
		}

		#endregion Private methods

		#region Public API

		public bool IsPointOnNavMesh(Vector3 point, out NavMeshHit hit)
		{
			int areas = NavMesh.AllAreas;

			if (!NavMesh.SamplePosition(point, out hit, Game.Navigation.MaxNavMeshProjection, areas)) 
			{
				// No such point on the NavMesh within the MaxNavMeshProjection range
				return false; 
			}

			return true;
		}

		public void Stop()
		{
			Vector3 stopPosition = transform.position + transform.forward * Game.Navigation.StopDistance;
			_navMeshAgent.SetDestination(stopPosition);
		}

		public void SetDestination(Vector3 destination)
		{
			_navMeshAgent.SetDestination(destination);
		}

		public bool IsAtDestination()
		{
			return _navMeshAgent.remainingDistance < Game.Navigation.MoveTargetTreshold;
		}

		public bool TurnTowards(Vector3 position)
		{
			Vector3 direction = position - transform.position;
			float dot = Vector3.Dot(transform.forward, direction);

			if (!Mathf.Approximately(dot, 1))
			{
				float speed = Time.deltaTime * turningSpeed;
				float angle = Vector3.SignedAngle(transform.forward, direction, Vector3.up) * speed;
				transform.Rotate(Vector3.up, angle);
				return false;
			}

			return true;
		}

		public void Walk()
		{
			SetSpeed(MoveSpeed * Game.Navigation.WalkMulti);
		}

		public void Run()
		{
			SetSpeed(MoveSpeed);
		}

		public void ToggleSneak()
		{
			if (!_isSneaking)
			{
				SetSpeed(MoveSpeed * Game.Navigation.CrouchMulti);
				_isSneaking = true;
			}
			else
			{
				SetSpeed(MoveSpeed);
				_isSneaking = false;
			}

			_animator.SetBool(Game.Animation.Crouched, _isSneaking);
		}

		public void Warp(Vector3 position, Quaternion rotation)
		{
			_navMeshAgent.Warp(position);
			transform.rotation = rotation;
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