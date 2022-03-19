using RedDust.Combat;
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

		private Queue<ActionBase> _actionQueue = new Queue<ActionBase>();
		private ActionBase _currentAction;
		private ActionState _actionState;

		public bool LoggingEnabled => logActions;
		public Mover Mover { get; private set; }
		public Fighter Fighter { get; private set; }
		public Squad Squad { get; private set; }

		#region Unity messages

		public virtual void Awake()
		{
			Mover = GetComponent<Mover>();
			Fighter = GetComponent<Fighter>();
			AddAction(new IdleAction(this));
		}

		#endregion

		protected void ExecuteAction()
		{
			if (_currentAction == null) 
			{ 
				_currentAction = GetNextAction();
				_currentAction.OnStart();
				return;	// TODO: Is this return necessary? Written as a safety measure.
			}

			_actionState = _currentAction.Execute();

			if (_actionState == ActionState.Success)
			{
				_currentAction.OnSuccess();
				_currentAction = null;
			}
			else if (_actionState == ActionState.Failure)
			{
				_currentAction.OnFailure();
				_currentAction = null;
			}
		}

		private ActionBase GetNextAction()
		{
			if (_actionQueue.Count == 0)
			{
				return new IdleAction(this);
			}
			else
			{
				return _actionQueue.Dequeue();
			}
		}

		#region Public API

		public void AddAction(ActionBase action)
		{
			_actionQueue.Enqueue(action);

			if (_actionState == ActionState.Idle)
			{
				_currentAction = null;
			}
		}

		public void CancelActions()
		{
			_currentAction.OnCancel();
			_currentAction = null;
			_actionQueue.Clear();
		}

		public void SetSquad(Squad s)
		{
			Squad = s;
		}

		#endregion
	}
}