using System.Collections.Generic;
using UnityEngine;

namespace RedDust.UI.Cursor
{
	[System.Serializable]
	public class ActionIcons
	{
		[System.Serializable]
		public class Icon
		{
			public string actionName;
			public Texture2D icon;
		}

		public List<Icon> mappings;
	}
}