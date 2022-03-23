using System.Collections;
using UnityEngine;

namespace RedDust.Control.Actions
{
	public class TurnToAction : ActionBase
	{
		Vector3 target;

		public TurnToAction(Character c, Vector3 target) : base(c)
		{
			this.target = target;
		}

		public override ActionState Execute()
		{
			if (Character.Mover.TurnTowards(target))
			{
				return ActionState.Success;
			}

			return ActionState.Running;
		}
	}
}