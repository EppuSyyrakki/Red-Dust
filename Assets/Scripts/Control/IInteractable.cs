using RedDust.Control.Actions;
using System.Collections;
using UnityEngine;

namespace RedDust.Control
{
	public interface IInteractable
	{
		Collider InteractionCollider { get; }
		/// <summary>
		/// The transform that should be looked at when near this interactable.
		/// </summary>
		Transform LookTarget { get; }

		/// <summary>
		/// Gets the action associated with this IInteractable. Should check if the action is possible.
		/// </summary>
		/// <param name="p">The Character interacting with this IInteractable.</param>
		/// <returns>The associated action. If the caller can't do this action, returns null.</returns>
		ActionBase GetAction(Character c);

		Sprite GetIcon(Player p);
	}
}