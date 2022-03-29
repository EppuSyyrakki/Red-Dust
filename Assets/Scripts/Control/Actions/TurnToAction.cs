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
			MakesCharacterBusy = false;
		}

		public override ActionState Execute()
		{
			if (Character.Motion.TurnTowards(target))
			{
				return ActionState.Success;
			}

			return ActionState.Running;
		}
	}
}