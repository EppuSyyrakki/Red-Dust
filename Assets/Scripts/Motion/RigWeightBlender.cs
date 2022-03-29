using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Animations.Rigging;

namespace RedDust.Motion
{
    public class RigWeightBlender : MonoBehaviour
    {
        public  enum ConstraintType { None, MultiAim, TwoBone }     

        [SerializeField]
        private float startingWeight = 0;

        [SerializeField]
        private AnimationCurve inCurve, outCurve;

        [SerializeField]
        private float inTime, outTime;

        private ConstraintType constraint;
        private MultiAimConstraint multiAim;
        private TwoBoneIKConstraint twoBone;
        private Coroutine inRoutine;
        private Coroutine outRoutine;

        public float Weight 
        { 
            get 
            {
                if (constraint == ConstraintType.MultiAim) { return multiAim.weight; }
                else if (constraint == ConstraintType.TwoBone) { return twoBone.weight; }
                else { Debug.Log("Constraint Type is invalid!"); return -1; }
            } 
            private set
            {
                if (constraint == ConstraintType.MultiAim) { multiAim.weight = value; }
                else if (constraint == ConstraintType.TwoBone) { twoBone.weight = value; }
                else { Debug.Log("Constraint Type is invalid!"); }
            }
        }

        private void Awake()
        {
            if (TryGetComponent(out MultiAimConstraint aim))
            {
                multiAim = aim;
                constraint = ConstraintType.MultiAim;
            }
            else if (TryGetComponent(out TwoBoneIKConstraint bone))
            {
                twoBone = bone;
                constraint = ConstraintType.TwoBone;
            }

            Weight = startingWeight;
        }

        public void StartBlend(bool blendIn)
        {
            // Already at target weight, don't do anything.
            if ((blendIn && Weight == 1) || (!blendIn && Weight == 0)) { return; }

            if (blendIn)
            {
                // Start the blend in routine, but wait until blend out has finished if still running.
                inRoutine = StartCoroutine(Blend(inTime, outRoutine, inCurve));
                return;
            }

            // Start the blend out, but wait until blend in has finished if still running.
            outRoutine = StartCoroutine(Blend(outTime, inRoutine, outCurve));
        }

        public void SetBoneTarget(Transform target)
        {
            twoBone.data.target = target;
        }

        private IEnumerator Blend(float time, Coroutine mustBeFinished, AnimationCurve curve)
        {
            float t = 0;

            while (t < time)
            {
                if (mustBeFinished != null) { yield return null; }

                Weight = curve.Evaluate(t / time);
                t += Time.deltaTime;
                yield return null;
            }
        }
    }
}