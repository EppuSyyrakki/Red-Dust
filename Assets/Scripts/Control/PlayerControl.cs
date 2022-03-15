using RedDust.Control.Actions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace RedDust.Control
{
	public class PlayerControl : CharacterControl
	{
		[SerializeField]
		private LayerMask ignoreMouseClicksOn;

		public override void Awake()
		{
			base.Awake();
		}

		private void Update()
		{
			InteractWithMovement();
			ExecuteAction();
		}

		private bool InteractWithMovement()
		{
			if (!RaycastNavMesh(out Vector3 target, out NavMeshPath path)) { return false; }

			if (Input.GetMouseButtonDown(0))
			{
				if (!Input.GetKey(KeyCode.LeftShift))
				{
					CancelActions();
				}

				AddAction(new MoveToAction(this, path));
			}

			return true;
		}

		private bool RaycastNavMesh(out Vector3 target, out NavMeshPath path)
		{
			target = new Vector3();
			path = new NavMeshPath();

			if (!Physics.Raycast(GetMouseRay(), out RaycastHit hit, 200f, ~ignoreMouseClicksOn)) { return false; }

			return Mover.HasPathTo(hit.point, ref target, ref path);
		}	

		private static Ray GetMouseRay()
		{
			return Camera.main.ScreenPointToRay(Input.mousePosition);
		}
	}
}
