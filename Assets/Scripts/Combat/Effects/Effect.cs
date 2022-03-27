using UnityEngine;

namespace RedDust.Combat
{
	public abstract class Effect : MonoBehaviour
	{
		[SerializeField]
		private bool isExplosive = false;

		public bool IsExplosive { get => isExplosive; }

		public RaycastHit Hit { get; set; }
	}
}