using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using RedDust.Combat;
using Utils;

namespace RedDust.Control
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

		[SerializeField, Tooltip("How fast the character turns its head towards things.")]
		private float lookSpeed = 4f;

		[SerializeField, Tooltip("How fast the character aims their weapon towards targets")]
		private float aimSpeed = 6f;

		[SerializeField]
		private Vector3 defaultAimOffset = new Vector3(0, 1.5f, 2f);

		[SerializeField]
		private Transform lookTarget;

		[SerializeField]
		private MultiAimConstraint lookConstraint;

		[SerializeField]
		private Transform aimTarget;

		[SerializeField]
		private MultiAimConstraint aimConstraint;

		private Timer refreshTargetTimer = null;
		private Collider[] nearColliders;
		private IInteractable nearTarget = null;
		private Coroutine blend = null;
		private Vector3 attackTarget;
		private RigMode mode;
		private Fighter fighter;

		private void Awake()
		{
			refreshTargetTimer = new Timer(refreshTime, true, Random.Range(0, refreshTime));
			nearColliders = new Collider[maxNearTargets];
			fighter = GetComponentInParent<Fighter>();
		}

		private void Start()
		{
			SetRigMode(RigMode.Look);
		}

		private void OnEnable()
		{
			refreshTargetTimer.Alarm += OnRefreshTargetsTimer;
			fighter.AimingEnabled += OnFighterAimEnabled;
			fighter.Aiming += OnFighterAiming;
		}

		private void OnDisable()
		{
			refreshTargetTimer.Alarm -= OnRefreshTargetsTimer;
			fighter.AimingEnabled -= OnFighterAimEnabled;
			fighter.Aiming -= OnFighterAiming;
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

        private void Look()
        {
			var target = nearTarget == null
                                ? lookConstraint.transform.TransformPoint(defaultAimOffset)
                                : nearTarget.LookTarget.position;
            lookTarget.position = Vector3.MoveTowards(lookTarget.position, target, lookSpeed * Time.deltaTime);
        }

		private void LookAndAim()
        {
			lookTarget.position = Vector3.MoveTowards(lookTarget.position, attackTarget, lookSpeed * Time.deltaTime);
			aimTarget.position = Vector3.MoveTowards(aimTarget.position, attackTarget, aimSpeed * Time.deltaTime);
		}

		private void OnFighterAimEnabled(bool enabled)
        {
			if (enabled) 
			{ 
				SetRigMode(RigMode.Aim);	
			}
			else 
			{
				nearTarget = null;
				SetRigMode(RigMode.Look);
				aimTarget.position = aimConstraint.transform.TransformPoint(defaultAimOffset);
				lookTarget.position = aimTarget.position;
			}
		}

		private void OnFighterAiming(Vector3 target)
        {
			attackTarget = target;
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

        private void SetRigMode(RigMode targetMode)
        {           
            if (blend != null) { StopCoroutine(blend); }

            blend = StartCoroutine(BlendWeights(targetMode));

			if (enableLogging) { Debug.Log($"{gameObject.name} rig went from {mode} to {targetMode}"); }

			mode = targetMode;
		}

        private IEnumerator BlendWeights(RigMode mode)
        {
            float lookTarget = mode == RigMode.Look ? 1f : 0;
            float aimTarget = mode == RigMode.Aim ? 1f : 0;

            while (!Mathf.Approximately(lookConstraint.weight, lookTarget)
                && !Mathf.Approximately(aimConstraint.weight, aimTarget))
            {
                var delta = lookSpeed * Time.deltaTime;
                lookConstraint.weight = Mathf.Lerp(lookConstraint.weight, lookTarget, delta);
                aimConstraint.weight = Mathf.Lerp(aimConstraint.weight, aimTarget, delta);
                yield return null;
            }

			lookConstraint.weight = lookTarget;
			aimConstraint.weight = aimTarget;

			if (enableLogging)
            {
				Debug.Log($"{gameObject.name} Rig weight blending ended."
				+ $"lookWeight {lookConstraint.weight}, aimWeight {aimConstraint.weight}.");
			}			
        }
    }
}
