using UnityEngine;

namespace RedDust.Control.Actions
{
	/// <summary>
	/// Sets the Character's NavMesh path. Validity of the path should be done elsewhere to prevent a character
	/// from stopping if an impossible move action is given.
	/// </summary>
	public class MoveToAction : ActionBase
	{
		private Vector3 _destination;

		/// <summary>
		/// Create a new Move action.
		/// </summary>
		/// <param name="c">The character executing this order.</param>
		/// <param name="destination">The destination that will be passed to the NavMeshAgent of the character.</param>
		public MoveToAction(Character c, Vector3 destination) : base(c)
		{
			_destination = destination;
		}

		public override void OnStart()
		{
			base.OnStart();
			Character.Mover.SetDestination(_destination);
		}

		public override ActionState Execute()
		{
			if (Character.Mover.IsAtDestination())
			{
				return ActionState.Success;
			}

			return ActionState.Running;
		}

		public override void OnCancel()
		{
			base.OnCancel();
			Character.Mover.Stop();
		}
	}
}
