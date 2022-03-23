using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace RedDust.Control.Actions
{
	public class TalkToAction : ActionBase
	{
		private Character target;
		private float updateTargetTimer;
		private float updateInterval;
		private bool useIndicator;

		// Temporary for testing
		private float talkTime = 0;

		public TalkToAction(Character c, Character target, bool useIndicator = false) : base(c)
		{
			this.target = target;
			updateInterval = Values.Navigation.FollowUpdateInterval;
			this.useIndicator = useIndicator;
		}

		public override void OnStart()
		{
			base.OnStart();
			updateTargetTimer = 0;
			Character.Mover.SetDestination(GetTalkPosition(), useIndicator);
		}

		public override ActionState Execute()
		{
			if (target.IsBusy) { return ActionState.Failure; }

			if (updateTargetTimer > updateInterval)
			{
				// TEST
				if (Character.Mover.IsAtDestination())
				{
					talkTime += updateTargetTimer;
					Debug.Log($"{Character.name} is talking to {target.name}");
				}

				if (talkTime > 5f) { return ActionState.Success; }
				// TEST
			
				if (Character.Mover.GetPathStatus() != NavMeshPathStatus.PathComplete)
				{
					return ActionState.Failure;
				}

				Character.Mover.SetDestination(target.transform.position);
				updateTargetTimer = 0;
			}

			return ActionState.Running;
		}

		private Vector3 GetTalkPosition()
		{
			var self = Character.transform.position;
			var other = target.transform.position;
			var offset = (self - other).normalized * Values.Navigation.GroupMoveRange;
			return other + offset;
		}
	}
}
