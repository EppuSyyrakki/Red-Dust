using RedDust.Messages;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace RedDust.Control
{
	public enum SquadStatus { Neutral, Allied, Hostile, Player }

	public class Squad : MonoBehaviour
	{
		[SerializeField]
		private Squad[] initialHostileSquads = null;

		[SerializeField]
		private Squad[] initialAlliedSquads = null;

		private List<Character> members = new List<Character>();
		[SerializeField]
		private List<Squad> hostiles = new List<Squad>();
		[SerializeField]
		private List<Squad> allies = new List<Squad>();
		
		private bool isPlayerSquad = false;

		public Character[] Members => members.ToArray();
		public Action<Character, bool> MembersModified;

		#region Unity messages

		private void Awake()
		{
			// This feels hacky but affords the same component for Player and NPC squads.
			isPlayerSquad = gameObject.CompareTag(Values.Tag.PlayerSquad);

			// Any Characters as child to this will get added as members
			foreach (Transform child in transform)
			{
				if (!child.TryGetComponent(out Character c)) { continue; }

				members.Add(c);
				c.SetSquad(this);
			}
		}

		private void Start()
		{
			// Do this in start so other squads are Awake and can listen to a message from
			// PlayerSquad when it does this - it will change their hostility indicators.
			foreach (var s in initialAlliedSquads) { AddAlliedSquad(s); }
			foreach (var s in initialHostileSquads) { AddHostileSquad(s); }
		}

		#endregion		

		private void TrySendMsg(Squad s, SquadStatus status)
		{
			if (!isPlayerSquad) { return; }
			
			var msg = new PlayerSquadMsg(s, status);
			Game.Instance.Bus.Send(msg);
		}

		#region Public API

		public bool HasMember(Character c)
		{
			return members.Contains(c);
		}

		public bool AddMember(Character c)
		{
			if (members.Contains(c)) { return false; }

			MembersModified?.Invoke(c, true);
			members.Add(c);
			return true;
		}

		public bool RemoveMember(Character c)
		{
			if (members.Remove(c)) 
			{
				MembersModified?.Invoke(c, false);
				return true; 
			}

			return false;
		}

		public bool AddHostileSquad(Squad s)
		{
			if (!hostiles.Contains(s))
			{
				allies.Remove(s);
				hostiles.Add(s);
				TrySendMsg(s, SquadStatus.Hostile);
				s.AddHostileSquad(this);
				return true;
			}

			return false;
		}

		public bool RemoveHostileSquad(Squad s)
		{
			if (!hostiles.Remove(s)) { return false; }

			TrySendMsg(s, SquadStatus.Neutral);
			s.RemoveHostileSquad(this);
			return true;
		}

		public bool AddAlliedSquad(Squad s)
		{
			if (!allies.Contains(s))
			{
				hostiles.Remove(s);
				allies.Add(s);
				TrySendMsg(s, SquadStatus.Allied);
				s.AddAlliedSquad(this);
				return true;
			}

			return false;
		}

		public bool RemoveAlliedSquad(Squad s)
		{
			if (!allies.Remove(s)) { return false; }

			TrySendMsg(s, SquadStatus.Neutral);
			s.RemoveAlliedSquad(this);
			return allies.Remove(s);
		}

		public bool IsHostileTo(Character c)
		{
			foreach (var hostileSquad in hostiles)
			{
				if (hostileSquad.HasMember(c)) { return true; }
			}
			
			return false;
		}

		public bool IsHostileTo(Squad s)
		{
			return hostiles.Contains(s);
		}

		public bool IsAlliedTo(Character c)
		{
			foreach (var friendlySquad in allies)
			{
				if (friendlySquad.HasMember(c)) { return true; }
			}

			return false;
		}

		public bool IsAlliedTo(Squad s)
		{
			return allies.Contains(s);
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
