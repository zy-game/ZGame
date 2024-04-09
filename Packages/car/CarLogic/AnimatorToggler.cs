using UnityEngine;

namespace CarLogic
{
	public class AnimatorToggler : MonoBehaviour
	{
		public Animator Target;

		private bool enableOnDisappear;

		public void Start()
		{
			if (Target != null)
			{
				enableOnDisappear = Target.enabled;
			}
		}

		public void OnBecameVisible()
		{
			if (Target != null)
			{
				Target.enabled = enableOnDisappear;
			}
		}

		public void OnBecameInvisible()
		{
			if (Target != null)
			{
				enableOnDisappear = Target.enabled;
				Target.enabled = false;
			}
		}
	}
}
