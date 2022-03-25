using Messaging;
using System.Collections;
using UnityEngine;
using RedDust.Combat;

namespace RedDust.Messages
{
	public class ProjectileMsg : IMessage
	{
		public ProjectileMsg(Vector3 origin, Vector3 dir, int senderId, Projectile prefab)
		{
			Origin = origin;
			Dir = dir;
			SenderId = senderId;
			Prefab = prefab;
		}

		public Vector3 Origin { get; }
		public Vector3 Dir { get; }
		public int SenderId { get; }
		public Projectile Prefab { get; }
	}
}