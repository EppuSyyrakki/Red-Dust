using Messaging;
using RedDust.Control.Actions;
using RedDust.Messages;
using UnityEngine;

namespace RedDust.Control
{
	public class Npc : Character, IInteractable
	{
		/// <summary>
		/// The player Squad sends these messages when another Squad's status for it changes so any NPC
		/// can set their Hostility Indicator accordingly.
		/// </summary>
		private ISubscription<PlayerSquadMsg> statusSub = null;		

		private void OnEnable()
		{
			statusSub = Game.Instance.Bus.Subscribe<PlayerSquadMsg>(PlayerSquadMsgHandler);
		}

		private void OnDisable()
		{
			Game.Instance.Bus.Unsubscribe(statusSub);
		}

		private void PlayerSquadMsgHandler(PlayerSquadMsg msg)
		{
			if (msg.Squad != Squad) { return; }

			base.SetIndicatorColor(msg.Status);
		}

		#region IInteractable implementation

		public Transform LookTarget => Head;

		public Sprite GetIcon(Player p)
		{
			if (p.Squad.IsHostileTo(this)) { return ActionBase.LoadIcon(nameof(ShootAction)); }

			return ActionBase.LoadIcon(nameof(TalkToAction));
		}

		public ActionBase GetAction(Character c)
		{
			if (c.Squad.IsHostileTo(this))
			{
				return new ShootAction(c, Combat.Health);
			}

			return new TalkToAction(c, this);
		}

		#endregion
	}
}