using System;
using System.Collections.Generic;
using UnityEngine;

namespace RedDust.Control
{
	public class Squad : MonoBehaviour
	{
		[SerializeField]
		private List<Character> members = new List<Character>();

		[SerializeField]
		private List<Squad> hostileSquads = new List<Squad>();

		[SerializeField]
		private List<Squad> friendlySquads = new List<Squad>();

		private void Awake()
		{
			// CharacterControls as childPort to this will get added as members
			List<Character> characters = new List<Character>();
			characters.AddRange(GetComponentsInChildren<Character>());	

			foreach (var c in characters)
			{
				members.Add(c);
				c.SetSquad(this);
			}
		}

		public bool HasMember(Character c)
		{
			return members.Contains(c);
		}

		public bool AddMember(Character c)
		{
			if (members.Contains(c)) { return false; }
			
			members.Add(c);
			return true;
		}

		public bool RemoveMember(Character c)
		{
			return members.Remove(c);
		}

		public bool RemoveHostileGroup(Squad s)
		{
			return hostileSquads.Remove(s);
		}

		public bool AddFriendlyGroup(Squad s)
		{
			if (!friendlySquads.Contains(s))
			{
				friendlySquads.Add(s);

				if (hostileSquads.Contains(s))
				{
					hostileSquads.Remove(s);
				}

				return true;
			}

			return false;
		}

		public bool RemoveFriendlyGroup(Squad s)
		{
			return friendlySquads.Remove(s);
		}

		public bool IsHostileTo(Character c)
		{
			foreach (var hostileSquad in hostileSquads)
			{
				if (hostileSquad.HasMember(c)) { return true; }
			}
			
			return false;
		}

		public bool IsFriendlyTo(Character c)
		{
			foreach (var friendlySquad in friendlySquads)
			{
				if (friendlySquad.HasMember(c)) { return true; }
			}

			return false;
		}

		public bool AddHostileGroup(Squad s)
		{
			if (!hostileSquads.Contains(s))
			{
				hostileSquads.Add(s);

				if (friendlySquads.Contains(s))
				{
					friendlySquads.Remove(s);
				}

				return true;
			}

			return false;
		}		

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
