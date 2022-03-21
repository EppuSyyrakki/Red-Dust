using RedDust.Control.Actions;
using System;
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

		/// <summary>
		/// Don't modify this List directly, use the public methods in the "Selection modification" region.
		/// They make sure the selection indicators follow along.
		/// </summary>
		private List<Player> _selected = new List<Player>();

		private PlayerInput _playerInput;
		private GameInputs _gameInputs;

		private Vector2 _cursorPosition;
		private bool _addModifier = false;
		private bool _forceAttack = false;
		private IPlayerInteractable _interactable = null;

		public DragMode Drag { get; private set; }
		public Squad PlayerSquad { get; private set; }
		public Vector2 CursorPosition => _cursorPosition;
		public event Action SelectionBoxStarted;
		public event Action SelectionBoxEnded;
		public event Action<Sprite> InteractableChanged;
		public event Action InteractableNulled;

		#region Unity messages

		private void Awake()
		{
			PlayerSquad = GetComponent<Squad>();
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

			Ray ray = GetCameraRay(_cursorPosition);

			// 1. UI check - if true, (for safety, null _currentinteractable?) return
			// 2. some activated ability check? Like first aid
			// 3. Interactable check

			if (TryGetInteractable(ray, out IPlayerInteractable newInteractable)) 
			{
				TryChangeInteractable(newInteractable);
			}
			else
			{
				NullInteractable();
			}
		}

		#endregion

		#region General private methods

		private bool TryGetInteractable(Ray cursorRay, out IPlayerInteractable newInteractable)
		{
			newInteractable = null;

			if (Physics.Raycast(cursorRay, out RaycastHit hit, Game.Input.CursorCastRange, interactionLayers)
				&& hit.collider.TryGetComponent(out newInteractable))
			{
				if (newInteractable is Player p && _selected.Contains(p)) 
				{
					// Don't allow player to interact with itself
					return false; 
				}

				return true;
			}
			
			// No interactable from cursor ray found				
			return false;			
		}

		private void TryChangeInteractable(IPlayerInteractable newInteractable)
		{
			if (_interactable == null || newInteractable != _interactable)
			{
				_interactable = newInteractable;
				InteractableChanged?.Invoke(_interactable.GetIcon());

				if (logInput) { Debug.Log(name + " current interactable is: " + _interactable.GetType().Name); }
			}
		}

		private void NullInteractable()
		{
			InteractableNulled?.Invoke();
			_interactable = null;

			if (logInput) { Debug.Log(name + " current interactable was nulled"); }
		}

		private bool MoveSelectedToCursor(Ray cursorRay)
		{
			for (int i = 0; i < _selected.Count; i++)
			{
				if (!RaycastNavMesh(cursorRay, out RaycastHit rHit)) { continue; }

				if (!SampleNavMesh(rHit.point, out NavMeshHit nHit)) { continue; }		

				if (!_addModifier) { _selected[i].CancelActions(); }

				_selected[i].AddAction(new MoveToAction(_selected[i], nHit.position, true));
			}

			return true;
		}

		#endregion

		#region Selection modification

		/// <summary>
		/// Depending on the state of _addModifier and if _selected contains p, adds p to selection,  
		/// removes it, or clears the _selected and adds p.
		/// </summary>
		public void ModifySelection(Player p)
		{
			if (_addModifier)
			{
				if (_selected.Contains(p))
				{
					RemoveFromSelection(p);
					return;
				}

				AddToSelection(p);
				return;
			}

			ClearSelection();
			AddToSelection(p);
		}

		public void ClearSelection()
		{
			foreach (var p in _selected) { p.SetIndicatorSelected(false); }
			_selected.Clear();
		}

		public void SetSelection(List<Player> newSelection)
		{
			ClearSelection();
			_selected = newSelection;
			foreach (var p in _selected) { p.SetIndicatorSelected(true); }
		}

		private void AddToSelection(Player p)
		{
			_selected.Add(p);
			p.SetIndicatorSelected(true);
		}

		private void RemoveFromSelection(Player p)
		{
			_selected.Remove(p); 
			p.SetIndicatorSelected(false);
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
			if (ctx.phase != InputActionPhase.Canceled || Drag != DragMode.None) { return; }

			var ray = GetCameraRay(_cursorPosition);
		
			if (!Physics.Raycast(ray, out RaycastHit hit, Game.Input.CursorCastRange, Game.Layer.Character)) 
			{
				// If we don't hit anything in the Character layer, it's supposed to be a move order
				MoveSelectedToCursor(ray);

				if (logInput) { Debug.Log(name + " MoveOrSelect released - Move"); }
				return;
			}

			if (hit.collider.TryGetComponent(out Player p))
			{
				ModifySelection(p);
				if (logInput) { Debug.Log(name + " MoveOrSelect released - Select"); }
			}
		}

		public void OnInteract(InputAction.CallbackContext ctx)
		{
			if (ctx.phase != InputActionPhase.Canceled
				|| _selected.Count == 0
				|| _interactable == null) { return; }
			
			for (int i = 0; i < _selected.Count; i++)
			{
				var player = _selected[i];

				if (!_addModifier) { player.CancelActions(); }

				player.AddAction(_interactable.GetAction(player));

				if (logInput) { Debug.Log(name + " Interact button released"); }
			}		
		}

		public void OnAddModifier(InputAction.CallbackContext ctx)
		{
			if (ctx.phase == InputActionPhase.Performed) { _addModifier = true; }
			else if (ctx.phase == InputActionPhase.Canceled) { _addModifier = false; }

			if (logInput && ctx.phase != InputActionPhase.Started) { Debug.Log(name + " Add: " + _addModifier); }			
		}

		public void OnForceAttack(InputAction.CallbackContext ctx)
		{
			if (ctx.phase == InputActionPhase.Performed) { _forceAttack = true; }
			else if (ctx.phase == InputActionPhase.Canceled) { _forceAttack = false; }

			if (logInput && ctx.phase != InputActionPhase.Started) { Debug.Log(name + "ForceAttack: " + _forceAttack); }
		}

		public void OnDrag(InputAction.CallbackContext ctx)
		{
			// TODO: Change drag functionality from	selection box to moving & looking towards dragged position
			// depending on _addModifier

			if (ctx.phase == InputActionPhase.Started) { return; }

			if (ctx.phase == InputActionPhase.Performed)
			{
				SelectionBoxStarted?.Invoke();
				Drag = DragMode.Selection;

				if (logInput) { Debug.Log(name + "Drag performing with mode " + Drag.ToString()); }
			}
			else if (ctx.phase == InputActionPhase.Canceled	&& Drag != DragMode.None)
			{
				SelectionBoxEnded?.Invoke();
				Invoke(nameof(ResetDrag), 0.1f);

				if (logInput) { Debug.Log(name + "Drag ended"); }
			}
		}

		private void ResetDrag()
		{
			Drag = DragMode.None;
		}

		public void OnStop(InputAction.CallbackContext ctx)
		{
			if (ctx.phase == InputActionPhase.Performed)
			{
				foreach (var p in _selected)
				{
					p.Mover.Stop();
				}

				if (logInput) { Debug.Log(name + " Stop pressed"); }
			}
		}

		public void SwitchInputToMenu()
		{
			_playerInput.currentActionMap = _gameInputs.Menu.Get();

			if (logInput) { Debug.Log(name + " Switched to Menu input map"); }
		}

		public void SwitchInputToTactical()
		{
			_playerInput.currentActionMap = _gameInputs.Tactical.Get();

			if (logInput) { Debug.Log(name + " Switched to Tactical input map"); }
		}

		#endregion

		#region Static methods

		public static bool GetWorldPosition(Vector2 cursorPosition, int layerMask, out Vector3 worldPosition)
		{
			Ray ray = GetCameraRay(cursorPosition);
			worldPosition = new Vector3();
			
			if (Physics.Raycast(ray, out RaycastHit hit, Game.Input.CursorCastRange, layerMask))
			{
				worldPosition = hit.point;
				return true;
			}

			return false;
		}

		private static Ray GetCameraRay(Vector2 cursorPosition)
		{
			return Camera.main.ScreenPointToRay(cursorPosition);
		}

		private static bool RaycastNavMesh(Ray cursorRay, out RaycastHit hit)
		{
			float range = Game.Input.CursorCastRange;
			int layer = Game.Layer.Ground;

			if (Physics.Raycast(cursorRay, out hit, range, layer)) { return true; }

			return false;
		}

		private static bool SampleNavMesh(Vector3 point, out NavMeshHit hit)
		{
			int areas = NavMesh.AllAreas;
			float range = Game.Navigation.MaxNavMeshProjection;

			if (NavMesh.SamplePosition(point, out hit, range, areas)) { return true; }

			return false;
		}

		#endregion
	}
}