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
		[System.Serializable]
		public class ActionIcon
		{
			public Texture2D icon;
			public ActionBase action;
		}

		[SerializeField]
		private Vector2 iconOffset;

		[SerializeField]
		private ActionIcon[] icons = null;

		private TacticalControls _controls = null;
		private Image _iconRenderer = null;
		private Color _transparent = new Color(1, 1, 1, 0);
		private Color _opaque = new Color(1, 1, 1, 1);

		private void Awake()
		{
			var controlObj = GameObject.FindGameObjectWithTag(Config.Tags.PlayerSquad);
			_controls = controlObj.GetComponent<TacticalControls>();
			_iconRenderer = GetComponentInChildren<Image>();
			_iconRenderer.transform.localPosition = iconOffset;
		}

		private void OnEnable()
		{
			_controls.HideActionIcon += OnHideActionIcon;
			_controls.ShowActionIcon += OnShowActionIcon;
		}

		private void OnDisable()
		{
			_controls.HideActionIcon -= OnHideActionIcon;
			_controls.ShowActionIcon -= OnShowActionIcon;
		}

		private void Update()
		{
			transform.position = _controls.CursorPosition;
		}

		private void OnHideActionIcon()
		{
			_iconRenderer.color = _transparent;
		}

		private void OnShowActionIcon(Type actionType)
		{
			_iconRenderer.color = _opaque;
		}
	}
}