using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace RedDust.Combat
{
	public class CharacterHealth : Health
	{
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