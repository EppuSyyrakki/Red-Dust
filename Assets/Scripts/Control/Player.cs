using RedDust.Control.Actions;
using System;
using UnityEngine;

namespace RedDust.Control
{
	/// <summary>
	/// No other characters should be Players except those in Squad with PlayerSquad tag.
	/// </summary>
	public class Player : Character, IInteractable
	{
		public int PlayerIndex { get; set; } = -1;

		#region Unity messages

		public override void Awake()
		{
			base.Awake();
		}

		public override void Start()
		{
			base.Start();
			SetIndicatorColor(SquadStatus.Player);
			Motion.SetMoveIndicatorColor(Values.Color.Player);
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

		#region IInteractable implementation

		public Transform LookTarget => Head;

		public Sprite GetIcon(Player p)
		{
			return ActionBase.LoadIcon(nameof(FollowAction));
		}

		public ActionBase GetAction(Character c)
		{
			return new FollowAction(c, transform);
		}

		#endregion	
	}
}
