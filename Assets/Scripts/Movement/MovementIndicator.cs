using System.Collections;
using UnityEngine;
using Utils;

namespace RedDust.Movement
{
	public class MovementIndicator : MonoBehaviour
	{
		private RaycastForNormal _raycaster = null;
		private Color _color = Game.Color.Neutral;

		public Color Color
		{
			get
			{
				return _color;
			}
			set
			{
				_color = value;
				var renderers = GetComponentsInChildren<SpriteRenderer>();
				foreach (var r in renderers) { r.color = _color; }
			}
		}

		private void Awake()
		{
			_raycaster = GetComponent<RaycastForNormal>();		
		}

		private void OnEnable()
		{
			_raycaster.CastPerformed += OnCastPerformed;
			transform.rotation = Quaternion.Euler(new Vector3(-90, 0 , 0));
		}

		private void OnDisable()
		{
			_raycaster.CastPerformed -= OnCastPerformed;
		}

		private void OnCastPerformed(Vector3 normal)
		{
			transform.forward = normal;
		}
	}
}