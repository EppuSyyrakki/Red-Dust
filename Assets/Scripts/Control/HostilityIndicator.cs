using UnityEngine;
using Utils;

namespace RedDust.Control
{
	public class HostilityIndicator : MonoBehaviour
	{
		[SerializeField]
		private Sprite selectedSprite;

		[SerializeField]
		private Sprite unselectedSprite;

		private SpriteRenderer _indicatorRenderer = null;
		private RaycastForNormal _raycaster = null;

		private void Awake()
		{
			_indicatorRenderer = GetComponent<SpriteRenderer>();
			_raycaster = GetComponent<RaycastForNormal>();
			_raycaster.SetTimer(Game.Timer.IndicatorUpdate);
			SetSelected(false);
		}

		private void OnEnable()
		{
			_raycaster.CastPerformed += OnGroundRaycast;
		}

		private void OnDisable()
		{
			_raycaster.CastPerformed -= OnGroundRaycast;
		}

		private void OnGroundRaycast(Vector3 normal)
		{
			transform.forward = normal;
		}

		public void SetColor(Color color)
		{
			_indicatorRenderer.color = color;
		}

		public void SetSelected(bool selected)
		{
			_indicatorRenderer.sprite = selected ? selectedSprite : unselectedSprite;
		}
	}
}