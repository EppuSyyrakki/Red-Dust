using RedDust.Control.Actions;
using System;
using UnityEngine;

namespace RedDust.Control
{
	public class Player : Character, IPlayerInteractable
	{
		[SerializeField]
		private Sprite interactionIcon;

		public Transform Transform => transform;

		#region Unity messages

		public override void Awake()
		{
			base.Awake();
		}

		private void Start()
		{
			SetIndicatorColor(SquadStatus.Player);
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

		#endregion

		#region IPlayerInteractable implementation

		public Sprite GetIcon()
		{
			return interactionIcon;
		}

		public ActionBase GetAction(Player p)
		{
			return new FollowAction(p, transform);
		}

		#endregion	
	}
}
