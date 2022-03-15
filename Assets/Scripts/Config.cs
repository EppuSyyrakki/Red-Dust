using System.Collections;
using UnityEngine;

namespace RedDust
{
	public static class Config
	{
		public class Navigation
		{
			public const float MoveTargetTreshold = 0.5f;
			public const float MaxNavMeshProjection = 1f;
			public const float MovingThreshold = 0.1f;
			public const float WalkMulti = 0.6f;
			public const float CrouchMulti = 0.5f;
		}

		public class Animation
		{
			public const string Speed = "forwardSpeed";
			public const string IsCrouched = "isCrouched";
		}
	}
}