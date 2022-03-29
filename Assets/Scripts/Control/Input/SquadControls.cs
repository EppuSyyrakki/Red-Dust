using RedDust.Control.Actions;
using System.Linq;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using static RedDust.Control.Input.GameInputs;
using RedDust.Combat;

namespace RedDust.Control.Input
{
	public class SquadControls : Controls, ISquadActions
	{
		public enum DragMode { None, LookDirection, Selection, UI }

		[SerializeField]
		private bool logInput;

		[SerializeField]
		private LayerMask interactionLayers;

		private const float castRange = Values.Input.CursorCastRange;

		/// <summary>
		/// Don't modify this List directly, use the public methods in the "Selection modification" region.
		/// They make sure the selection indicators follow along.
		/// </summary>
		private List<Player> selected = new List<Player>();
		private Player[] players;
		private Vector2 dragOrigin;
		private bool addModifier = false;
		private bool forceAttack = false;
		private IInteractable interactable = null;

		public DragMode Drag { get; private set; }

		public event Action<Vector2> SelectionBoxStarted;
		public event Action SelectionBoxEnded;
		public event Action<Sprite> InteractableChanged;
		public event Action InteractableNulled;
		public event Action<bool> ForceAttack;

		#region Unity messages

		private void Awake()
		{
			Setup(Game.Instance.Inputs.Squad, true);
			Game.Instance.Inputs.Squad.SetCallbacks(this);
		}

		private void Start()
		{
			var squad = GetComponent<Squad>();
			var characters = squad.Members;
			players = new Player[characters.Length];

			for (int i = 0; i < characters.Length; i++)
			{
				if (characters[i] is Player p)
				{
					players[i] = p;
					p.PlayerIndex = i;
				}
			}
		}

		private void Update()
		{
			// No selected players means no action can be added
			if (selected.Count == 0) { return; }

			Ray ray = GetCameraRay(CursorPosition);

			// 1. UI check - if true, (for safety, null _currentinteractable?) return
			// 2. some activated ability check? Like first aid
			// 3. Interactable check

			if (TryGetInteractable(ray, out IInteractable newInteractable)) 
			{
				TryChangeInteractable(newInteractable);
			}
			else if (interactable != null)
			{
				NullInteractable();
			}
		}

		#endregion

		#region Private functionality

		private bool TryGetInteractable(Ray cursorRay, out IInteractable newInteractable)
		{
			newInteractable = null;

			if (Physics.Raycast(cursorRay, out RaycastHit hit, castRange, interactionLayers)
				&& hit.collider.TryGetComponent(out newInteractable))
			{
				if (newInteractable is Player p && selected.Contains(p)) 
				{
					// Don't allow player to interact with itself
					return false; 
				}

				return true;
			}
			
			// No interactable from cursor ray found				
			return false;			
		}

		private void TryChangeInteractable(IInteractable newInteractable)
		{
			if (interactable == null || newInteractable != interactable)
			{
				interactable = newInteractable;
				InteractableChanged?.Invoke(interactable.GetIcon(selected[0]));

				if (logInput) { Debug.Log($"{name}'s current interactable is: {interactable.GetType().Name}"); }
			}
		}

		private void NullInteractable()
		{
			InteractableNulled?.Invoke();
			interactable = null;

			if (logInput) { Debug.Log($"{name}'s current interactable was nulled"); }
		}

		private void MoveSelectedToCursor(Ray cursorRay)
		{
			NavMeshHit hit = new NavMeshHit();

			for (int i = 0; i < selected.Count; i++)
			{
				if (!addModifier) { selected[i].CancelActions(); }

				if (i == 0)
				{
					if (!RaycastNavMesh(cursorRay, out hit)) { return; }

					selected[i].AddAction(new MoveToAction(selected[i], hit.position, true));
					continue;
				}

				Transform t = selected[i].transform;
				Vector3 iDir = (t.position - hit.position).normalized * Values.Navigation.GroupMoveRange;
				Vector3 iPos = hit.position + iDir;
				
				if (!RaycastNavMesh(iPos, out hit)) { continue; }

				selected[i].AddAction(new MoveToAction(selected[i], iPos, true));
			}
		}

