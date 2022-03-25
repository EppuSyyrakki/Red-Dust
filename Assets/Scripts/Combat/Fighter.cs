using RedDust.Messages;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RedDust.Combat
{
    [RequireComponent(typeof(Health), typeof(Animator))]
    public class Fighter : MonoBehaviour
    {
        [Tooltip("Character data")]
        [SerializeField]
        private float weaponSkill = 1;

        [SerializeField]
        private float attackFreq = 5;

        [Tooltip("Weapon data")]

        [SerializeField]
        private Projectile projectile;

        [SerializeField]
        private Transform muzzle;

        private Animator animator;

        public float AttackFrequency => attackFreq;

		private void Awake()
		{
            animator = GetComponent<Animator>();
		}

		public void Attack(Vector3 target)
		{
            var direction = GetProjectileDirection();
            var msg = new ProjectileMsg(muzzle.position, direction, GetInstanceID(), projectile);
            Game.Instance.Bus.Send(msg);
		}

        private Vector3 GetProjectileDirection()
		{
            // Calculate the actual direction here from skill & weapon accuracy.
            return muzzle.forward;
		}
    }
}
