using System;
using System.Collections;
using UnityEngine;
using Utils;
using RedDust.Combat.Weapons;

namespace RedDust.Combat
{
    [RequireComponent(typeof(CharacterHealth))]
    public class CombatControl : MonoBehaviour
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
                 
        private Animator animator;       
        private Transform rightHand;

        /// <summary>
        /// Broadcasts the transform of the off-hand when a weapon was created or put away. 
        /// If it is null, the weapon was put away (destroyed).
        /// </summary>
        public event Action<Transform> WeaponCreated;

        public Weapon Weapon { get; private set; }
        public float AttackFrequency => attackFreq;
        public CharacterHealth Health { get; private set; }

        private void Awake()
		{
            animator = GetComponent<Animator>();
            Health = GetComponent<CharacterHealth>();
            rightHand = transform.FindObjectWithTag(Values.Tag.RightHand).transform;
        }

        private void Start()
        {
            CreateWeapon(defaultWeaponConfig);
        }

        public void CreateWeapon(WeaponConfig wConfig)
		{
            Weapon = wConfig.Create(rightHand);
            SetOverride(Weapon.AnimOverride);
            WeaponCreated?.Invoke(Weapon.OffHandSlot);
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
            Vector3 unitSphere = UnityEngine.Random.insideUnitSphere.normalized;
			Vector3 toRangeLocalRandomized = toRangeLocal + unitSphere * statModifiers;
			return Weapon.Muzzle.TransformDirection(toRangeLocalRandomized.normalized);
        }

        private void SetOverride(AnimatorOverrideController aoc)
		{
            if (aoc != null)
            {
                animator.runtimeAnimatorController = aoc;
            }
            else
            {
                var overrideController = animator.runtimeAnimatorController as AnimatorOverrideController;

                if (overrideController != null)
                {
                    animator.runtimeAnimatorController = overrideController.runtimeAnimatorController;
                }
            }
        }
    }
}
