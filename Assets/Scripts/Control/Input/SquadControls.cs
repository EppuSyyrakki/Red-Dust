using RedDust.Control.Actions;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using static RedDust.Control.Input.GameInputs;

namespace RedDust.Control.Input
{
	public class SquadControls : Controls, ISquadActions
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
		private Player[] _players;

		private Vector2 _cursorPosition;
		private bool _addModifier = false;
		private bool _forceAttack = false;
		private IPlayerInteractable _interactable = null;

		public DragMode Drag { get; private set; }
		public Vector2 CursorPosition => _cursorPosition;

		public event Action SelectionBoxStarted;
		public event Action SelectionBoxEnded;
		public event Action<Sprite> InteractableChanged;
		public event Action InteractableNulled;

		#region Unity messages

		private void Awake()
		{
			Setup(Game.Instance.Inputs.Squad, true);
			Game.Instance.Inputs.Squad.SetCallbacks(this);
		}

		private void Start()
		{
			var squad = GetComponent<Squad>();
			_players = new Player[squad.Members.Count];

			for (int i = 0; i < squad.Members.Count; i++)
			{
				if (squad.Members[i] is Player p)
				{
					p.playerIndex = i;
					_players[i] = p;
				}
			}
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
			else if (_interactable != null)
			{
				NullInteractable();
			}
		}

		#endregion

		#region Private functionality

		private bool TryGetInteractable(Ray cursorRay, out IPlayerInteractable newInteractable)
		{
			newInteractable = null;

			if (Physics.Raycast(cursorRay, out RaycastHit hit, Values.Input.CursorCastRange, interactionLayers)
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

		private void MoveSelectedToCursor(Ray cursorRay)
		{
			NavMeshHit hit = new NavMeshHit();

			for (int i = 0; i < _selected.Count; i++)
			{
				if (!_addModifier) { _selected[i].CancelActions(); }

				if (i == 0)
				{
					if (!RaycastNavMesh(cursorRay, out hit)) { return; }

					_selected[i].AddAction(new MoveToAction(_selected[i], hit.position, true));
					continue;
				}

				Transform t = _selected[i].transform;
				Vector3 iDir = (t.position - hit.position).normalized * Values.Navigation.GroupMoveRange;
				Vector3 iPos = hit.position + iDir;
				
				if (!RaycastNavMesh(iPos, out hit)) { continue; }

				_selected[i].AddAction(new MoveToAction(_selected[i], iPos, true));
			}
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

		/// <summary>
		/// Divides a 4-point polygon into 2 triangles, checks if any Player is inside them and makes a new
		/// selection.
		/// </summary>
		/// <param name="screenCorners">Array of Vector3's. Only 4 points counted.</param>
		public void CheckPolygonSelection(Vector3[] screenCorners)
		{
			Vector3[] corners = new Vector3[4];
			List<Player> newSelection = new List<Player>(_players.Length);

			for (int i = 0; i < 4; i++)
			{
				GetWorldPosition(screenCorners[i], Values.Layer.Ground, out corners[i]);
			}

			foreach (Player p in _players)
			{
				if (PointInTriangle(p.transform.position, corners[0], corners[1], corners[2])
					|| PointInTriangle(p.transform.position, corners[2], corners[3], corners[0]))
				{
					newSelection.Add(p);
				}
			}

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

		#region ISquadAction implementation

		public void OnCursorChange(InputAction.CallbackContext ctx)
		{
			if (ctx.performed) { _cursorPosition = ctx.ReadValue<Vector2>(); }
		}

		public void OnMoveOrSelect(InputAction.CallbackContext ctx)
		{
			if (ctx.phase != InputActionPhase.Canceled || Drag != DragMode.None) { return; }

			var ray = GetCameraRay(_cursorPosition);
		
			if (!Physics.Raycast(ray, out RaycastHit hit, Values.Input.CursorCastRange, Values.Layer.Character)) 
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

		public void OnForceAttackModifier(InputAction.CallbackContext ctx)
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
				Invoke(nameof(ResetDrag), 0.1f);	// Hacky but works. This prevents 

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
					p.CancelActions();
				}

				if (logInput) { Debug.Log(name + " Stop pressed"); }
			}
		}		

		//public void SwitchInputToMenu()
		//{
		//	_playerInput.currentActionMap = _gameInputs.Menu.Get();

		//	if (logInput) { Debug.Log(name + " Switched to Menu input map"); }
		//}

		//public void SwitchInputToTactical()
		//{
		//	_playerInput.currentActionMap = _gameInputs.Squad.Get();

		//	if (logInput) { Debug.Log(name + " Switched to Tactical input map"); }
		//}

		#endregion

		#region Static methods

		private static bool RaycastNavMesh(Ray cursorRay, out NavMeshHit hit)
		{
			float range = Values.Input.CursorCastRange;
			int layer = Values.Layer.Ground;
			int areas = NavMesh.AllAreas;
			float projection = Values.Navigation.MaxNavMeshProjection;
			hit = new NavMeshHit();

			if (Physics.Raycast(cursorRay, out RaycastHit rHit, range, layer) 
				&& NavMesh.SamplePosition(rHit.point, out hit, projection, areas)) { return true; }

			return false;
		}

		private static bool RaycastNavMesh(Vector3 pos, out NavMeshHit hit)
		{
			float range = Values.Input.CursorCastRange * 0.1f;
			int layer = Values.Layer.Ground;
			int areas = NavMesh.AllAreas;
			float projection = Values.Navigation.MaxNavMeshProjection;
			hit = new NavMeshHit();

			if (Physics.Raycast(pos + Vector3.up, Vector3.down, out RaycastHit rHit, range, layer)
				&& NavMesh.SamplePosition(rHit.point, out hit, projection, areas)) { return true; }

			return false;
		}

		private static bool PointInTriangle(Vector3 p, Vector3 a, Vector3 b, Vector3 c)
		{
			Vector3 d, e;
			double w1, w2;
			d = b - a;
			e = c - a;

			if (Mathf.Approximately(e.z, 0)) { e.z = 0.0001f; }	// avoid division by 0

			w1 = (e.x * (a.z - p.z) + e.z * (p.x - a.x)) / (d.x * e.z - d.z * e.x);
			w2 = (p.z - a.z - w1 * d.z) / e.z;
			return (w1 >= 0f) && (w2 >= 0.0) && ((w1 + w2) <= 1.0);
		}

		#endregion
	}
}