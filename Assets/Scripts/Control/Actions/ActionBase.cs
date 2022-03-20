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
	/// The base class for all Actions in the game.
	/// </summary>
	[System.Serializable]
	public abstract class ActionBase
	{
		protected Character Character { get; private set; }

		protected ActionBase(Character character)
		{
			Character = character;
		}

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
			if (Character.LoggingEnabled)
			{
				Debug.Log(Character.gameObject.name + " cancelled " + GetType().Name);
			}
		}
	}
}
