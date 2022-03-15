using System;
using UnityEngine;
using UnityEngine.AI;

namespace RedDust.Movement
{
	[RequireComponent(typeof(Rigidbody), typeof(NavMeshAgent), typeof(Animator))]
	public class Mover : MonoBehaviour
	{
		[SerializeField]
		private float turningSpeed = 2f, moveSpeed = 5f;

		private Rigidbody _rb;
		private NavMeshAgent _navMeshAgent;
		private Animator _animator;
		private bool _isSneaking;
		private float _forwardSpeed;

		public float TurningSpeed => turningSpeed;
		public float MoveSpeed => moveSpeed;
		public bool IsSneaking => _isSneaking;
		public bool IsMoving => _forwardSpeed > Config.Navigation.MovingThreshold;

		#region Unity messages

		private void Awake()
		{
			_rb = GetComponent<Rigidbody>();
			_navMeshAgent = GetComponent<NavMeshAgent>();
			_navMeshAgent.SetDestination(transform.position);
			_animator = GetComponent<Animator>();
		}

		private void OnDisable()
		{
			_animator.enabled = false;
			_navMeshAgent.enabled = false;
		}

		private void Update()
		{
			// UpdateAnimator();
		}

		#endregion Unity messages

		#region Private methods

		private void UpdateAnimator()
		{
			_forwardSpeed = transform.InverseTransformVector(_rb.velocity).z;
			_animator.SetFloat(Config.Animation.Speed, _forwardSpeed);
		}

		private void SetSpeed(float speed)
		{
			_navMeshAgent.speed = speed;
		}

		#endregion Private methods

		#region Public API

		public bool HasPathTo(Vector3 point, ref Vector3 target, ref NavMeshPath path)
		{
			float dist = Config.Navigation.MaxNavMeshProjection;
			NavMeshHit navMeshHit;
			int areas = NavMesh.AllAreas;

			if (!NavMesh.SamplePosition(point, out navMeshHit, dist, areas)) { return false; }

			target = navMeshHit.position;
			Vector3 source = transform.position;

			if (!NavMesh.CalculatePath(source, target, areas, path)) { return false; }

			if (path.status != NavMeshPathStatus.PathComplete) { return false; }

			return true;
		}

		public void SetPath(NavMeshPath path)
		{
			_navMeshAgent.SetPath(path);
		}

		public bool IsAtDestination()
		{
			return _navMeshAgent.remainingDistance < Config.Navigation.MoveTargetTreshold;
		}

		public void TurnTowards(Vector3 position)
		{
			Vector3 direction = position - transform.position;
			float dot = Vector3.Dot(transform.forward, direction);

			if (!Mathf.Approximately(dot, 1))
			{
				float speed = Time.deltaTime * TurningSpeed;
				float angle = Vector3.SignedAngle(transform.forward, direction, Vector3.up) * speed;
				transform.Rotate(Vector3.up, angle);
			}
		}

		public void Stop()
		{
			_navMeshAgent.destination = transform.position;
			_navMeshAgent.isStopped = true;
		}

		public void Walk()
		{
			SetSpeed(MoveSpeed * Config.Navigation.WalkMulti);
		}

		public void Run()
		{
			SetSpeed(MoveSpeed);
		}

		public void ToggleSneak()
		{
			if (!_isSneaking)
			{
				SetSpeed(MoveSpeed * Config.Navigation.CrouchMulti);
				_isSneaking = true;
			}
			else
			{
				SetSpeed(MoveSpeed);
				_isSneaking = false;
			}

			_animator.SetBool(Config.Animation.IsCrouched, _isSneaking);
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