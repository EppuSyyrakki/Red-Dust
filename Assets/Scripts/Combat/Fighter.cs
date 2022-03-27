using RedDust.Combat.Weapons;
using RedDust.Messages;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils;

namespace RedDust.Combat
{
    public class Fighter : MonoBehaviour
    {
        [Tooltip("Character data")]
        [SerializeField]
        private int weaponSkill = 1;

        [SerializeField]
        private int perception = 1;

        [SerializeField]
        private float attackFreq = 5;

        [SerializeField]
        private WeaponConfig defaultWeaponConfig;

        public Weapon Weapon { get; private set; }

        private Animator animator;

        public float AttackFrequency => attackFreq;

        public CharacterHealth Health { get; private set; }
        public WeaponConfig DefaultWeaponCfg => defaultWeaponConfig;

        private void Awake()
		{

            animator = GetComponent<Animator>();
            Health = GetComponent<CharacterHealth>();
        }

        public void CreateDefaultWeapon(Transform rHand, Transform lHand)
		{
            if (defaultWeaponConfig == null) 
            {
                Debug.LogWarning(gameObject.name + " has no default weapon!");
                return; 
            }

            Weapon = defaultWeaponConfig.Create(rHand, lHand);
        }

		public void Shoot(Vector3 target)
		{
            Weapon.Fire(GetProjectileDirection(target), GetInstanceID());          
		}

        private Vector3 GetProjectileDirection(Vector3 target)
        {
            Vector3 muzzle = Weapon.Muzzle.position;
            Vector3 toTargetLocal = Weapon.Muzzle.InverseTransformDirection(target - muzzle);
            
            // Project the target direction to weaponRange
            Vector3 toRangeLocal = toTargetLocal.normalized * Weapon.Data.range;

            // Just some testing mod alues
            float statModifiers = 1f / (weaponSkill + perception) * 10f;

			// Get a random sphere multiplied by mod values
			Vector3 toRangeLocalRandomized = toRangeLocal + Random.insideUnitSphere.normalized * statModifiers;
			return Weapon.Muzzle.TransformDirection(toRangeLocalRandomized.normalized);
        }
    }
}
