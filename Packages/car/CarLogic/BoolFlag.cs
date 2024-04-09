namespace CarLogic
{
	public class BoolFlag
	{
		private int flag;

		public void Reset()
		{
			flag = 0;
		}

		public static implicit operator bool(BoolFlag o)
		{
			if (o != null)
			{
				return o.flag != 0;
			}
			return false;
		}

		public static BoolFlag operator ++(BoolFlag o)
		{
			o.flag++;
			return o;
		}

		public static BoolFlag operator --(BoolFlag o)
		{
			if (o != null && o.flag > 0)
			{
				o.flag--;
			}
			return o;
		}

		public static bool operator ==(BoolFlag o, bool b)
		{
			if (!b || o == null)
			{
				return o.flag == 0;
			}
			return o.flag != 0;
		}

		public static bool operator !=(BoolFlag o, bool b)
		{
			if (!b || o == null)
			{
				return o.flag != 0;
			}
			return o.flag == 0;
		}
	}
}
