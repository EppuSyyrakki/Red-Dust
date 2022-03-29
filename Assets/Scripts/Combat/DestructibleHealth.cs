using System.Collections;
using UnityEngine;

namespace RedDust.Combat
{
	public class DestructibleHealth : Health
	{
        public override void Awake()
        {
            base.Awake();
			TargetingTransform = transform;
        }

        public override void Kill()
		{
			Debug.Log(gameObject.name + " destroyed");
			GetComponent<MeshRenderer>().enabled = false;
		}
	}
}