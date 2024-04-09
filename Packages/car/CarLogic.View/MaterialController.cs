using UnityEngine;

namespace CarLogic.View
{
	[AddComponentMenu("Effects/MaterialController")]
	public class MaterialController : MonoBehaviour
	{
		public string ColorName = "_Color";

		public Color ColorValue = Color.white;

		public Material Mat;

		public bool Cached = true;

		private Color cacheColor = Color.white;

		public void Start()
		{
			if (Mat == null)
			{
				Renderer component = GetComponent<Renderer>();
				if ((bool)component)
				{
					Mat = component.material;
				}
			}
		}

		public void Update()
		{
			updateValues();
		}

		private void updateValues()
		{
			if (Mat != null && (cacheColor != ColorValue || !Cached))
			{
				Mat.SetColor(ColorName, ColorValue);
				cacheColor = ColorValue;
			}
		}
	}
}
