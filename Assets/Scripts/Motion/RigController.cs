using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using RedDust.Combat;
using RedDust.Control;
using Utils;
using UnityEngine.Animations.Rigging;

namespace RedDust.Motion
{
	public enum RigMode { None, Interact, Look, HoldWeapon, Aim }

    public class RigController : MonoBehaviour
    {
		private const int maxNearTargets = Values.Animation.MaxNearTargets;
		private const int layerMask = Values.Layer.LookAts;
		private const float refreshTime = Values.Animation.LookRefresh;

		public bool enableLogging = false;

		[SerializeField, Range(0.5f, 5f), Tooltip("How far to look for interactables.")]
		private float lookRange = 3.5f;

		[SerializeField, Tooltip("Speed of lerp moving the lookTarget")]
		private float lookSpeed = 4f;

		[SerializeField, Tooltip("Speed of lerp moving the aimTarget")]
		private float aimSpeed = 6f;

		[SerializeField]
		private Vector3 defaultLookOffset = new Vector3(0, 1.5f, 2f);

		[SerializeField]
		private HandTargetMover leftHandMover;

		[SerializeField]
		private Transform lookTarget, aimTarget;

		[SerializeField]
		private RigWeightBlender lookBlender;

		[SerializeField]
		private RigWeightBlender aimBlender;

		[SerializeField]
		private RigWeightBlender rHandBlender;

		[SerializeField]
		private RigWeightBlender lHandBlender;

		private Timer refreshTargetTimer = null;
		private Collider[] nearColliders;
		private IInteractable nearTarget = null;
		private Vector3 weaponTarget;
		private RigMode mode;
		private CombatControl combat;
		private Vector3 defaultAimOffset;

        #region Unity messages

        private void Awake()
		{
			refreshTargetTimer = new Timer(refreshTime, true, Random.Range(0, refreshTime));
			nearColliders = new Collider[maxNearTargets];
			combat = GetComponentInParent<CombatControl>();
			defaultAimOffset = aimTarget.localPosition;
		}

		private void Start()
		{
			// SetRigMode(RigMode.Aim);
		}

		private void OnEnable()
		{
			refreshTargetTimer.Alarm += OnRefreshTargetsTimer;
			combat.CombatEntered += OnCombatEntered;
			combat.WeaponDrawn += OnCombatWeaponDrawn;
			combat.Aiming += OnCombatAiming;
			combat.AimingEnded += OnCombatAimingEnded;
		}

		private void OnDisable()
		{
			refreshTargetTimer.Alarm -= OnRefreshTargetsTimer;
			combat.CombatEntered -= OnCombatEntered;
			combat.WeaponDrawn -= OnCombatWeaponDrawn;
			combat.Aiming -= OnCombatAiming;
			combat.AimingEnded -= OnCombatAimingEnded;
		}

		private void Update()
		{
			if (mode == RigMode.Look || mode == RigMode.HoldWeapon)
            {
                refreshTargetTimer.Tick();
                Look();
            }
            else if (mode == RigMode.Aim)
            {
				LookAndAim();
            }
		}

        #endregion

        #region Private methods

        /// <summary>
        /// Move lookTarget toward nearest interactable, or look at default offset if none found.
        /// </summary>
        private void Look()
        {
			Vector3 look = nearTarget == null
                                ? lookBlender.transform.TransformPoint(defaultLookOffset)
                                : nearTarget.LookTarget.position;
            lookTarget.position = Vector3.MoveTowards(lookTarget.position, look, lookSpeed * Time.deltaTime);
			Vector3 aim = aimBlender.transform.InverseTransformPoint(defaultAimOffset);
			aimTarget.position = Vector3.MoveTowards(aimTarget.position, aim, aimSpeed * Time.deltaTime);
        }

		/// <summary>
		/// Move look and aim targets towards attackTarget (set by even subscription from Fighter).
		/// </summary>
		private void LookAndAim()
        {
			lookTarget.position = Vector3.MoveTowards(lookTarget.position, weaponTarget, lookSpeed * Time.deltaTime);
			aimTarget.position = Vector3.MoveTowards(aimTarget.position, weaponTarget, aimSpeed * Time.deltaTime);
		}

		private void SetRigMode(RigMode mode)
		{
			// Looking at things should be true in on all modes except None.
			bool look = mode != RigMode.None ? true : false;
			// Aim should be true only in Aim mode.
			bool aim = mode == RigMode.Aim ? true : false;
			// Constraining left hand should be false in None and Look modes.
			bool leftHand = mode == RigMode.None || mode == RigMode.Look ? false : true;
			// Constraining right hand should only be true in Interact.
			bool rightHand = mode == RigMode.Interact ? true : false;

			lookBlender.StartBlend(look);
			aimBlender.StartBlend(aim);
			lHandBlender.StartBlend(leftHand);
			rHandBlender.StartBlend(rightHand);
			this.mode = mode;
		}

		#endregion

		#region Event handlers

		private void OnCombatEntered(bool entered)
        {
			if (entered) 
			{
				// Interact before HoldWeapon. The Grab weapon animation will call to change it to
				// HoldWeapon once its completed via an event in CombatControl.
				SetRigMode(RigMode.Interact);
			}
			else 
			{
				nearTarget = null;
				SetRigMode(RigMode.Look);
				weaponTarget = aimBlender.transform.InverseTransformPoint(defaultAimOffset);
				lookTarget.position = lookBlender.transform.InverseTransformPoint(defaultLookOffset);
			}
		}

		private void OnCombatAiming(Vector3 target)
        {
			// Check first, because this gets called every frame the character is aiming.
			if (mode != RigMode.Aim) { SetRigMode(RigMode.Aim); }

			weaponTarget = target;
        }

		private void OnCombatAimingEnded()
        {
			if (mode != RigMode.HoldWeapon) { SetRigMode(RigMode.HoldWeapon); }
        }

		// Gets called when the "draw weapon" animation is complete.
		private void OnCombatWeaponDrawn(Transform offHandSlot)
        {
			SetRigMode(RigMode.HoldWeapon);
			leftHandMover.SetTarget(offHandSlot);
        }

        private void OnRefreshTargetsTimer()
		{
			if (mode != RigMode.Look)
			{
				nearTarget = null;
				return;
			}

			var halfRange = lookRange * 0.5f;
			var center = transform.position + Vector3.up + transform.forward * halfRange;
			int hitCount = Physics.OverlapSphereNonAlloc(center, halfRange, nearColliders, layerMask);
			List<IInteractable> nearTargets = new List<IInteractable>(maxNearTargets);

			for (int i = 0; i < hitCount; i++)
			{
				var col = nearColliders[i];

				if (transform.IsChildOf(col.transform)
					|| !col.gameObject.TryGetComponent(out IInteractable interactable)) { continue; }

				nearTargets.Add(interactable);
			}

			if (nearTargets.Count == 0)
			{
				nearTarget = null;
				return;
			}

			var orderedTargets = nearTargets.OrderBy
				(r => (r.LookTarget.position - transform.position).sqrMagnitude).ToList();
			nearTarget = orderedTargets[0];
		}

        #endregion
    }
}
