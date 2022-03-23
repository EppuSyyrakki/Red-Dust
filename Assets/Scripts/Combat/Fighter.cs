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

        [SerializeField]
        private float attackSpeed;

        [Tooltip("Weapon data")]
        [SerializeField]
        private float weaponRange;

        [SerializeField]
        private float weaponAccuracy;

        private Animator animator;

        public float AttackSpeed => attackSpeed;

		private void Awake()
		{
            animator = GetComponent<Animator>();
		}

		public void Attack(Vector3 target)
		{
            Debug.Log(gameObject.name + " attacked " + GetGameObjectAtPosition(target));
		}

        private GameObject GetGameObjectAtPosition(Vector3 pos)
		{
            Collider[] cols = Physics.OverlapBox(pos, Vector3.one * 0.2f);

            if (cols != null)
			{
                return cols[0].gameObject;
			}

            return null;
		}
    }
}
