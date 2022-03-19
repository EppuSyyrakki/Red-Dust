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
		protected Character Character { get; private set; }
		public string Name => GetType().Name;

		protected ActionBase(Character character)
		{
			Character = character;
		}

		public abstract ActionState Execute();

		public virtual void OnStart()
		{
			if (Character.LoggingEnabled)
			{
				Debug.Log(Character.gameObject.name + " started " + Name);
			}
		}

		public virtual void OnSuccess()
		{
			if (Character.LoggingEnabled)
			{
				Debug.Log(Character.gameObject.name + " succeeded in " + Name);
			}
		}

		public virtual void OnFailure()
		{
			if (Character.LoggingEnabled)
			{
				Debug.Log(Character.gameObject.name + " failed in " + Name);
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
