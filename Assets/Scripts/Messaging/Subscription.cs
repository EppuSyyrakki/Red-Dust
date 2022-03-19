using System;

namespace Messaging
{
	public class Subscription<TMessage> : ISubscription<TMessage>
		where TMessage : IMessage
	{
		public Action<TMessage> Action
		{
			get;
		}

		public Subscription(Action<TMessage> action)
		{
			Action = action;
		}
	}
}