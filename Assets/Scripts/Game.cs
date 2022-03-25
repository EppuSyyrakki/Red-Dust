using Messaging;
using RedDust.Combat;
using RedDust.Control.Input;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

namespace RedDust
{
	/// <summary>
	/// Ensure that this is script's execution order is before any other character-related script.
	/// </summary>
	public class Game : MonoBehaviour
	{
		public static Game Instance { get; private set; }

		/// <summary>
		/// The class for relaying messages and managing their subscriptions.
		/// </summary>
		public MessageBus Bus { get; private set; }

		/// <summary>
		/// The auto-generated Input wrapper.
		/// </summary>
		public GameInputs Inputs { get; private set; }

		/// <summary>
		/// The host for all projectiles and other attacks in the game. Handles 
		/// </summary>
		private ProjectileScheduler Projectiles { get; set; }

		public void Awake()
		{
			if (Instance != null && Instance != this) { Destroy(this); }
			else { Instance = this; }			

			Bus = new MessageBus();
			Inputs = new GameInputs();
			Projectiles = new ProjectileScheduler();
		}

		private void Update()
		{
			if (Projectiles.HasSpawnQueue) { Projectiles.RunSpawn(); }

			if (Projectiles.HasJobs) { Projectiles.ScheduleJobs(); }		
		}

		private void LateUpdate()
		{
			if (Projectiles.HasJobs) { Projectiles.CompleteJobs(); }

			if (Projectiles.HasDeSpawnQueue) { Projectiles.RunDeSpawn(); }
		}

		/// <summary>
		/// A convenience method to set up a PlayerInput component and the EventSystem UI Module.
		/// </summary>
		/// <param name="playerInput">The component to set.</param>
		/// <param name="map">The mapping to set to the component.</param>
		/// <param name="setUiModule">Connect the component parameter to the UI Module.</param>
		public void SetInputComponent(PlayerInput playerInput, InputActionMap map, bool setUiModule)
		{
			playerInput.actions = Inputs.asset;
			playerInput.defaultActionMap = map.name;
			playerInput.currentActionMap = map;

			if (!setUiModule) { return; }

			playerInput.camera = Camera.main;
			var uiInputModule = EventSystem.current.gameObject.GetComponent<InputSystemUIInputModule>();
			uiInputModule.actionsAsset = Inputs.asset;
			playerInput.uiInputModule = uiInputModule;
		}
	}
}
