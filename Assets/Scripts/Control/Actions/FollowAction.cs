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
		private Transform target;
		private float updateTargetTimer;
		private float updateInterval;

		/// <summary>
		/// Create a new Follow action.
		/// </summary>
		/// <param name="c">The character executing this order.</param>
		/// <param name="target">The transform to follow.</param>
		/// <param name="path">The path to the target. Will be updated at specific intervals.</param>
		public FollowAction(Character c, Transform target) : base(c)
		{
			this.target = target;
			updateInterval =  Values.Navigation.FollowUpdateInterval;
			updateTargetTimer = Random.Range(0, updateInterval);
			MakesCharacterBusy = false;
		}

		public override void OnStart()
		{
			base.OnStart();			
			Character.Motion.SetStoppingDistance(Values.Navigation.AgentStoppingDistanceFollow);
			Character.Motion.SetDestination(target.position);
		}

		public override ActionState Execute()
		{
			updateTargetTimer += Time.deltaTime;

			if (updateTargetTimer > updateInterval)
			{
				if (Character.Motion.GetPathStatus() != NavMeshPathStatus.PathComplete)
				{
					return ActionState.Failure;
				}

				Character.Motion.SetDestination(target.position);
				updateTargetTimer = 0;
			}
			
			return ActionState.Running;
		}

		public override void OnFailure()
		{
			base.OnFailure();
			Character.Motion.SetStoppingDistance(Values.Navigation.AgentStoppingDistance);
			Character.Motion.Stop();
		}

		public override void OnCancel()
		{
			base.OnCancel();
			Character.Motion.SetStoppingDistance(Values.Navigation.AgentStoppingDistance);
			Character.Motion.Stop();
		}
	}
}