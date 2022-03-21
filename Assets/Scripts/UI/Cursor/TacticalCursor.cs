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

		#region Unity messages

		private void Awake()
		{
			var controlObj = GameObject.FindGameObjectWithTag(Game.Tag.PlayerSquad);
			_controls = controlObj.GetComponent<TacticalControls>();
			iconRenderer.transform.localPosition = iconOffset;
			selectionBox.color = Game.Color.SelectionBox;
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

			Vector3 pos = transform.position;
			_dragEnd = new Vector2(Mathf.Clamp(pos.x, 0, Screen.width), Mathf.Clamp(pos.y, 0, Screen.height));
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
			iconRenderer.color = Game.Color.WhiteTransparent;
		}

		private void OnActionChanged(Sprite icon)
		{
			iconRenderer.sprite = icon;
			iconRenderer.color = Game.Color.WhiteOpaque;
		}

		private void OnSelectionBoxStarted()
		{
			_boxEnabled = true;
			selectionBox.enabled = _boxEnabled;
			_dragStart = transform.position;
		}

		private void OnSelectionBoxEnded()
		{
			_boxEnabled = false;
			selectionBox.enabled = _boxEnabled;
			Vector3[] screenCorners = new Vector3[4];
			selectionBox.rectTransform.GetWorldCorners(screenCorners);
			_controls.CheckPolygonSelection(screenCorners);
		}

		#endregion
	}
}