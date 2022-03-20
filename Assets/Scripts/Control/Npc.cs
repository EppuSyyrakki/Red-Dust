using Messaging;
using RedDust.Messages;
using System.Collections;
using UnityEngine;

namespace RedDust.Control
{
	public class Npc : Character
	{
		private ISubscription<PlayerSquadMsg> _statusSub = null;

		private void OnEnable()
		{
			_statusSub = Game.Instance.Bus.Subscribe<PlayerSquadMsg>(PlayerSquadMsgHandler);
		}

		private void OnDisable()
		{
			Game.Instance.Bus.Unsubscribe(_statusSub);
		}

		private void PlayerSquadMsgHandler(PlayerSquadMsg msg)
		{
			if (msg.Squad != Squad) { return; }

			SetIndicatorColor(msg.Status);
		}
	}
}