using System;
using UnityEngine;

namespace CarLogic
{
	[Serializable]
	public class PathDependence
	{
		public string ResPath = "";

		public UnityEngine.Object ResObject;

		public bool AutoLoad;
	}
}
