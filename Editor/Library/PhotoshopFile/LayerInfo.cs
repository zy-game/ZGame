#define DEBUG
using System.Diagnostics;

namespace PhotoshopFile
{
	public abstract class LayerInfo
	{
		public abstract string Key { get; }

		protected abstract void WriteData(PsdBinaryWriter writer);

		public void Save(PsdBinaryWriter writer)
		{
			Debug.WriteLine("LayerInfo.Save started at " + writer.BaseStream.Position);
			writer.WriteAsciiChars("8BIM");
			writer.WriteAsciiChars(Key);
			long position = writer.BaseStream.Position;
			using (new PsdBlockLengthWriter(writer))
			{
				WriteData(writer);
			}
			writer.WritePadding(position, 4);
		}
	}
}
