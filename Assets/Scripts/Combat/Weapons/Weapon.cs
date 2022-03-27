using RedDust.Messages;
using System.Collections;
using UnityEngine;
using Utils;

namespace RedDust.Combat.Weapons
{
	public class Weapon : MonoBehaviour
	{
		private ParticleSystem muzzleParticles = null;

		public WeaponData Data { get; private set; }
		public Transform Muzzle { get; private set; }
		public Transform RightHandSlot { get; private set; }
		public Transform LeftHandSlot { get; private set; }

		public void Set(WeaponData data, Transform userRightHand, Transform userLeftHand, Transform muzzle)
		{
			Data = data;
			Muzzle = muzzle;
			muzzleParticles = muzzle.GetComponent<ParticleSystem>();
			RightHandSlot = transform.FindObjectWithTag(Values.Tag.RHandSlot).transform;
			LeftHandSlot = transform.FindObjectWithTag(Values.Tag.LHandSlot).transform;

			if (muzzleParticles == null) 
			{ 
				Debug.LogWarning($"{Data.name} doesnt have a particle system in the muzzle."); 
			}
		}
		
		public void Fire(Vector3 direction, int senderId)
		{
			var msg = new ProjectileMsg(Muzzle.position, direction, senderId, Data);
			Game.Instance.Bus.Send(msg);
			muzzleParticles.Play();
		}
	}
}