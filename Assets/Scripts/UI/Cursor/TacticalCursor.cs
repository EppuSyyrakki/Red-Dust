using RedDust.Control;
using RedDust.Control.Actions;
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

		private SquadControls controls = null;
		private bool boxEnabled;
		private Vector2 dragOrigin;
		private Vector2 dragEnd;
		private Sprite storedIcon = null;

		#region Unity messages

		private void Awake()
		{
			var controlObj = GameObject.FindGameObjectWithTag(Values.Tag.PlayerSquad);
			controls = controlObj.GetComponent<SquadControls>();
			iconRenderer.transform.localPosition = iconOffset;
			selectionBox.color = Values.Color.SelectionBox;
		}

		private void OnEnable()
		{
			controls.InteractableNulled += OnActionNulled;
			controls.InteractableChanged += OnActionChanged;
			controls.SelectionBoxStarted += OnSelectionBoxStarted;
			controls.SelectionBoxEnded += OnSelectionBoxEnded;
			controls.ForceAttack += OnForceAttack;
		}

		private void OnDisable()
		{
			controls.InteractableNulled -= OnActionNulled;
			controls.InteractableChanged -= OnActionChanged;
			controls.SelectionBoxStarted -= OnSelectionBoxStarted;
			controls.SelectionBoxEnded -= OnSelectionBoxEnded;
			controls.ForceAttack -= OnForceAttack;
		}

		private void Update()
		{
			transform.position = controls.CursorPosition;

			if (!boxEnabled) { return; }

			Vector3 pos = transform.position;
			dragEnd = new Vector2(Mathf.Clamp(pos.x, 0, Screen.width), Mathf.Clamp(pos.y, 0, Screen.height));
			Vector2 dragMiddle = (dragOrigin + dragEnd) * 0.5f;
			RectTransform box = selectionBox.rectTransform;
			box.position = dragMiddle;
			// Set the size as the difference between start and end
			box.sizeDelta = new Vector2(Mathf.Abs(dragOrigin.x - dragEnd.x), Mathf.Abs(dragOrigin.y - dragEnd.y));
		}

		#endregion

		#region Delegate handlers

		private void OnActionNulled()
		{
			iconRenderer.sprite = null;
			iconRenderer.color = Values.Color.WhiteTransparent;
		}

		private void OnActionChanged(Sprite icon)
		{
			iconRenderer.sprite = icon;
			iconRenderer.color = Values.Color.WhiteOpaque;
		}

		private void OnSelectionBoxStarted(Vector2 origin)
		{
			boxEnabled = true;
			selectionBox.enabled = boxEnabled;
			dragOrigin = origin;
		}

		private void OnSelectionBoxEnded()
		{
			boxEnabled = false;
			selectionBox.enabled = boxEnabled;
			Vector3[] screenCorners = new Vector3[4];
			selectionBox.rectTransform.GetWorldCorners(screenCorners);
			controls.CheckPolygonSelection(screenCorners);
		}

		private void OnForceAttack(bool forceAttack)
		{
			if (forceAttack)
			{
				storedIcon = iconRenderer.sprite;
				iconRenderer.sprite = ActionBase.LoadIcon(nameof(ShootAction));
				return;
			}

			iconRenderer.sprite = storedIcon;
		}

		#endregion
	}
}