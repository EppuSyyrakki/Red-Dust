using Messaging;
using RedDust.Control.Input;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

namespace RedDust
{
	public class Game
	{
		private Game()
		{
			Bus = new MessageBus();
			Inputs = new GameInputs();
		}

		private static Game _instance;

		public static Game Instance
		{
			get
			{
				if (_instance == null) { _instance = new Game(); }

				return _instance;
			}
		}

		/// <summary>
		/// The class for relaying messages and managing their subscriptions.
		/// </summary>
		public MessageBus Bus { get; }

		/// <summary>
		/// The auto-generated Input wrapper.
		/// </summary>
		public GameInputs Inputs { get; }

		/// <summary>
		/// A convenience method to set up a PlayerInput component and the EventSystem UI Module.
		/// </summary>
		/// <param name="playerInput"></param>
		/// <param name="map"></param>
		/// <param name="setUiModule"></param>
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
