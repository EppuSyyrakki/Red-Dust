using RedDust.Control;
using RedDust.Control.Input;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RedDust.UI.Cursor
{
	public class TacticalCursor : MonoBehaviour
	{
		[SerializeField]
		private Vector2 iconOffset;

		[SerializeField]
		private Image iconRenderer = null;

		[SerializeField]
		private Image selectionBox = null;

		private TacticalControls _controls = null;
		private bool _boxEnabled;
		private Vector2 _dragStart;
		private Vector2 _dragEnd;
		// private float _dragTimer = 0;

		#region Unity messages

		private void Awake()
		{
			var controlObj = GameObject.FindGameObjectWithTag(Game.Tag.PlayerSquad);
			_controls = controlObj.GetComponent<TacticalControls>();
			iconRenderer.transform.localPosition = iconOffset;
			selectionBox.color = Game.Colors.SelectionBox;
		}

		private void OnEnable()
		{
			_controls.InteractableNulled += OnActionNulled;
			_controls.InteractableChanged += OnActionChanged;
			_controls.SelectionBoxStarted += OnSelectionBoxStarted;
			_controls.SelectionBoxEnded += OnSelectionBoxEnded;
		}

		private void OnDisable()
		{
			_controls.InteractableNulled -= OnActionNulled;
			_controls.InteractableChanged -= OnActionChanged;
			_controls.SelectionBoxStarted -= OnSelectionBoxStarted;
			_controls.SelectionBoxEnded -= OnSelectionBoxEnded;
		}

		private void Update()
		{
			transform.position = _controls.CursorPosition;

			if (!_boxEnabled) { return; }

			// _dragTimer += Time.deltaTime;
			_dragEnd = transform.position;
			Vector2 dragMiddle = (_dragStart + _dragEnd) * 0.5f;
			RectTransform box = selectionBox.rectTransform;
			box.position = dragMiddle;
			// Set the size as the difference between start and end
			box.sizeDelta = new Vector2(Mathf.Abs(_dragStart.x - _dragEnd.x), Mathf.Abs(_dragStart.y - _dragEnd.y));
		}

		#endregion

		#region Delegate handlers

		private void OnActionNulled()
		{
			iconRenderer.sprite = null;
			iconRenderer.color = Game.Colors.WhiteTransparent;
		}

		private void OnActionChanged(Sprite icon)
		{
			iconRenderer.sprite = icon;
			iconRenderer.color = Game.Colors.WhiteOpaque;
		}

		private void OnSelectionBoxStarted()
		{
			//Debug.Log("Selection started");
			//_dragTimer = 0;
			_boxEnabled = true;
			selectionBox.enabled = _boxEnabled;
			_dragStart = transform.position;
		}

		private void OnSelectionBoxEnded()
		{
			_boxEnabled = false;
			selectionBox.enabled = _boxEnabled;

			// if (_dragTimer < Game.Input.DragMinimum) { return; }
			// Debug.Log("Selection ended, calculating");

			Vector3[] corners = RaycastWorldCorners();
			List<Player> newSelection = new List<Player>();

			foreach (Character c in _controls.PlayerSquad.Members)
			{
				if (c is Player p 
					&& (PointInTriangle(p.transform.position, corners[0], corners[1], corners[2])
					|| PointInTriangle(p.transform.position, corners[0], corners[3], corners[2])))
				{
					newSelection.Add(p);
				}
			}

			_controls.SetSelection(newSelection);
		}

		#endregion

		private Vector3[] RaycastWorldCorners()
		{
			Vector3[] screenCorners = new Vector3[4];
			Vector3[] worldCorners = new Vector3[4];
			selectionBox.rectTransform.GetWorldCorners(screenCorners);

			for (int i = 0; i < screenCorners.Length; i++)
			{
				TacticalControls.GetWorldPosition(screenCorners[i], Game.Layer.Ground, out worldCorners[i]);
			}

			return worldCorners;
		}

		private bool PointInTriangle(Vector3 p, Vector3 a, Vector3 b, Vector3 c)
		{
			Vector3 d, e;
			double w1, w2;
			d = b - a;
			e = c - a;

			if (Mathf.Approximately(e.z, 0))
			{
				e.z = 0.0001f;
			}

			w1 = (e.x * (a.z - p.z) + e.z * (p.x - a.x)) / (d.x * e.z - d.z * e.x);
			w2 = (p.z - a.z - w1 * d.z) / e.z;
			return (w1 >= 0f) && (w2 >= 0.0) && ((w1 + w2) <= 1.0);
		}
	}
}