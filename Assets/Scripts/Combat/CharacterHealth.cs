using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using Utils;

namespace RedDust.Combat
{
	public class CharacterHealth : Health
	{
        public override void Awake()
        {
			base.Awake();
			TargetingTransform = transform.FindObjectWithTag(Values.Tag.CharTarget).transform;

			if (TargetingTransform == null)
            {
				Debug.LogError(gameObject.name + " doesn't have a transform marked as " + Values.Tag.CharTarget);
            }
		}

        public override void Kill()
		{
			Debug.Log(gameObject.name + " is ded");
			GetComponent<Animator>().enabled = false;
			// GetComponent<Collider>().enabled = false;
			GetComponent<NavMeshAgent>().enabled = false;
			transform.rotation = Quaternion.Euler(new Vector3(90, 0, 0));
		}
	}
}