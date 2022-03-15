using RedDust.Movement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace RedDust.Control.Actions
{
	public class MoveToAction : ActionBase
	{
		private Mover _mover;
		private NavMeshPath _path;
		private bool _pathSet;

		public MoveToAction(CharacterControl character, NavMeshPath path) : base(character)
		{
			_mover = Character.Mover;
			_path = path;
		}

		public override ActionState Execute()
		{
			if (!_pathSet)
			{
				_mover.SetPath(_path);
				_pathSet = true;
			}

			if (_mover.IsAtDestination())
			{
				return ActionState.Success;
			}

			return ActionState.Running;
		}
	}
}
