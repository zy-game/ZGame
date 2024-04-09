using System;
using System.Text;
using UnityEngine;

namespace CarLogic
{
	[Serializable]
	public class ControllerBase
	{
		[SerializeField]
		private Bool active = false;

		public bool ShowDebug = true;

		private static StringBuilder builder = new StringBuilder(512);

		public bool Active
		{
			get
			{
				return active;
			}
			set
			{
				if (!(active == value))
				{
					OnActiveChange(value);
					active = value;
				}
			}
		}

		public void Log(params object[] info)
		{
			if (ShowDebug)
			{
				buildInfo(info);
				Debug.Log(builder.ToString());
			}
		}

		public void LogWarning(params object[] info)
		{
			if (ShowDebug)
			{
				buildInfo(info);
				Debug.LogWarning(builder.ToString());
			}
		}

		public void LogError(params object[] info)
		{
			if (ShowDebug)
			{
				buildInfo(info);
				Debug.LogError(builder.ToString());
			}
		}

		public virtual void OnActiveChange(bool active)
		{
		}

		private void buildInfo(object[] obj)
		{
			builder.Remove(0, builder.Length);
			if (obj == null)
			{
				builder.Append("Null at ").Append(DateTime.Now);
				return;
			}
			for (int i = 0; i < obj.Length; i++)
			{
				builder.Append(obj[i]);
			}
			builder.Append(" at ").Append(DateTime.Now);
		}
	}
}
