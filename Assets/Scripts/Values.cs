using UnityEngine;

namespace RedDust
{
	public static class Values
	{
		// TODO: Make the relevant things (like Colors) user changeable via properties.
		public class Navigation
		{
			public const float MoveTargetTreshold = 0.5f;
			public const float MaxNavMeshProjection = 1.5f;
			public const float MovingThreshold = 0.05f;
			public const float WalkMulti = 0.6f;
			public const float CrouchMulti = 0.5f;
			public const float MaxSpeed = 10f;
			public const float StopDistance = 0.75f;
			public const float FollowUpdateInterval = 0.4f;
			public const float AgentStoppingDistance = 0.4f;
			public const float AgentStoppingDistanceFollow = 3.8f;
			public const float GroupMoveRange = 1.2f;
		}

		public class Animation
		{
			public const string Velocity = "velocity";
			public const string Turning = "turning";
			public const string Crouched = "crouched";
		}

		public class Input
		{
			public const string MapSquad = "Squad";
			public const string MapCam = "Camera";
			public const string MapMenu = "Menu";
			public const float CursorCastRange = 200f;
		}

		public class Layer
		{
			public const int Ground = 1 << 3;
			public const int Character = 1 << 6;
		}

		public class Tag
		{
			public const string PlayerSquad = "PlayerSquad";
			public const string CamTarget = "CameraTarget";
			public const string TacCamera = "TacticalCamera";
		}

		public class Timer
		{
			public const float IndicatorUpdate = 0.5f;
		}

		public class Path
		{
			public const string Formations = "Control/Formations";
		}

		public class Camera
		{
			public const float AccelMove = 20f;
			public const float AccelRot = 20f;
			public const float AccelZoom = 20f;
			public const float InputZoomResetDelay = 0.1f;
			public const float LookAhead = 2f;
			public const float RotationSpeed = 90f;
			public const float MoveSpeed = 20f;
			public const float ZoomSpeed = 300f;
			public const float FovZoomOffset = 43f;
			public const string TargetName = "Control Camera Target";
			public const float MinZoom = 2f;
			public const float MaxZoom = 15f;
			public static readonly Vector3 DefaultOffset = new Vector3(0, 8, -7);
		}

		public class Color
		{
			public static readonly UnityEngine.Color SelectionBox = new UnityEngine.Color(0.35f, 1f, 0.2f, 1f);
			public static readonly UnityEngine.Color Player = new UnityEngine.Color(0.35f, 1f, 0.2f, 1f);
			public static readonly UnityEngine.Color Friendly = new UnityEngine.Color(0.2f, 0.5f, 0.9f, 1f);
			public static readonly UnityEngine.Color Neutral = new UnityEngine.Color(1f, 1f, 1f, 1f);
			public static readonly UnityEngine.Color Hostile = new UnityEngine.Color(1f, 0.2f, 0.2f, 1f);
			public static readonly UnityEngine.Color WhiteTransparent = new UnityEngine.Color(1f, 1f, 1f, 0);
			public static readonly UnityEngine.Color WhiteOpaque = new UnityEngine.Color(1f, 1f, 1f, 1f);
		}
	}

}