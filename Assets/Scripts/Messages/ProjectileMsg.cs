using Messaging;
using UnityEngine;
using RedDust.Combat;
using RedDust.Combat.Weapons;
using System.Collections.Generic;

namespace RedDust.Messages
{
	public class ProjectileMsg : IMessage
	{
		/// <summary>
		/// A message for a projectile without a sender.
		/// </summary>
		/// <param name="origin">Spawning position.</param>
		/// <param name="dir">Heading direction.</param>
		/// <param name="penetration">How many points of armor is ignored.</param>
		/// <param name="damage">How many points of damage done on hit.</param>
		/// <param name="prefab">The prefab source to spawn.</param>
		/// <param name="effects">The effects done on hit.</param>
		public ProjectileMsg(Vector3 origin, Vector3 dir, int penetration, int damage,
			Projectile prefab, Effect[] effects)
		{
			Origin = origin;
			Dir = dir;
			SenderId = -1;
			Prefab = prefab;
			WpnPenetration = penetration;
			WpnDamage = damage;
			Effects = effects;
		}

		/// <summary>
		/// A message to fire a projectile
		/// </summary>
		/// <param name="origin">Spawning position.</param>
		/// <param name="dir">Heading direction.</param>
		/// <param name="senderId">The InstanceID of the sender.</param>
		/// <param name="wpnData">Data from the weapon that shot this projectile.</param>
		public ProjectileMsg(Vector3 origin, Vector3 dir, int senderId, WeaponData wpnData)
		{
			Origin = origin;
			Dir = dir;
			SenderId = senderId;
			Prefab = wpnData.projectile;
			WpnPenetration = wpnData.penetration;
			WpnDamage = wpnData.damage;
			int split = wpnData.effects.Length;
			Effects = new Effect[split + wpnData.projectile.Effects.Length];		

			for (int i = 0; i < Effects.Length; i++)
			{
				Effects[i] = i < split ? wpnData.effects[i] : wpnData.projectile.Effects[split + i];
			}
		}

		/// <summary>
		/// The starting position (usually Weapon.Muzzle.position).
		/// </summary>
		public Vector3 Origin { get; }
		/// <summary>
		/// Starting direction (usually Weapon.Muzzle.forward).
		/// </summary>
		public Vector3 Dir { get; }
		/// <summary>
		/// The unique InstanceID of the sender.
		/// </summary>
		public int SenderId { get; }
		/// <summary>
		/// How many points of armor is ignored.
		/// </summary>
		public int WpnPenetration { get; }
		/// <summary>
		/// How many points of damage done on hit.
		/// </summary>
		public int WpnDamage { get; }
		/// <summary>
		/// The prefab source to spawn.
		/// </summary>
		public Projectile Prefab { get; }
		/// <summary>
		/// The effects done on hit.
		/// </summary>
		public Effect[] Effects { get; }
	}
}