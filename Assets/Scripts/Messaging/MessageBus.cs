using System;
using System.Collections;
using System.Collections.Generic;

namespace Messaging
{
	public class MessageBus : IMessageBus
	{
		// Key: Type of the message
		// Value: List of subscriptions related to the type
		private readonly IDictionary<Type, IList> _subscriptions =
			new Dictionary<Type, IList>();

		public void Send<TMessage>(TMessage message) where TMessage : IMessage
		{
			if (message == null)
			{
				throw new ArgumentNullException(nameof(message));
			}

			Type messageType = typeof(TMessage);
			if (_subscriptions.ContainsKey(messageType))
			{
				// There are subscriptions, let's send the message to receivers.

				// Fetch the list that contains subscriptions
				IList subscriptionList = _subscriptions[messageType];
				foreach (var obj in subscriptionList)
				{
					ISubscription<TMessage> subscription = obj as ISubscription<TMessage>;
					subscription?.Action(message);
				}
			}
		}

		public ISubscription<TMessage> Subscribe<TMessage>(Action<TMessage> action)
			where TMessage : IMessage
		{
			// The type of the message is stored in this variable.
			Type messageType = typeof(TMessage);

			ISubscription<TMessage> subscription = new Subscription<TMessage>(action);

			if (!_subscriptions.ContainsKey(messageType))
			{
				// SubscriptionList contains all the subscriptions for the type TMessage.
				List<ISubscription<TMessage>> subscriptionList = new List<ISubscription<TMessage>>();
				_subscriptions.Add(messageType, subscriptionList);
			}

			_subscriptions[messageType].Add(subscription);

			return subscription;
		}

		public bool Unsubscribe<TMessage>(ISubscription<TMessage> subscription)
			where TMessage : IMessage
		{
			Type messageType = typeof(TMessage);

			if (_subscriptions.ContainsKey(messageType))
			{
				if (_subscriptions[messageType].Contains(subscription))
				{
					_subscriptions[messageType].Remove(subscription);
					return true;
				}
			}

			return false;
		}
	}
}