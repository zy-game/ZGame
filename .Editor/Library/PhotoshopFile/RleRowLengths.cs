using System.Linq;

namespace PhotoshopFile
{
	public class RleRowLengths
	{
		public int[] Values { get; private set; }

		public int Total => Values.Sum();

		public int this[int i]
		{
			get
			{
				return Values[i];
			}
			set
			{
				Values[i] = value;
			}
		}

		public RleRowLengths(int rowCount)
		{
			Values = new int[rowCount];
		}

		public RleRowLengths(PsdBinaryReader reader, int rowCount)
			: this(rowCount)
		{
			for (int i = 0; i < rowCount; i++)
			{
				Values[i] = reader.ReadUInt16();
			}
		}

		public void Write(PsdBinaryWriter writer)
		{
			for (int i = 0; i < Values.Length; i++)
			{
				writer.Write((ushort)Values[i]);
			}
		}
	}
}
