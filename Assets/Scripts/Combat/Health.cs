using System.Collections;
using UnityEngine;

namespace RedDust.Combat
{
	public class Health : MonoBehaviour
	{
		[SerializeField]
		private int health = 1;

		public int Current => health;

		public void TakeDamage(int amount)
		{
			health = Mathf.Clamp(health, 0, health - amount);

			if (health <= 0) { Kill(); }
		}

		public void Kill()
		{
			Debug.Log(gameObject.name + " is ded");
			GetComponent<Animator>().enabled = false;
			transform.rotation = Quaternion.Euler(new Vector3(90, 0, 0));
		}
	}
}