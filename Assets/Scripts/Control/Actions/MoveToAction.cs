using UnityEngine.AI;

namespace RedDust.Control.Actions
{
	/// <summary>
	/// Sets the Character's NavMesh path. Validity of the path should be done elsewhere to prevent a character
	/// from stopping if an impossible move action is given.
	/// </summary>
	public class MoveToAction : ActionBase
	{
		private NavMeshPath _path;

		/// <summary>
		/// Create a new Move action.
		/// </summary>
		/// <param name="c">The character executing this order.</param>
		/// <param name="path">The path that will be passed to the NavMeshAgent of the character.</param>
		public MoveToAction(Character c, NavMeshPath path) : base(c)
		{
			_path = path;
		}

		public override void OnStart()
		{
			base.OnStart();
			Character.Mover.SetPath(_path);
		}

		public override ActionState Execute()
		{
			if (Character.Mover.IsAtDestination())
			{
				return ActionState.Success;
			}

			return ActionState.Running;
		}
	}
}
