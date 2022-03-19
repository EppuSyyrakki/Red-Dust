using System;

namespace Messaging
{
	/// <summary>
	/// A generic interface for message subscriptions. The related message has to implement the IMessage
	/// interface.
	/// </summary>
	/// <typeparam name="TMessage">The type of the message</typeparam>
	public interface ISubscription<TMessage>
		where TMessage : IMessage
	{
		Action<TMessage> Action { get; }
	}
}