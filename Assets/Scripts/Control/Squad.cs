using RedDust.Messages;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace RedDust.Control
{
	public enum SquadStatus { Neutral, Friendly, Hostile, Player }

	public class Squad : MonoBehaviour
	{
		[SerializeField]
		private Squad[] initialHostileSquads = null;

		[SerializeField]
		private Squad[] initialFriendlySquads = null;

		private List<Character> _members = new List<Character>();
		private List<Squad> _hostiles = new List<Squad>();
		private List<Squad> _friendlies = new List<Squad>();

		private bool _isPlayerSquad = false;

		public List<Character> Members => _members;
		public Action<Character, bool> MembersModified;

		#region Unity messages

		private void Awake()
		{
			// This feels hacky but affords the same component for Player and NPC squads.
			_isPlayerSquad = gameObject.CompareTag(Game.Tag.PlayerSquad);

			// Any Characters as child to this will get added as members
			foreach (Transform child in transform)
			{
				if (!child.TryGetComponent(out Character c)) { continue; }

				_members.Add(c);
				c.SetSquad(this);
			}
		}

		private void Start()
		{
			// Do this in start so other squads are Awake and can listen to a message from
			// PlayerSquad when it does this - it will change their hostility indicators.
			foreach (var s in initialFriendlySquads) { AddFriendlySquad(s); }
			foreach (var s in initialHostileSquads) { AddHostileSquad(s); }
		}

		#endregion		

		private void TrySendMsg(Squad s, SquadStatus status)
		{
			if (!_isPlayerSquad) { return; }
			
			var msg = new PlayerSquadMsg(s, status);
			Game.Instance.Bus.Send(msg);		
		}

		#region Public API

		public bool HasMember(Character c)
		{
			return _members.Contains(c);
		}

		public bool AddMember(Character c)
		{
			if (_members.Contains(c)) { return false; }

			MembersModified?.Invoke(c, true);
			_members.Add(c);
			return true;
		}

		public bool RemoveMember(Character c)
		{
			if (_members.Remove(c)) 
			{
				MembersModified?.Invoke(c, false);
				return true; 
			}

			return false;
		}

		public bool AddHostileSquad(Squad s)
		{
			if (!_hostiles.Contains(s))
			{
				_friendlies.Remove(s);
				_hostiles.Add(s);
				TrySendMsg(s, SquadStatus.Hostile);
				return true;
			}

			return false;
		}

		public bool RemoveHostileSquad(Squad s)
		{
			if (!_hostiles.Remove(s)) { return false; }

			TrySendMsg(s, SquadStatus.Neutral);
			return true;
		}

		public bool AddFriendlySquad(Squad s)
		{
			if (!_friendlies.Contains(s))
			{
				_hostiles.Remove(s);
				_friendlies.Add(s);
				TrySendMsg(s, SquadStatus.Friendly);
				return true;
			}

			return false;
		}

		public bool RemoveFriendlySquad(Squad s)
		{
			if (!_friendlies.Remove(s)) { return false; }

			TrySendMsg(s, SquadStatus.Neutral);
			return _friendlies.Remove(s);
		}

		public bool IsHostileTo(Character c)
		{
			foreach (var hostileSquad in _hostiles)
			{
				if (hostileSquad.HasMember(c)) { return true; }
			}
			
			return false;
		}

		public bool IsFriendlyTo(Character c)
		{
			foreach (var friendlySquad in _friendlies)
			{
				if (friendlySquad.HasMember(c)) { return true; }
			}

			return false;
		}

		#endregion

		///// <summary>
		///// Alert other group members to an enemy, adds enemy group to hostile groups if not already hostile.
		///// </summary>
		///// <param name="c">The enemy controller</param>
		//public void AlertMembersTo(Character c)
		//{
		//	if (!hostileGroups.Contains(c.Squad))
		//	{
		//		AddHostileGroup(c.Squad);
		//	}

		//	// TODO: Change alerting to only alert members close by (shouting distance basically)

		//	foreach (var member in members)
		//	{
		//		if (!member.Memory.Contains(c))
		//		{
		//			member.Memory.Add(c, new EnemySighting(c.transform.position, Time.time));
		//		}
		//	}
		//}

		///// <summary>
		///// Alert other group members to a Point Of Interest.
		///// </summary>
		///// <param name="p">The point of interest</param>
		//public void AlertMembersTo(PointOfInterest p)
		//{
		//	foreach (var member in members)
		//	{
		//		member.Memory.Add(p);
		//	}
		//}
	}
}
