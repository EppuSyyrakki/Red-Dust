using UnityEngine;
using UnityEngine.AI;

namespace RedDust.Control.Actions
{
	/// <summary>
	/// Will only return a failure if there's no path to the follow target. Never returns a success.
	/// </summary>
	[System.Serializable]
	public class FollowAction : ActionBase
	{
		private Transform _target;
		private NavMeshPath _pathToTarget;	
		private float _updateTargetTimer;
		private float _updateInterval;

		/// <summary>
		/// Create a new Follow action.
		/// </summary>
		/// <param name="c">The character executing this order.</param>
		/// <param name="target">The transform to follow.</param>
		/// <param name="path">The path to the target. Will be updated at specific intervals.</param>
		public FollowAction(Character c, Transform target, NavMeshPath path) : base(c)
		{
			_target = target;
			_pathToTarget = path;
			_updateInterval = Config.AI.FollowUpdateInterval;
			_updateTargetTimer = Random.Range(0, _updateInterval);
		}

		public override void OnStart()
		{
			base.OnStart();
			Character.Mover.SetPath(_pathToTarget);
		}

		public override ActionState Execute()
		{
			_updateTargetTimer += Time.deltaTime;

			if (_updateTargetTimer > _updateInterval)
			{
				Vector3 followPosition = _target.position - _target.forward * Config.AI.FollowDistance;

				if (Character.Mover.HasPathTo(followPosition, out NavMeshPath path))
				{
					Character.Mover.SetPath(path);
					_updateTargetTimer = 0;
				}
				else
				{
					return ActionState.Failure;
				}			
			}
			
			return ActionState.Running;
		}
	}
}