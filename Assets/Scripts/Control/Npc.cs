using Messaging;
using RedDust.Messages;
namespace RedDust.Control
{
	public class Npc : Character
	{
		// The player Squad sends these messages when another Squad's status for it changes.
		// This is done so any NPC can set their Indicator accordingly, independetly of their own squad.
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

			SetIndicatorColor(msg.Status);
		}
	}
}