using Messaging;

namespace RedDust
{
	public class TacticalGame
	{
		private static TacticalGame _instance;

		public static TacticalGame Instance
		{
			get
			{
				if (_instance == null)
				{
					_instance = new TacticalGame();
				}

				return _instance;
			}
		}

		private TacticalGame()
		{
			Bus = new MessageBus();
		}

		/// <summary>
		/// The class for relaying messages and managing their subscriptions.
		/// </summary>
		public MessageBus Bus { get; }
	}
}
