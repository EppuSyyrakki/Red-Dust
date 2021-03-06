using System;
using UnityEngine;
using Cinemachine;
using UnityEngine.InputSystem;
using static RedDust.Control.Input.GameInputs;

namespace RedDust.Control.Input
{
	public class TacticalCamControls : Controls, ICameraActions
	{
		[SerializeField]
		private float _moveSpeed;

		[SerializeField]
		private float _rotationSpeed;

		[SerializeField]
		private float _zoomSpeed;

		[SerializeField]
		private Vector3 _locationOffset;

		private Vector3 _dragOrigin;
		private Vector3 _targetDragOrigin;
		private Vector3 _dragCurrent;
		private bool _dragging = false;
		private CamInput _input;	// This is being lerped towards the target and used in cam motion
		private CamInput _targetInput;	// This is what's being set by inputs.
		private CinemachineVirtualCamera _camera;

		public Transform Target { get; private set; }

		private void Awake()
		{			
			var squadObj = GameObject.FindGameObjectWithTag(Values.Tag.PlayerSquad);
			CreateTarget(squadObj.transform.position);
			_camera = GetComponent<CinemachineVirtualCamera>();

			Setup(Game.Instance.Inputs.Camera, false);
			Game.Instance.Inputs.Camera.SetCallbacks(this);		
		}

		public void CreateTarget(Vector3 position)
		{
			var target = new GameObject(Values.Camera.TargetName);
			target.transform.position = position;
			target.tag = Values.Tag.CamTarget;
			Target = target.transform;
		}

		private void Update()
		{
			_input.LerpTo(_targetInput);
			DragTargetByInput();
			MoveTargetByInput();
			RotateTargetByInput();
			MoveCamera();
		}

		private void OnDrawGizmos()
		{
			if (Target == null) { return; }

			Gizmos.color = Color.blue;
			Gizmos.DrawSphere(Target.position, 0.1f);
		}

		private void DragTargetByInput()
		{
			if (!_dragging) { return; }

			Vector3 drag = _dragOrigin - _dragCurrent;

			if (drag.magnitude < 0.01f) { return; }
			Target.position = _targetDragOrigin + drag;
		}

		private void MoveTargetByInput()
		{
			float x = _input.pan.x * Time.deltaTime * _moveSpeed;
			float y = RaycastTerrainHeight();
			float z = _input.pan.y * Time.deltaTime * _moveSpeed;

			Target.Translate(new Vector3(x, y, z));
		}

		private void RotateTargetByInput()
		{
			float rotation = _input.rotation * Time.deltaTime * _rotationSpeed;
			Target.transform.Rotate(Vector3.up, rotation);
		}

		private void MoveCamera()
		{
			float height = _locationOffset.y + _input.zoom * _zoomSpeed * Time.deltaTime;
			height = Mathf.Clamp(height, Values.Camera.MinZoom, Values.Camera.MaxZoom);
			float fov = height + Values.Camera.FovZoomOffset;
			_locationOffset.y = height;
			transform.position = Target.TransformPoint(_locationOffset);
			Vector3 lookPosition = Target.TransformPoint(Vector3.forward * Values.Camera.LookAhead);
			transform.LookAt(lookPosition);
			_camera.m_Lens.FieldOfView = fov;
		}

		private float RaycastTerrainHeight()
		{
			var pos = Target.position + Vector3.up * 200f;
			var layer = Values.Layer.Ground;

			if (Physics.Raycast(pos, Vector3.down, out var hit, 500f, layer))
			{
				float relativeY = Target.transform.InverseTransformPoint(hit.point).y;
				return relativeY;
			}

			return 0;
		}


		#region ICameraActions implementation

		public void OnCursorChange(InputAction.CallbackContext ctx)
		{
			if (ctx.phase == InputActionPhase.Performed)
			{
				CursorPosition = ctx.ReadValue<Vector2>();

				if (_dragging && GetWorldPosition(CursorPosition, Values.Layer.Ground, out var world))
				{
					_dragCurrent = world;
				}
			}	
		}

		public void OnPan(InputAction.CallbackContext ctx)
		{
			_targetInput.pan = ctx.ReadValue<Vector2>();
		}

		public void OnRotate(InputAction.CallbackContext ctx)
		{
			_targetInput.rotation = ctx.ReadValue<float>();
		}

		public void OnZoom(InputAction.CallbackContext ctx)
		{
			if (ctx.phase == InputActionPhase.Performed)
			{
				float value = ctx.ReadValue<float>();
				_targetInput.zoom = Mathf.Clamp(value, -1, 1);
				Invoke(nameof(ResetZoom), Values.Camera.InputZoomResetDelay);
			}
		}

		private void ResetZoom()
		{
			_targetInput.zoom = 0;
		}

		public void OnDrag(InputAction.CallbackContext ctx)
		{
			if (ctx.phase == InputActionPhase.Performed
				&& GetWorldPosition(CursorPosition, Values.Layer.Ground, out _dragOrigin))
			{
				_dragCurrent = _dragOrigin;
				_targetDragOrigin = Target.position;
				_dragging = true;
			}
			else if (ctx.phase == InputActionPhase.Canceled)
			{
				_dragging = false;
			}
		}

		#endregion
	}
}