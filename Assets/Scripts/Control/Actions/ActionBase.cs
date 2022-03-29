using System.Collections.Generic;
using UnityEngine;

namespace RedDust.Control.Actions
{
	public enum ActionState
	{
		Running,
		Success,
		Failure,
		Idle
	}

	/// <summary>
	/// The base class for all Actions in the game. No action should be checking its own viability
	/// before construction, this should be done by Character before assigning the action to them.
	/// </summary>
	[System.Serializable]
	public abstract class ActionBase
	{
		protected ActionBase(Character character)
		{
			Character = character;
		}		

		private static Dictionary<string, Sprite> iconLookup;

		protected Character Character { get; private set; }

		public bool MakesCharacterBusy { get; protected set; } = true;

		/// <summary>
		/// Tries to lookup a Sprite for the given action name. If not found, tries to Load the icon
		/// from resources to the lookup.
		/// </summary>
		public static Sprite LoadIcon(string actionName)
		{
			if (iconLookup == null) { iconLookup = new Dictionary<string, Sprite>(); }

			if (iconLookup.TryGetValue(actionName, out Sprite icon)) { return icon; }

			var path = Values.Path.ActionIcons + actionName;
			var loadedIcon = Resources.Load(path, typeof(Sprite)) as Sprite;

			if (loadedIcon == null) { Debug.LogWarning("No Sprite icon found at " + path); }

			iconLookup.Add(actionName, loadedIcon);
			return loadedIcon;
		}		

		/// <summary>
		/// Gets Called each frame by the character action queue. Should return different ActionStates
		/// depending on the results of that frame's execution.
		/// </summary>
		/// <returns>Enum used to decide if a new action should be pulled from the queue.</returns>
		public abstract ActionState Execute();

		/// <summary>
		/// Called by Character immediately before Execute() is called for the first time.
		/// </summary>
		public virtual void OnStart()
		{
			if (Character.LoggingEnabled)
			{
				Debug.Log(Character.gameObject.name + " started " + GetType().Name);
			}
		}

		/// <summary>
		/// Called by Character immediately after Execute() returns ActionState.Success.
		/// </summary>
		public virtual void OnSuccess()
		{
			OnEnd();

			if (Character.LoggingEnabled)
			{
				Debug.Log(Character.gameObject.name + " succeeded in " + GetType().Name);
			}
		}

		/// <summary>
		/// Called by Character immediately after Execute() returns ActionState.Failure.
		/// </summary>
		public virtual void OnFailure()
		{
			OnEnd();

			if (Character.LoggingEnabled)
			{
				Debug.Log(Character.gameObject.name + " failed in " + GetType().Name);
			}
		}

		/// <summary>
		/// Called by Character when its Action Queue gets cancelled (cleared).
		/// </summary>
		public virtual void OnCancel()
		{
			OnEnd();

			if (Character.LoggingEnabled)
			{
				Debug.Log(Character.gameObject.name + " cancelled " + GetType().Name);
			}		
		}

		/// <summary>
		/// Gets called in base.OnSuccess, OnFailure and OnCancel. Can be used to reduce repetition in those.
		/// </summary>
		public virtual void OnEnd()
		{
		}
	}
}
