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
		private bool _useIndicator;

		/// <summary>
		/// Create a new Move action.
		/// </summary>
		/// <param name="c">The character executing this order.</param>
		/// <param name="destination">The destination that will be passed to the NavMeshAgent of the character.</param>
		/// <param name="useIndicator">Player characters can true this to show their movement indicator.</param>
		public MoveToAction(Character c, Vector3 destination, bool useIndicator = false) : base(c)
		{
			_destination = destination;
			_useIndicator = useIndicator;
		}

		public override void OnStart()
		{
			base.OnStart();
			Character.Mover.SetDestination(_destination, _useIndicator);			
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
