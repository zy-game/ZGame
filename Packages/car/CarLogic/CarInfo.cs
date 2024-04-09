using System;

namespace CarLogic
{
	[Serializable]
	public class CarInfo
	{
		private object info;

		private static Func<CarInfo, uint> fcGetPlayerId;

		private static Func<CarInfo, long> fcGetWorldId;

		public static Func<CarInfo, uint> FcGetPlayerId
		{
			set
			{
				fcGetPlayerId = value;
			}
		}

		public static Func<CarInfo, long> FcGetWorldId
		{
			set
			{
				fcGetWorldId = value;
			}
		}

		public uint PlayerId
		{
			get
			{
				if (fcGetPlayerId != null)
				{
					return fcGetPlayerId(this);
				}
				return 0u;
			}
		}

		public long WorldId
		{
			get
			{
				if (fcGetWorldId != null)
				{
					return fcGetWorldId(this);
				}
				return 0L;
			}
		}

		public void SetInfo(object o)
		{
			info = o;
		}

		public T Info<T>() where T : new()
		{
			if (info is T)
			{
				return (T)info;
			}
			return new T();
		}
	}
}
