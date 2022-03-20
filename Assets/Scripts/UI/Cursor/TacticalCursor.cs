using RedDust.Control.Actions;
using RedDust.Control.Input;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace RedDust.UI.Cursor
{
	public class TacticalCursor : MonoBehaviour
	{
		[SerializeField]
		private Vector2 iconOffset;

		private TacticalControls _controls = null;
		private Image _iconRenderer = null;

		private void Awake()
		{
			var controlObj = GameObject.FindGameObjectWithTag(Game.Tag.PlayerSquad);
			_controls = controlObj.GetComponent<TacticalControls>();
			_iconRenderer = GetComponentInChildren<Image>();
			_iconRenderer.transform.localPosition = iconOffset;
		}

		private void OnEnable()
		{
			_controls.InteractableNulled += OnActionNulled;
			_controls.InteractableChanged += OnActionChanged;
		}

		private void OnDisable()
		{
			_controls.InteractableNulled -= OnActionNulled;
			_controls.InteractableChanged -= OnActionChanged;
		}

		private void Update()
		{
			transform.position = _controls.CursorPosition;
		}

		private void OnActionNulled()
		{
			_iconRenderer.sprite = null;
			_iconRenderer.color = Game.Colors.WhiteTransparent;
		}

		private void OnActionChanged(Sprite icon)
		{
			_iconRenderer.sprite = icon;
			_iconRenderer.color = Game.Colors.WhiteOpaque;
		}
	}
}