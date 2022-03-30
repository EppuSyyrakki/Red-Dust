using RedDust.Messages;
using System.Collections;
using UnityEngine;
using Utils;

namespace RedDust.Combat.Weapons
{
	public class Weapon : MonoBehaviour
	{
		private ParticleSystem muzzleParticles = null;
		private bool hasParticles;

		public WeaponData Data { get; private set; }
		public Transform Muzzle { get; private set; }
		public Transform OffHandSlot { get; private set; }

		public void Set(WeaponData data, Transform muzzle)
		{
			Data = data;
			Muzzle = muzzle;
			muzzleParticles = muzzle.GetComponent<ParticleSystem>();
			OffHandSlot = transform.FindObjectWithTag(Values.Tag.OffHandSlot).transform;
			hasParticles = muzzleParticles != null;
		}
		
		public void Fire(Vector3 direction, int senderId)
		{
			var msg = new ProjectileMsg(Muzzle.position, direction, senderId, Data);
			Game.Instance.Bus.Send(msg);

			if (hasParticles) { muzzleParticles.Play(); }
		}
    }
}