using RedDust.Control.Actions;
using System;
using UnityEngine;

namespace RedDust.Control
{
	/// <summary>
	/// No other characters should be Players except those in Squad with PlayerSquad tag.
	/// </summary>
	public class Player : Character, IPlayerInteractable
	{
		public int PlayerIndex { get; set; } = -1;

		#region Unity messages

		public override void Awake()
		{
			base.Awake();
		}

		private void Start()
		{
			SetIndicatorColor(SquadStatus.Player);
			Mover.SetMoveIndicatorColor(Values.Color.Player);
		}

		private void Update()
		{
			base.ExecuteAction();
		}

		#endregion

		#region Public API

		public void SetIndicatorSelected(bool selected)
		{
			Indicator.SetSelected(selected);
		}

		new public void CancelActions()
		{
			base.CancelActions();
		}

		#endregion

		#region IPlayerInteractable implementation

		public Sprite GetIcon(Player p)
		{
			return ActionBase.LoadIcon(nameof(FollowAction));
		}

		public ActionBase GetAction(Player p)
		{
			return new FollowAction(p, transform);
		}

		#endregion	
	}
}
