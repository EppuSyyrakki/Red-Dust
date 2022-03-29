using System.Collections;
using UnityEngine;

namespace RedDust.Motion
{
    /// <summary>
    /// Help class to make the hand IKs follow the weapon hand slots.
    /// </summary>
    public class HandTargetMover : MonoBehaviour
    {
        public Transform Target { get; private set; }

        private void Update()
        {
            if (Target == null) { return; }

            transform.position = Target.position;
            transform.rotation = Target.rotation;
        }

        public void SetTarget(Transform target)
        {
            CancelInvoke();

            if (target == null)
            {
                Invoke(nameof(ResetTarget), 1f);
            }

            Target = target;
        }

        private void ResetTarget()
        {
            transform.position = transform.parent.position;
            transform.rotation = transform.parent.rotation;
        }
    }
}