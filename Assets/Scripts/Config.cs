namespace RedDust
{
	public static class Config
	{
		public class Navigation
		{
			public const float MoveTargetTreshold = 0.5f;
			public const float MaxNavMeshProjection = 1.5f;
			public const float MovingThreshold = 0.05f;
			public const float WalkMulti = 0.6f;
			public const float CrouchMulti = 0.5f;
			public const float MaxSpeed = 10f;
		}

		public class Animation
		{
			public const string Velocity = "velocity";
			public const string Turning = "turning";
			public const string Crouched = "crouched";
		}

		public class Input
		{
			public const string MapTactical = "Tactical";
			public const string MapMenu = "Menu";
			public const string ActionMoveCursor = "MoveCursor";
			public const string ActionInteract = "Interact";
			public const string ActionDrag = "Drag";
			public const float MouseCastRange = 200f;
		}

		public class AI
		{
			public const float FollowUpdateInterval = 1f;
		}

		public class Layers
		{
			public const int Ground = 1 << 3;
			public const int Character = 1 << 6;
		}

		public class Tags
		{
			public const string TacticalControl = "TacticalControl";
		}
	}
}