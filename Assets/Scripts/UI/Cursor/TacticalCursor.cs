using RedDust.Control.Input;
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
			var controlObj = GameObject.FindGameObjectWithTag(Config.Tags.TacticalControl);
			_controls = controlObj.GetComponent<TacticalControls>();
			_iconRenderer = GetComponentInChildren<Image>();
			_iconRenderer.transform.localPosition = iconOffset;
		}

		private void Update()
		{
			transform.position = _controls.CursorPosition;
		}


	}
}