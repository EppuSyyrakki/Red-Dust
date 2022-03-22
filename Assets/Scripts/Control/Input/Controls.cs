using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace RedDust.Control.Input
{
	[RequireComponent(typeof(PlayerInput))]
	public abstract class Controls : MonoBehaviour
	{
		protected PlayerInput PlayerInput { get; private set; }
		// protected Vector2 CursorPosition

		public bool InputsActive
		{
			set
			{
				if (value) { PlayerInput.ActivateInput(); }
				else { PlayerInput.DeactivateInput(); }
			}
		}

		protected void Setup(InputActionMap actionMap, bool setUiModule)
		{
			PlayerInput = GetComponent<PlayerInput>();
			Game.Instance.SetInputComponent(PlayerInput, actionMap, setUiModule);
		}

		private void OnEnable()
		{
			PlayerInput.ActivateInput();
		}

		private void OnDisable()
		{
			PlayerInput.DeactivateInput();
		}

		protected static bool GetWorldPosition(Vector2 screenPos, int layerMask, out Vector3 worldPosition)
		{
			Ray ray = GetCameraRay(screenPos);
			worldPosition = new Vector3();

			if (Physics.Raycast(ray, out RaycastHit hit, Values.Input.CursorCastRange, layerMask))
			{
				worldPosition = hit.point;
				return true;
			}

			return false;
		}

		protected static Ray GetCameraRay(Vector2 cursorPosition)
		{
			return Camera.main.ScreenPointToRay(cursorPosition);
		}
	}
}