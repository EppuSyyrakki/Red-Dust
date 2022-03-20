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

		public Type ActionType => typeof(FollowAction);
		public Sprite ActionIcon => interactionIcon;

		public ActionBase GetAction(Player p)
		{
			if (p == this) { return null; }

			if (p.Mover.HasPathTo(transform.position, out var path))
			{
				return new FollowAction(p, transform, path);
			}

			return null;
		}

		#endregion	
	}
}
