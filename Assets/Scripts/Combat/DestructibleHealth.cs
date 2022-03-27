using System.Collections;
using UnityEngine;

namespace RedDust.Combat
{
	public class DestructibleHealth : Health
	{
		public override void Kill()
		{
			Debug.Log(gameObject.name + " destroyed");
			Destroy(gameObject);
		}
	}
}