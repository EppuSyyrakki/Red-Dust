using System.Collections;
using UnityEngine;

namespace RedDust.Control.Actions
{
	public class TurnToAction : ActionBase
	{
		Vector3 _target;

		public TurnToAction(Character c, Vector3 target) : base(c)
		{
			_target = target;
		}

		public override ActionState Execute()
		{
			if (Character.Mover.TurnTowards(_target))
			{
				return ActionState.Success;
			}

			return ActionState.Running;
		}
	}
}