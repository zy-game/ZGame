using System;
using UnityEngine;

namespace CarLogic
{
	public class ResourceOffer : Singleton<ResourceOffer>, IInit
	{
		private Action<string, ResCallback> resLoader;

		public bool LoaderAvailable => resLoader != null;

		public void Init()
		{
		}

		public void SetLoader(Action<string, ResCallback> loader)
		{
			resLoader = loader;
		}

		internal void Load(string path, ResCallback callback)
		{
			if (resLoader != null)
			{
				resLoader(path, callback);
				return;
			}
			UnityEngine.Object @object = Resources.Load(path);
			if (@object != null)
			{
				callback?.Invoke(@object);
			}
		}
	}
}
