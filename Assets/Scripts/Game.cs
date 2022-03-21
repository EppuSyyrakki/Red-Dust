using Messaging;
using UnityEngine;

namespace RedDust
{
	public class Game
	{
		private static Game _instance;

		public static Game Instance
		{
			get
			{
				if (_instance == null)
				{
					_instance = new Game();
				}

				return _instance;
			}
		}

		private Game()
		{
			Bus = new MessageBus();
		}

		/// <summary>
		/// The class for relaying messages and managing their subscriptions.
		/// </summary>
		public MessageBus Bus { get; }


		// The following are used in various places, like replacing string references in code or
		// as default values.
		//
		// TODO: Make the relevant things (like Colors) user changeable via properties.
		public static class Navigation
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

		public static class Animation
		{
			public const string Velocity = "velocity";
			public const string Turning = "turning";
			public const string Crouched = "crouched";
		}

		public static class Input
		{
			public const string MapTactical = "Tactical";
			public const string MapMenu = "Menu";
			public const string ActionMoveCursor = "MoveCursor";
			public const string ActionInteract = "Interact";
			public const string ActionDrag = "Drag";
			public const float CursorCastRange = 200f;
		}

		public static class Layer
		{
			public const int Ground = 1 << 3;
			public const int Character = 1 << 6;
		}

		public static class Tag
		{
			public const string PlayerSquad = "PlayerSquad";
		}

		public static class Timer
		{
			public const float IndicatorUpdate = 0.5f;
		}

		public static class Path
		{
			public const string Formations = "Control/Formations";
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
