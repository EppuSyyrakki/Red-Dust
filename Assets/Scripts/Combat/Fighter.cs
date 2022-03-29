using System;
using System.Collections;
using UnityEngine;
using Utils;
using RedDust.Combat.Weapons;

namespace RedDust.Combat
{
    [RequireComponent(typeof(CharacterHealth))]
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
        [SerializeField]
        private float aimLayerSpeed = 0.3f;

        private const int aimLayer = Values.Animation.AimingLayer;     

        private Animator animator;
        private Coroutine aimBlend;

        public event Action<Vector3> Aiming;
        public event Action<bool> AimingEnabled;

        public Weapon Weapon { get; private set; }
        public float AttackFrequency => attackFreq;
        public CharacterHealth Health { get; private set; }
        /// <summary>
		/// Combat uses this transform's position as a "center of mass" target.
		/// </summary>
	

        private void Awake()
		{
            animator = GetComponent<Animator>();
            Health = GetComponent<CharacterHealth>();           
        }

        public void EnableAiming(bool enable)
		{
            if (aimBlend != null) { StopCoroutine(aimBlend); }

            float target = enable ? 1 : 0;
            aimBlend = StartCoroutine(BlendLayerTo(
                animator.GetLayerWeight(aimLayer), target, aimLayer, aimLayerSpeed));
            AimingEnabled?.Invoke(enable);
		}

        public void Aim(Vector3 target)
        {
            Aiming?.Invoke(target);
        }

        private IEnumerator BlendLayerTo(float currentWeight, float targetWeight, int layer, float time)
		{
            float t = 0;

            while (t < time)
			{
                if (Mathf.Approximately(currentWeight, targetWeight)) 
                {
                    currentWeight = targetWeight;
                }

                t += Time.deltaTime;
                animator.SetLayerWeight(layer, Mathf.Lerp(currentWeight, targetWeight, t / time));
                yield return null;
            }
		}

		public void CreateDefaultWeapon(Transform rHand, Transform lHand)
		{
            if (defaultWeaponConfig == null)
            {
                Debug.LogWarning(gameObject.name + " has no default weapon!");
                return; 
            }

            Weapon = defaultWeaponConfig.Create(rHand, lHand);
            SetOverride(Weapon.AnimOverride);
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
