using RedDust.Control.Actions;
using UnityEngine;

namespace RedDust.Control
{
	public class Player : Character, IPlayerInteractable
	{
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

		public ActionBase GetAction(Player p)
		{
			if (p.Mover.HasPathTo(transform.position, out var path))
			{
				return new FollowAction(p, transform, path);
			}

			return null;
		}

		#endregion	
	}
}
