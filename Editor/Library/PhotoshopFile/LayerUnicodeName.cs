namespace PhotoshopFile
{
	public class LayerUnicodeName : LayerInfo
	{
		public override string Key => "luni";

		public string Name { get; set; }

		public LayerUnicodeName(string name)
		{
			Name = name;
		}

		public LayerUnicodeName(PsdBinaryReader reader)
		{
			Name = reader.ReadUnicodeString();
		}

		protected override void WriteData(PsdBinaryWriter writer)
		{
			long position = writer.BaseStream.Position;
			writer.WriteUnicodeString(Name);
			writer.WritePadding(position, 4);
		}
	}
}
