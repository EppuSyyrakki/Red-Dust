using RedDust.Control.Actions;
using System;
using UnityEngine;

namespace RedDust.Control
{
	public class Player : Character, IPlayerInteractable
	{
		[SerializeField]
		private Sprite interactionIcon;

		public Transform Transform => transform;

		#region Unity messages

		public override void Awake()
		{
			base.Awake();
		}

		private void Update()
		{
			base.ExecuteAction();
		}

		#endregion

		#region IPlayerInteractable implementation

		public Sprite GetIcon()
		{
			return interactionIcon;
		}

		public ActionBase GetAction(Player p)
		{
			if (p == this) { return null; }

			if (p.Mover.IsPointOnNavMesh(transform.position, out _))
			{
				return new FollowAction(p, transform);
			}

			return null;
		}

		#endregion	
	}
}
