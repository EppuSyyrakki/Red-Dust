using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RedDust.Combat
{
    public class Fighter : MonoBehaviour
    {
        [Tooltip("Character data")]
        [SerializeField]
        private float weaponSkill;

        [Tooltip("Weapon data")]
        [SerializeField]
        private float weaponRange;

        [SerializeField]
        private float weaponAccuracy;

        private Animator animator;

		private void Awake()
		{
            animator = GetComponent<Animator>();
		}

		public void Attack()
		{

		}
    }
}
