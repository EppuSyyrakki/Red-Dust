using RedDust.Control.Actions;
using System;
using UnityEngine;

namespace RedDust.Control
{
	/// <summary>
	/// Interface that any Player uses to interact with the world.
	/// </summary>
	public interface IPlayerInteractable
	{
		Transform Transform { get; }

		/// <summary>
		/// Gets the action associated with this IInteractable.
		/// </summary>
		/// <param name="p">The Player interacting with this IInteractable.</param>
		/// <returns>The associated action. If the caller can't do this action, returns null.</returns>
		ActionBase GetAction(Player p);

		Type GetActionType(); 
	}
}