		private void AttackSelectedOnCursor(Ray cursorRay)
		{
			if (Physics.Raycast(cursorRay, out RaycastHit hit, castRange, Values.Layer.ProjectileHits))
			{
				var health = hit.collider.gameObject.GetComponent<Health>();
				ShootAction action;

				for (int i = 0; i < selected.Count; i++)
				{
					if (!addModifier) { selected[i].CancelActions(); }

					if (health != null) { action = new ShootAction(selected[i], health); }
					else { action = new ShootAction(selected[i], hit.point); }

					selected[i].AddAction(action);
				}
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
			if (addModifier)
			{
				if (selected.Contains(p))
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
			foreach (var p in selected) { p.SetIndicatorSelected(false); }
			selected.Clear();
		}

		/// <summary>
		/// Divides a 4-point polygon into 2 triangles, checks if any Player is inside them and makes a new
		/// selection.
		/// </summary>
		/// <param name="screenCorners">Array of Vector3's. Only 4 points counted.</param>
		public void CheckPolygonSelection(Vector3[] screenCorners)
		{
			Vector3[] corners = new Vector3[4];
			List<Player> newSelection = new List<Player>(players.Length);

			for (int i = 0; i < 4; i++)
			{
				GetWorldPosition(screenCorners[i], Values.Layer.Ground, out corners[i]);
			}

			foreach (Player p in players)
			{
				if (PointInTriangle(p.transform.position, corners[0], corners[1], corners[2])
					|| PointInTriangle(p.transform.position, corners[2], corners[3], corners[0]))
				{
					newSelection.Add(p);
				}
			}

			ClearSelection();
			selected = newSelection;
			foreach (var p in selected) { p.SetIndicatorSelected(true); }
		}

		private void AddToSelection(Player p)
		{
			selected.Add(p);
			p.SetIndicatorSelected(true);
		}

		private void RemoveFromSelection(Player p)
		{
			selected.Remove(p); 
			p.SetIndicatorSelected(false);
		}

		private void SortSelection()
		{
			selected = selected.OrderBy(s => s.PlayerIndex).ToList();
		}

		#endregion

		#region ISquadAction implementation

		public void OnCursorChange(InputAction.CallbackContext ctx)
		{
			if (ctx.performed) { CursorPosition = ctx.ReadValue<Vector2>(); }
		}

		public void OnSelect(InputAction.CallbackContext ctx)
		{
			// Save the cursor position in case this will be a drag later
			if (ctx.phase == InputActionPhase.Performed) { dragOrigin = CursorPosition; }

			if (ctx.phase != InputActionPhase.Canceled || Drag != DragMode.None) { return; }

			var ray = GetCameraRay(CursorPosition);
			var range = castRange;
			var layers = Values.Layer.Character;
		
			if (Physics.Raycast(ray, out RaycastHit hit, range, layers)
				&& hit.collider.TryGetComponent(out Player p)
				&& players.Contains(p))
			{
				ModifySelection(p);
				
				if (logInput) { Debug.Log(name + " Select released"); }
			}
		}

		public void OnInteract(InputAction.CallbackContext ctx)
		{
			if (ctx.phase != InputActionPhase.Canceled || selected.Count == 0) { return; }
			
			if (interactable != null)
			{
				for (int i = 0; i < selected.Count; i++)
				{
					var player = selected[i];

					if (!addModifier) { player.CancelActions(); }
					
					player.AddAction(interactable.GetAction(player));

					if (logInput) { Debug.Log(name + " Interact button released"); }
				}

				return;
			}

			if (forceAttack)
			{
				AttackSelectedOnCursor(GetCameraRay(CursorPosition));
				return;
			}

			MoveSelectedToCursor(GetCameraRay(CursorPosition));
		}

		public void OnAddModifier(InputAction.CallbackContext ctx)
		{
			if (ctx.phase == InputActionPhase.Performed) { addModifier = true; }
			else if (ctx.phase == InputActionPhase.Canceled) { addModifier = false; }

			if (logInput && ctx.phase != InputActionPhase.Started) { Debug.Log(name + " Add: " + addModifier); }			
		}

		public void OnForceAttackModifier(InputAction.CallbackContext ctx)
		{
			if (ctx.phase == InputActionPhase.Started) { return; }

			forceAttack = ctx.phase == InputActionPhase.Performed ? true : false;
			ForceAttack?.Invoke(forceAttack);

			if (logInput && ctx.phase != InputActionPhase.Started) { Debug.Log(name + "ForceAttack: " + forceAttack); }
		}

		public void OnDrag(InputAction.CallbackContext ctx)
		{
			// TODO: Change drag functionality from	selection box to moving & looking towards dragged position
			// depending on _addModifier

			if (ctx.phase == InputActionPhase.Started) { return; }

			if (ctx.phase == InputActionPhase.Performed)
			{
				SelectionBoxStarted?.Invoke(dragOrigin);
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
				foreach (var p in selected)
				{
					p.CancelActions();
				}

				if (logInput) { Debug.Log(name + " Stop pressed"); }
			}
		}
		
		public void OnSelectDirect(InputAction.CallbackContext ctx)
		{
			if (ctx.phase != InputActionPhase.Performed) { return; }

			if (SelectionAxisToIndex(ctx.ReadValue<Vector2>(), out int i))
			{
				ModifySelection(players[i]);
			}	
		}

		private bool SelectionAxisToIndex(Vector2 value, out int i)
		{
			i = -1;

			if (value == Vector2.up) { i = 0; }
			else if (value == Vector2.right) { i = 1; }
			else if (value == Vector2.down) { i = 2; }
			else if (value == Vector2.left) { i = 3; }

			return i > -1 && i < players.Length;
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
			int layer = Values.Layer.Ground;
			int areas = NavMesh.AllAreas;
			float projection = Values.Navigation.MaxNavMeshProjection;
			hit = new NavMeshHit();

			if (Physics.Raycast(cursorRay, out RaycastHit rHit, castRange, layer) 
				&& NavMesh.SamplePosition(rHit.point, out hit, projection, areas)) { return true; }

			return false;
		}

		private static bool RaycastNavMesh(Vector3 pos, out NavMeshHit hit)
		{
			int layer = Values.Layer.Ground;
			int areas = NavMesh.AllAreas;
			float projection = Values.Navigation.MaxNavMeshProjection;
			hit = new NavMeshHit();

			if (Physics.Raycast(pos + Vector3.up, Vector3.down, out RaycastHit rHit, castRange * 0.1f, layer)
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