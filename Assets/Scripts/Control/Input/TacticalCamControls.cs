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

		private Vector2 _cursorPosition = new Vector2();
		private CamInput _input;	// This is being lerped towards the target and used in cam motion
		private CamInput _targetInput;	// This is what's being set by inputs.
		private CinemachineVirtualCamera _camera;
		private bool _dragging = false;

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

			if (_dragging) 
			{ 
				DragTargetByInput();
				return;
			}

			MoveTargetByInput();
			RotateTargetByInput();
		}

		private void LateUpdate()
		{
			MoveCamera();
		}

		private void OnDrawGizmos()
		{
			if (Target == null) { return; }

			Gizmos.color = Color.blue;
			Gizmos.DrawSphere(Target.position, 0.3f);
		}

		private void DragTargetByInput()
		{

		}

		private void MoveTargetByInput()
		{
			float x = _input.movement.x * Time.deltaTime * _moveSpeed;
			float y = RaycastTerrainHeight();
			float z = _input.movement.y * Time.deltaTime * _moveSpeed;

			Target.transform.Translate(new Vector3(x, y, z));
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
			Vector3 pos = Target.position;
			Vector3 origin = pos + Vector3.up * 50f;
			Vector3 end = pos + Vector3.down * 100f;

			if (Physics.Linecast(origin, end, out var hit, Values.Layer.Ground))
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
				_cursorPosition = ctx.ReadValue<Vector2>();
			}
		}

		public void OnMove(InputAction.CallbackContext ctx)
		{
			_targetInput.movement = ctx.ReadValue<Vector2>();
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
			if (ctx.phase == InputActionPhase.Performed)
			{
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