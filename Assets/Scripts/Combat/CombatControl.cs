using System;
using System.Collections;
using UnityEngine;
using Utils;
using RedDust.Combat.Weapons;

namespace RedDust.Combat
{
    // TODO: Change the weapon spawning to happen only at AnimationGrabSmallHolster(). Until then, only store
    // a prop version of the weapon.
    [RequireComponent(typeof(CharacterHealth))]
    public class CombatControl : MonoBehaviour
    {
        private const string TAG_RIGHT_HAND = Values.Tag.RightHand;
        private const string TAG_SMALL_HOLSTER = Values.Tag.SmallHolster;
        private const int ANIM_LAYER_COMBAT = Values.Animation.CombatLayer;
        private const string ANIM_WEAPON_TYPE = Values.Animation.WeaponType;
        private const string ANIM_IS_IN_COMBAT = Values.Animation.IsInCombat;
        private const string ANIM_WEAPON_DRAWN = Values.Animation.WeaponDrawn;
        private const float RELAX_TIMER = Values.Combat.RelaxTimer;

        [SerializeField]
        private WeaponConfig unarmed = null;
        [SerializeField]
        private float aimLayerBlendTime = 0.2f;
        [Tooltip("Character data")]
        [SerializeField]
        private int weaponSkill = 1;
        [SerializeField]
        private int perception = 1;
        [SerializeField]
        private float attackFreq = 5;
        [SerializeField]
        private WeaponConfig smallSlotConfig;
        [SerializeField]
        private WeaponConfig largeSlotConfig;

        [SerializeField]
        private float delayFromWeaponToCombat = 5f;
                      
        private Transform smallSlot;
        private Transform largeSlot;
        private Animator animator;
        private int instanceId;
        private bool isInCombat;
        private Coroutine aimBlend;
        private Weapon smallWeapon;
        private Weapon largeWeapon;
        private bool currentWeaponIsUnarmed;

        /// <summary>
        /// Broadcasts if combat mode (weapon drawn was started (true) or finished (false).
        /// </summary>
        public event Action<bool> CombatEntered;
        /// <summary>
        /// Broadcasts the transform of the off-hand when a weapon was drawn or put away. 
        /// If it is null, the weapon was put away (destroyed).
        /// </summary>
        public event Action<Transform> WeaponDrawn;
        /// <summary>
		/// Broadcasts location of aimed object. Used by Actions.
		/// </summary>
		public event Action<Vector3> Aiming;
        /// <summary>
        /// Called when aiming can stop (Combat action ends).
        /// </summary>
        public event Action AimingEnded;

        /// <summary>
        /// When this runs out, will call EnterCombat(false). 
        /// </summary>
        public Timer<bool> RelaxTimer { get; private set; }
        public Weapon CurrentWeapon { get; private set; }
        public float AttackFrequency => attackFreq;
        public CharacterHealth Health { get; private set; }
        public Transform RightHand { get; private set; }
        public bool IsInCombat => isInCombat;       

        #region Unity messages

        private void Awake()
		{
            Health = GetComponent<CharacterHealth>();
            RightHand = transform.FindObjectWithTag(TAG_RIGHT_HAND).transform;
            smallSlot = transform.FindObjectWithTag(TAG_SMALL_HOLSTER).transform;
            animator = GetComponent<Animator>();
            instanceId = GetInstanceID();
            RelaxTimer = new Timer<bool>(false, RELAX_TIMER, resetOnAlarm: false);
        }

        private void Start()
        {
            

            if (smallSlotConfig != null) { smallWeapon = CreateWeapon(smallSlotConfig, smallSlot); }
            if (largeSlotConfig != null) { largeWeapon = CreateWeapon(largeSlotConfig, largeSlot); }
        }

        private void OnEnable()
        {
            RelaxTimer.Alarm += EnterCombat;           
        }

        private void OnDisable()
        {
            RelaxTimer.Alarm -= EnterCombat;
        }

        private void Update()
        {
            if (isInCombat) { RelaxTimer.Tick(); }
        }

        #endregion

        #region Private methods

        private Weapon CreateWeapon(WeaponConfig config, Transform targetSlot)
		{
            var wpn = config.Create(targetSlot);           
            return wpn;
        }

