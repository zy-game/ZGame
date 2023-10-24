#define DEBUG
using System.Diagnostics;
using System.Globalization;

namespace PhotoshopFile
{
	public class BlendingRanges
	{
		public Layer Layer { get; private set; }

		public byte[] Data { get; set; }

		public BlendingRanges(Layer layer)
		{
			Layer = layer;
			Data = new byte[0];
		}

		public BlendingRanges(PsdBinaryReader reader, Layer layer)
		{
			Debug.WriteLine("BlendingRanges started at " + reader.BaseStream.Position.ToString(CultureInfo.InvariantCulture));
			Layer = layer;
			int num = reader.ReadInt32();
			if (num > 0)
			{
				Data = reader.ReadBytes(num);
			}
		}

		public void Save(PsdBinaryWriter writer)
		{
			Debug.WriteLine("BlendingRanges Save started at " + writer.BaseStream.Position.ToString(CultureInfo.InvariantCulture));
			if (Data == null)
			{
				writer.Write(0u);
				return;
			}
			writer.Write((uint)Data.Length);
			writer.Write(Data);
		}
	}
}
