using RedDust.Messages;
using System.Collections;
using UnityEngine;
using Utils;

namespace RedDust.Combat.Weapons
{
	public class Weapon : MonoBehaviour
	{
		[SerializeField]
		private bool showPivot;

		private ParticleSystem muzzleParticles = null;

		public WeaponData Data { get; private set; }
		public Transform Muzzle { get; private set; }
		public Transform OffHandSlot { get; private set; }
		public AnimatorOverrideController AnimOverride { get; private set; }

		public void Set(WeaponData data, Transform muzzle, AnimatorOverrideController animOverride)
		{
			Data = data;
			Muzzle = muzzle;
			muzzleParticles = muzzle.GetComponent<ParticleSystem>();
			OffHandSlot = transform.FindObjectWithTag(Values.Tag.OffHandSlot).transform;
			AnimOverride = animOverride;

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

        private void OnDrawGizmos()
        {
			if (showPivot)
            {
				Gizmos.DrawSphere(transform.position, 0.02f);
			}			
        }
    }
}