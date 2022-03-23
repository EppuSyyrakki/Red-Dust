﻿using RedDust.Combat;
using RedDust.Control.Actions;
using RedDust.Movement;
using System.Collections.Generic;
using UnityEngine;

namespace RedDust.Control
{
	[RequireComponent(typeof(Mover))]
	public abstract class Character : MonoBehaviour
	{
		[SerializeField]
		private bool logActions = false;

		private Queue<ActionBase> actionQueue = new Queue<ActionBase>();
		private ActionBase currentAction;
		private ActionState actionState;

		protected HostilityIndicator Indicator { get; private set; }

		public bool LoggingEnabled => logActions;
		public Mover Mover { get; private set; }
		public Fighter Fighter { get; private set; }
		public Squad Squad { get; private set; }

		#region Unity messages

		public virtual void Awake()
		{
			Mover = GetComponent<Mover>();
			Fighter = GetComponent<Fighter>();
			Indicator = GetComponentInChildren<HostilityIndicator>();
			AddAction(new IdleAction(this));
		}

		private void Start()
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

			if (status == SquadStatus.Friendly)
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