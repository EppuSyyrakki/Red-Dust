using System.Collections;
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

	public abstract class ActionBase
	{
		protected CharacterControl Character { get; private set; }

		protected ActionBase(CharacterControl character)
		{
			Character = character;
		}

		public abstract ActionState Execute();

		public virtual void OnSuccess()
		{
			if (Character.LoggingEnabled)
			{
				Debug.Log(Character.gameObject.name + " succeeded in " + this);
			}
		}

		public virtual void OnFailure()
		{
			if (Character.LoggingEnabled)
			{
				Debug.Log(Character.gameObject.name + " failed in " + this);
			}
		}

		public virtual void OnCancel()
		{
			if (Character.LoggingEnabled)
			{
				Debug.Log(Character.gameObject.name + " cancelled " + this);
			}
		}
	}
}
