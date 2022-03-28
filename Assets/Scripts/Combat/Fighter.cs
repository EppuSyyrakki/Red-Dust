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
        [SerializeField]
        private float aimLayerSpeed = 0.3f;

        private const int aimLayer = Values.Animation.AimLayer;     

        private Animator animator;
        private Coroutine aimBlend;

        public Weapon Weapon { get; private set; }
        public float AttackFrequency => attackFreq;
        public CharacterHealth Health { get; private set; }
        public WeaponConfig DefaultWeaponCfg => defaultWeaponConfig;

        private void Awake()
		{
            animator = GetComponent<Animator>();
            Health = GetComponent<CharacterHealth>();

        }

        /// <summary>
        /// Starts to blend animator layer weight towards aiming.
        /// </summary>
        public void StartAim()
		{
            if (aimBlend != null) { StopCoroutine(aimBlend); }

            aimBlend = StartCoroutine(BlendLayerTo(
                animator.GetLayerWeight(aimLayer), 1, aimLayer, aimLayerSpeed));
		}

        /// <summary>
        /// Starts to blend animator layer weight towards base layer.
        /// </summary>
        public void EndAim()
		{
            if (aimBlend != null) { StopCoroutine(aimBlend); }

            aimBlend = StartCoroutine(BlendLayerTo(
                animator.GetLayerWeight(aimLayer), 0, aimLayer, aimLayerSpeed * 2f));
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

            if (Weapon.AnimOverride != null)
			{
                animator.runtimeAnimatorController = Weapon.AnimOverride;
            }          
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
