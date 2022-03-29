using System.Collections;
using UnityEngine;
using Utils;

namespace RedDust.Motion
{
	public class MovementIndicator : MonoBehaviour
	{
		private RaycastForNormal raycaster = null;
		private Color color = Values.Color.Neutral;

		public Color Color
		{
			get
			{
				return color;
			}
			set
			{
				color = value;
				var renderers = GetComponentsInChildren<SpriteRenderer>();
				foreach (var r in renderers) { r.color = color; }
			}
		}

		private void Awake()
		{
			raycaster = GetComponent<RaycastForNormal>();		
		}

		private void OnEnable()
		{
			raycaster.CastPerformed += OnCastPerformed;
			transform.rotation = Quaternion.Euler(new Vector3(-90, 0 , 0));
		}

		private void OnDisable()
		{
			raycaster.CastPerformed -= OnCastPerformed;
		}

		private void OnCastPerformed(Vector3 normal)
		{
			transform.forward = normal;
		}
	}
}