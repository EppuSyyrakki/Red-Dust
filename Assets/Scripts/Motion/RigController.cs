using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using RedDust.Combat;
using RedDust.Control;
using Utils;
using UnityEngine.Animations.Rigging;

namespace RedDust.Motion
{
	public enum RigMode { None, Look, Aim }

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
		private Vector3 defaultAimOffset = new Vector3(0, 1.5f, 2f);

		[SerializeField]
		private Vector3 defaultRestOffset = new Vector3(-1.2f, 0, 1f);

		[SerializeField]
		private HandTargetMover offHandMover;

		[SerializeField]
		private Transform lookTarget, aimTarget;

		[SerializeField]
		private RigWeightBlender lookBlender;

		[SerializeField]
		private RigWeightBlender[] aimBlenders;

		private Timer refreshTargetTimer = null;
		private Collider[] nearColliders;
		private IInteractable nearTarget = null;
		private Vector3 attackTarget;
		private RigMode mode;
		private MotionControl motion;
		private CombatControl combat;

        #region Unity messages

        private void Awake()
		{
			// Time.timeScale = 0.1f;
			refreshTargetTimer = new Timer(refreshTime, true, Random.Range(0, refreshTime));
			nearColliders = new Collider[maxNearTargets];
			motion = GetComponentInParent<MotionControl>();
			combat = GetComponentInParent<CombatControl>();
		}

		private void Start()
		{
			// SetRigMode(RigMode.Look);
		}

		private void OnEnable()
		{
			refreshTargetTimer.Alarm += OnRefreshTargetsTimer;
			motion.AimingEnabled += OnMotionAimEnabled;
			motion.Aiming += OnMotionAiming;
			combat.WeaponCreated += OnCombatWeaponCreated;
		}

		private void OnDisable()
		{
			refreshTargetTimer.Alarm -= OnRefreshTargetsTimer;
			motion.AimingEnabled -= OnMotionAimEnabled;
			motion.Aiming -= OnMotionAiming;
			combat.WeaponCreated -= OnCombatWeaponCreated;
		}

		private void Update()
		{
			if (mode == RigMode.Look)
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
			var target = nearTarget == null
                                ? lookBlender.transform.TransformPoint(defaultAimOffset)
                                : nearTarget.LookTarget.position;
            lookTarget.position = Vector3.MoveTowards(lookTarget.position, target, lookSpeed * Time.deltaTime);
        }

		/// <summary>
		/// Move look and aim targets towards attackTarget (set by even subscription from Fighter).
		/// </summary>
		private void LookAndAim()
        {
			lookTarget.position = Vector3.MoveTowards(lookTarget.position, attackTarget, lookSpeed * Time.deltaTime);
			aimTarget.position = Vector3.MoveTowards(aimTarget.position, attackTarget, aimSpeed * Time.deltaTime);
		}

		private void SetRigMode(RigMode target)
		{
			mode = target;
			// start blend in for look if mode is either Look or Aim, otherwise blend out
			bool look = mode == RigMode.Look || mode == RigMode.Aim ? true : false;
			// start blend in for aim if mode is Aim, otherwise blend out
			bool aim = mode == RigMode.Aim ? true : false;

			foreach (var aimBlender in aimBlenders)
            {
				aimBlender.StartBlend(aim);
            }

			lookBlender.StartBlend(look);
		}

		#endregion

		#region Event handlers

		private void OnMotionAimEnabled(bool enabled)
        {
			if (enabled) 
			{ 
				SetRigMode(RigMode.Aim);	
			}
			else 
			{
				nearTarget = null;
				SetRigMode(RigMode.Look);
				aimTarget.position = lookBlender.transform.TransformPoint(defaultAimOffset);
				lookTarget.position = aimTarget.position;
			}
		}

		private void OnMotionAiming(Vector3 target)
        {
			attackTarget = target;
        }

		private void OnCombatWeaponCreated(Transform offHandSlot)
        {
			offHandMover.SetTarget(offHandSlot);
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
