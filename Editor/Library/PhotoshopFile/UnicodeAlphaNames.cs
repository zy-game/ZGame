using System.Collections.Generic;

namespace PhotoshopFile
{
	public class UnicodeAlphaNames : ImageResource
	{
		private List<string> channelNames = new List<string>();

		public override ResourceID ID => ResourceID.UnicodeAlphaNames;

		public List<string> ChannelNames => channelNames;

		public UnicodeAlphaNames()
			: base(string.Empty)
		{
		}

		public UnicodeAlphaNames(PsdBinaryReader reader, string name, int resourceDataLength)
			: base(name)
		{
			long num = reader.BaseStream.Position + resourceDataLength;
			while (reader.BaseStream.Position < num)
			{
				string item = reader.ReadUnicodeString();
				ChannelNames.Add(item);
			}
		}

		protected override void WriteData(PsdBinaryWriter writer)
		{
			foreach (string channelName in ChannelNames)
			{
				writer.WriteUnicodeString(channelName);
			}
		}
	}
}
