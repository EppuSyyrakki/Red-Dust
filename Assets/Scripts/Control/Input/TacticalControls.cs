using RedDust.Control.Actions;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using static RedDust.Control.Input.GameInputs;

namespace RedDust.Control.Input
{
	public class TacticalControls : MonoBehaviour, ITacticalActions
	{
		public enum DragMode { None, LookDirection, Selection, UI }

		[SerializeField]
		private bool logInput;

		[SerializeField]
		private LayerMask interactionLayers;

		[SerializeField]
		private List<Player> _selected = new List<Player>();
		private List<Player> _allCharacters = new List<Player>();

		private PlayerInput _playerInput;
		private GameInputs _gameInputs;

		private Vector2 _cursorPosition;
		private bool _addModifier = false;
		private bool _chainModifier = false;
		private RaycastHit[] _cursorCast = new RaycastHit[32];
		private DragMode _dragMode = DragMode.None;
		private Type _raycastActionType = null;
		private IPlayerInteractable _currentInteractable = null;

		public Vector2 CursorPosition => _cursorPosition;
		public event Action SelectionBox;
		public event Action<Type> ShowActionIcon;
		public event Action HideActionIcon;

		#region Unity messages

		private void Awake()
		{
			_playerInput = GetComponent<PlayerInput>();
			_playerInput.camera = Camera.main;
			// Construct the auto-generated wrapper
			_gameInputs = new GameInputs();
			// Set "menu" map as default for setting the UI
			_playerInput.defaultActionMap = _gameInputs.Menu.Get().name;
			_playerInput.actions = _gameInputs.asset;
			// find the UI module and set the generated wrapper as its asset
			var uiInputModule = EventSystem.current.gameObject.GetComponent<InputSystemUIInputModule>();
			uiInputModule.actionsAsset = _gameInputs.asset;
			// assign the UI input module to the PlayerInput component
			_playerInput.uiInputModule = uiInputModule;			
		}	

		private void OnEnable()
		{
			_playerInput.ActivateInput();
			_gameInputs.Tactical.SetCallbacks(this);
		}

		private void OnDisable()
		{
			_playerInput.DeactivateInput();
		}

		private void Start()
		{
			SwitchInputToTactical();
		}

		private void Update()
		{
			// No selected players means no action can be added
			if (_selected.Count == 0) { return; }  

			if (Physics.Raycast(GetCursorRay(_cursorPosition), out RaycastHit hit,
				Config.Input.MouseCastRange, interactionLayers)
				&& hit.collider.TryGetComponent(out _currentInteractable))
			{
				Type newActionType = _currentInteractable.GetActionType();

				if (_raycastActionType == null || _raycastActionType != newActionType)
				{
					_raycastActionType = newActionType;
					ShowActionIcon?.Invoke(_raycastActionType);
					return;
				}
			}

			// No interactable from cursor ray found
			HideActionIcon?.Invoke();
			_raycastActionType = null;
		}

		#endregion

		#region Input events and controls

		public void OnMoveCursor(InputAction.CallbackContext ctx)
		{
			if (ctx.performed)
			{
				_cursorPosition = ctx.ReadValue<Vector2>(); 
			}
		}

		public void OnMoveOrSelect(InputAction.CallbackContext ctx)
		{
			if (ctx.phase != InputActionPhase.Canceled || _dragMode != DragMode.None) { return; }

			var ray = GetCursorRay(_cursorPosition);
		
			if (!Physics.Raycast(ray, out RaycastHit hit, Config.Input.MouseCastRange, Config.Layers.Character)) 
			{
				// If we don't hit anything in the Character layer, it's supposed to be a move order
				TryMoveSelectedToCursor(ray);
				return;
			}

			if (hit.collider.TryGetComponent(out Player p))
			{
				ModifySelection(p);	
			}
		}

		public void OnInteract(InputAction.CallbackContext ctx)
		{
			if (ctx.phase != InputActionPhase.Performed 
				|| _selected.Count == 0
				|| _raycastActionType == null) { return; }
			
			for (int i = 0; i < _selected.Count; i++)
			{
				var player = _selected[i];
				player.AddAction(_currentInteractable.GetAction(player));
			}			
		}

