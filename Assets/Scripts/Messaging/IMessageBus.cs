using System;

namespace Messaging
{
	public interface IMessageBus
	{
		/// <summary>
		/// Sends the message to all recipients.
		/// </summary>
		/// <param name="message">The message that will be sent.</param>
		/// <typeparam name="TMessage">The type of the message.</typeparam>
		void Send<TMessage>(TMessage message)
			where TMessage : IMessage;

		/// <summary>
		/// Registers message listener.
		/// </summary>
		/// <param name="action">The action receiver should execute when message arrives.</param>
		/// <typeparam name="TMessage">The type of the message.</typeparam>
		/// <returns>The subscription</returns>
		ISubscription<TMessage> Subscribe<TMessage>(Action<TMessage> action)
			where TMessage : IMessage;

		/// <summary>
		/// Unregisters from listening to the messages of type TMessage.
		/// </summary>
		/// <param name="subscription">The subscription object Subscribe has returned.</param>
		/// <typeparam name="TMessage">The type of the message.</typeparam>
		/// <returns>True, if unsubscribed successfully, false otherwise.</returns>
		bool Unsubscribe<TMessage>(ISubscription<TMessage> subscription)
			where TMessage : IMessage;
	}
}