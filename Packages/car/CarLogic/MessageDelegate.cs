using System;
using UnityEngine;

namespace CarLogic
{
	public class MessageDelegate : MonoBehaviour
	{
		public Action AcOnFinish;

		public void OnFinish()
		{
			if (AcOnFinish != null)
			{
				AcOnFinish();
			}
		}
	}
}
