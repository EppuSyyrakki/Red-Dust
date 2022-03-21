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
		private float _updateTargetTimer;
		private float _updateInterval;

		/// <summary>
		/// Create a new Follow action.
		/// </summary>
		/// <param name="c">The character executing this order.</param>
		/// <param name="target">The transform to follow.</param>
		/// <param name="path">The path to the target. Will be updated at specific intervals.</param>
		public FollowAction(Character c, Transform target) : base(c)
		{
			_target = target;
			_updateInterval =  Game.Navigation.FollowUpdateInterval;
			_updateTargetTimer = Random.Range(0, _updateInterval);
		}

		public override void OnStart()
		{
			base.OnStart();			
			Character.Mover.SetStoppingDistance(Game.Navigation.AgentStoppingDistanceFollow);
			Character.Mover.SetDestination(_target.position);
		}

		public override ActionState Execute()
		{
			_updateTargetTimer += Time.deltaTime;

			if (_updateTargetTimer > _updateInterval)
			{
				if (Character.Mover.GetPathStatus() != NavMeshPathStatus.PathComplete)
				{
					return ActionState.Failure;
				}

				Character.Mover.SetDestination(_target.position);
				_updateTargetTimer = 0;
			}
			
			return ActionState.Running;
		}

		public override void OnFailure()
		{
			base.OnFailure();
			Character.Mover.SetStoppingDistance(Game.Navigation.AgentStoppingDistance);
			Character.Mover.Stop();
		}

		public override void OnCancel()
		{
			base.OnCancel();
			Character.Mover.SetStoppingDistance(Game.Navigation.AgentStoppingDistance);
			Character.Mover.Stop();
		}
	}
}