		public void OnAddModifier(InputAction.CallbackContext ctx)
		{
			if (ctx.phase == InputActionPhase.Performed) { _addModifier = true; }
			else if (ctx.phase == InputActionPhase.Canceled) { _addModifier = false; }

			if (logInput && ctx.phase != InputActionPhase.Started) { Debug.Log("Add button: " + _addModifier); }			
		}

		public void OnChainModifier(InputAction.CallbackContext ctx)
		{
			if (ctx.phase == InputActionPhase.Performed) { _chainModifier = true; }
			else if (ctx.phase == InputActionPhase.Canceled) { _chainModifier = false; }

			if (logInput && ctx.phase != InputActionPhase.Started) { Debug.Log("Chain button: " + _chainModifier); }
		}

		public void OnDrag(InputAction.CallbackContext ctx)
		{
			//if (ctx.phase == InputActionPhase.Started) { return; }

			//if (ctx.phase == InputActionPhase.Performed)
			//{
			//	if (_addModifier)
			//	{
			//		_dragMode = DragMode.Selection;
			//		SelectionBox?.Invoke();
			//	}

			//	StartCoroutine(DrawLookDirection());
			//	// start drawing a lookdirection
			//}
			//else	// ctx.phase.Canceled
			//{
				
			//	// finish dragging
			//}

			//if (logInput) { Debug.Log("Dragging: " + ctx.phase); }
		}

		public void SwitchInputToMenu()
		{
			_playerInput.currentActionMap = _gameInputs.Menu.Get();
		}

		public void SwitchInputToTactical()
		{
			_playerInput.currentActionMap = _gameInputs.Tactical.Get();
		}

		#endregion

		private bool TryMoveSelectedToCursor(Ray cursorRay)
		{
			var paths = RaycastNavMesh(cursorRay, out NavMeshPath[] path);

			for (int i = 0; i < _selected.Count; i++)
			{
				if (!paths[i]) { continue; }    // The _selected[i] doesn't have a path 

				if (!_chainModifier) { _selected[i].CancelActions(); }
				
				_selected[i].AddAction(new MoveToAction(_selected[i], path[i]));
			}

			return true;
		}

		/// <summary>
		/// Finds paths to all Players in _selected to a point raycast with cursor position.
		/// </summary>
		/// <param name="paths">The NavMeshPaths for _selected with matching indices.</param>
		/// <returns>Array of bools denoting if path was found for _selected with matching indices.</returns>
		private bool[] RaycastNavMesh(Ray mouseRay, out NavMeshPath[] paths)
		{
			bool[] havePaths = new bool[_selected.Count];

			paths = new NavMeshPath[_selected.Count];
			int layer = Config.Layers.Ground;

			if (!Physics.Raycast(mouseRay, out RaycastHit hit, 200f, layer)) { return havePaths; }

			for (int i = 0; i < _selected.Count; i++)
			{
				paths[i] = new NavMeshPath();
				havePaths[i] = _selected[i].Mover.HasPathTo(hit.point, out paths[i]);
			}

			return havePaths;
		}

		/// <summary>
		/// Depending on the state of _addModifier and if _selected contains p, adds p to selection,  
		/// removes it, or clears the _selected and adds p.
		/// </summary>
		/// <param name="p"></param>
		private void ModifySelection(Player p)
		{
			if (_addModifier)
			{
				if (_selected.Contains(p))
				{
					_selected.Remove(p);
					return;
				}

				_selected.Add(p);
				return;
			}

			_selected.Clear();
			_selected.Add(p);
		}

		private void SetSelection(List<Player> newSelection)
		{
			_selected = newSelection;
		}

		//private IEnumerator DrawLookDirection()
		//{
		//	while (_dragMode == DragMode.LookDirection)
		//	{
		//		if (GetWorldPosition(_cursorPosition, out var worldPosition))
		//		{

		//		}

		//		yield return null;
		//	}	
		//}

		private static bool GetWorldPosition(Vector2 cursorPosition, out Vector3 worldPosition)
		{
			Ray ray = GetCursorRay(cursorPosition);
			worldPosition = new Vector3();
			
			if (Physics.Raycast(ray, out RaycastHit hit, Config.Input.MouseCastRange, Config.Layers.Ground))
			{
				worldPosition = hit.point;
				return true;
			}

			return false;
		}

		private static Ray GetCursorRay(Vector2 cursorPosition)
		{
			return Camera.main.ScreenPointToRay(cursorPosition);
		}
	}
}