using UnityEngine;

namespace RedDust.Control
{
	public class HostilityIndicator : MonoBehaviour
	{
		[SerializeField]
		private Sprite selectedSprite;

		[SerializeField]
		private Sprite unselectedSprite;

		private SpriteRenderer _indicatorRenderer = null;

		private void Awake()
		{
			_indicatorRenderer = GetComponent<SpriteRenderer>();
			SetSelected(false);
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