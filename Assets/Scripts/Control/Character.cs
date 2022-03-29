using RedDust.Combat;
using RedDust.Control.Actions;
using RedDust.Motion;
using System.Collections.Generic;
using UnityEngine;
using Utils;

namespace RedDust.Control
{
	[RequireComponent(typeof(MotionControl), typeof(CombatControl))]
	public abstract class Character : MonoBehaviour
	{
		public bool enableLogging = false;

		private Queue<ActionBase> actionQueue = new Queue<ActionBase>();
		private ActionBase currentAction;
		private ActionState actionState;

		protected HostilityIndicator Indicator { get; private set; }
	
		public MotionControl Motion { get; private set; }
		public CombatControl Fighter { get; private set; }
		public Squad Squad { get; private set; }
        
        public Transform Head { get; private set; }
		public Collider InteractionCollider { get; private set; }

		public bool IsBusy
		{
			get 
			{ 
				if (currentAction != null) { return currentAction.MakesCharacterBusy; }
				return false;
			}
		}

		#region Unity messages

		public virtual void Awake()
		{
            Motion = GetComponent<MotionControl>();
			Fighter = GetComponent<CombatControl>();			
			Indicator = GetComponentInChildren<HostilityIndicator>();						
			Head = transform.FindObjectWithTag(Values.Tag.Head).transform;
			InteractionCollider = GetComponent<Collider>();
			AddAction(new IdleAction(this));
		}

		public virtual void Start()
		{
			if (Squad == null) 
			{ 
				Debug.LogError(gameObject.name + " has no Squad! Parent must have a Squad component."); 
			}
		}

		#endregion

		protected void ExecuteAction()
		{
			if (currentAction == null) 
			{ 
				currentAction = GetNextAction();
				currentAction.OnStart();
				return;	// TODO: Is this return necessary? Written as a safety measure.
			}

			actionState = currentAction.Execute();

			if (actionState == ActionState.Success)
			{
				currentAction.OnSuccess();
				currentAction = null;
			}
			else if (actionState == ActionState.Failure)
			{
				currentAction.OnFailure();
				currentAction = null;
			}
		}

		private ActionBase GetNextAction()
		{
			if (actionQueue.Count == 0)
			{
				return new IdleAction(this);
			}
			else
			{
				return actionQueue.Dequeue();
			}
		}

		#region Public API

		public void AddAction(ActionBase action)
		{
			if (action != null)
			{
				actionQueue.Enqueue(action);
			}		

			if (actionState == ActionState.Idle)
			{
				currentAction = null;
			}
		}

		public void CancelActions()
		{
			currentAction.OnCancel();
			currentAction = null;
			actionQueue.Clear();
		}

		public void SetSquad(Squad s)
		{
			Squad = s;
		}

		public void SetIndicatorColor(SquadStatus status)
		{
			Color color = Values.Color.Neutral;

			if (status == SquadStatus.Allied)
			{
				color = Values.Color.Friendly;
			}
			else if (status == SquadStatus.Hostile)
			{
				color = Values.Color.Hostile;
			}
			else if (status == SquadStatus.Player)
			{
				color = Values.Color.Player;
			}

			Indicator.SetColor(color);
		}

		#endregion
	}
}