using System.Collections.Specialized;
using UnityEngine;

namespace PhotoshopFile
{
	public class MaskInfo
	{
		public Mask LayerMask { get; set; }

		public Mask UserMask { get; set; }

		public MaskInfo()
		{
		}

		public MaskInfo(PsdBinaryReader reader, Layer layer)
		{
			uint num = reader.ReadUInt32();
			if (num != 0)
			{
				long position = reader.BaseStream.Position;
				long position2 = position + num;
				Rect rect = reader.ReadRectangle();
				byte color = reader.ReadByte();
				byte data = reader.ReadByte();
				LayerMask = new Mask(layer, rect, color, new BitVector32(data));
				if (num == 36)
				{
					byte data2 = reader.ReadByte();
					byte color2 = reader.ReadByte();
					Rect rect2 = reader.ReadRectangle();
					UserMask = new Mask(layer, rect2, color2, new BitVector32(data2));
				}
				reader.BaseStream.Position = position2;
			}
		}

		public void Save(PsdBinaryWriter writer)
		{
			if (LayerMask == null)
			{
				writer.Write(0u);
				return;
			}
			using (new PsdBlockLengthWriter(writer))
			{
				writer.Write(LayerMask.Rect);
				writer.Write(LayerMask.BackgroundColor);
				writer.Write((byte)LayerMask.Flags.Data);
				if (UserMask == null)
				{
					writer.Write((ushort)0);
					return;
				}
				writer.Write((byte)UserMask.Flags.Data);
				writer.Write(UserMask.BackgroundColor);
				writer.Write(UserMask.Rect);
			}
		}
	}
}
