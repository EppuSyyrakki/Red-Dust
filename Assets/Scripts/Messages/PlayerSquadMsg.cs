using Messaging;
using RedDust.Control;

namespace RedDust.Messages
{
	public class PlayerSquadMsg : IMessage
	{
		public Squad Squad { get; }
		public SquadStatus Status { get; }

		public PlayerSquadMsg(Squad squad, SquadStatus status)
		{
			Squad = squad;
			Status = status;
		}
	}
}