        private Vector3 GetProjectileDirection(Vector3 target)
        {
            Vector3 muzzle = CurrentWeapon.Muzzle.position;
            Vector3 toTargetLocal = CurrentWeapon.Muzzle.InverseTransformDirection(target - muzzle);

            // Project the target direction to weaponRange
            Vector3 toRangeLocal = toTargetLocal.normalized * CurrentWeapon.Data.range;

            // Just some testing mod alues
            float statModifiers = 1f / (weaponSkill + perception) * 10f;

            // Get a random sphere multiplied by mod values
            Vector3 unitSphere = UnityEngine.Random.insideUnitSphere.normalized;
            Vector3 toRangeLocalRandomized = toRangeLocal + unitSphere * statModifiers;
            return CurrentWeapon.Muzzle.TransformDirection(toRangeLocalRandomized.normalized);
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

        private void SetWeaponToUnarmed()
        {
            var weapon = unarmed.Create(RightHand);

            currentWeaponIsUnarmed = true;
        }

        private void SetupCurrentWeapon(Weapon weapon)
        {
            if (currentWeaponIsUnarmed) { WeaponConfig.DestroyOldWeapon(RightHand); }

            if (!isInCombat) { EnterCombat(true); }

            CurrentWeapon = weapon;
            animator.SetInteger(ANIM_WEAPON_TYPE, (int)weapon.Data.type);
            animator.SetTrigger(ANIM_WEAPON_DRAWN);            
        }

        #endregion

        #region Public API

        public bool DrawSmallHolster()
        {
            if (smallWeapon == null) { return false; }

            SetupCurrentWeapon(smallWeapon);
            return true;
        }

        public bool DrawLargeHolster()
        {
            if (largeWeapon == null) { return false; }

            SetupCurrentWeapon(largeWeapon);
            return true;
        }

        /// <summary>
        /// Invokes an event that broadcasts the aiming target. By default RigController listens to it to
        /// ensure the rig constraints point to the target.
        /// </summary>
        public void Aim(Vector3 target)
        {
            Aiming?.Invoke(target);
        }

        /// <summary>
        /// Invokes an event. By default RigController listens to blend out the aiming constraints.
        /// </summary>
        public void EndAim()
        {
            AimingEnded?.Invoke();
        }

        /// <summary>
        /// Makes CurrentWeapon go pew. Also resets the Relax Timer.
        /// </summary>
        /// <param name="target">Where the shot should be aimed at.</param>
        public void Shoot(Vector3 target)
		{
            RelaxTimer.Reset();
            CurrentWeapon.Fire(GetProjectileDirection(target), instanceId);
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="enter"></param>
        public void EnterCombat(bool enter)
        {
            isInCombat = enter;

            if (enter) { RelaxTimer.Reset(); }

            // Try drawing large weapon, then small. If no success, get the unarmed "weapon".
            if (!DrawLargeHolster() && !DrawSmallHolster()) { SetWeaponToUnarmed(); }

            animator.SetBool(ANIM_IS_IN_COMBAT, isInCombat);
            CombatEntered?.Invoke(enter);
            var currentLayerWeight = animator.GetLayerWeight(ANIM_LAYER_COMBAT);           

            // Unarmed weapons don't use the combat layer.
            if (currentWeaponIsUnarmed) { return; }  

            if ((isInCombat && currentLayerWeight == 1)
                || (!isInCombat && currentLayerWeight == 0)
                || CurrentWeapon.Data.type == 0) { return; }

            if (aimBlend != null) { StopCoroutine(aimBlend); }

            float target = isInCombat ? 1 : 0;
            aimBlend = StartCoroutine(BlendLayerTo(currentLayerWeight, target, ANIM_LAYER_COMBAT, aimLayerBlendTime));
        }

        #endregion

        #region Animation event handlers.


        private void AnimationGrabSmallHolster()
        {
            CurrentWeapon.transform.SetParent(RightHand);
            CurrentWeapon.transform.position = RightHand.position;
            CurrentWeapon.transform.rotation = RightHand.rotation;
        }

        private void AnimationWeaponDrawComplete()
        {
            WeaponDrawn?.Invoke(CurrentWeapon.OffHandSlot);
        }

        #endregion
    }
}
