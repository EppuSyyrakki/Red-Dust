using UnityEngine;
using Utils;

namespace RedDust.Combat
{
	public class HostilityIndicator : MonoBehaviour
	{
		[SerializeField]
		private Sprite selectedSprite;

		[SerializeField]
		private Sprite unselectedSprite;

		private SpriteRenderer indicatorRenderer = null;
		private RaycastForNormal raycaster = null;

		private void Awake()
		{
			indicatorRenderer = GetComponent<SpriteRenderer>();
			raycaster = GetComponent<RaycastForNormal>();
			raycaster.SetTimer(Values.Timer.IndicatorUpdate);
			SetSelected(false);
		}

		private void OnEnable()
		{
			raycaster.CastPerformed += OnGroundRaycast;
		}

		private void OnDisable()
		{
			raycaster.CastPerformed -= OnGroundRaycast;
		}

		private void OnGroundRaycast(Vector3 normal)
		{
			transform.forward = normal;
		}

		public void SetColor(Color color)
		{
			indicatorRenderer.color = color;
		}

		public void SetSelected(bool selected)
		{
			indicatorRenderer.sprite = selected ? selectedSprite : unselectedSprite;
		}
	}
}