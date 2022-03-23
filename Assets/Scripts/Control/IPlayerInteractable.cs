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
		Sprite GetIcon(Player p);

		/// <summary>
		/// Gets the action associated with this IInteractable. Should check if the action is possible.
		/// </summary>
		/// <param name="p">The Player interacting with this IInteractable.</param>
		/// <returns>The associated action. If the caller can't do this action, returns null.</returns>
		ActionBase GetAction(Player p);
	}
}