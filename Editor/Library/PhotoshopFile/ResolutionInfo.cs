namespace PhotoshopFile
{
	public class ResolutionInfo : ImageResource
	{
		public enum ResUnit
		{
			PxPerInch = 1,
			PxPerCm
		}

		public enum Unit
		{
			Inches = 1,
			Centimeters,
			Points,
			Picas,
			Columns
		}

		public override ResourceID ID => ResourceID.ResolutionInfo;

		public UFixed16_16 HDpi { get; set; }

		public UFixed16_16 VDpi { get; set; }

		public ResUnit HResDisplayUnit { get; set; }

		public ResUnit VResDisplayUnit { get; set; }

		public Unit WidthDisplayUnit { get; set; }

		public Unit HeightDisplayUnit { get; set; }

		public ResolutionInfo()
			: base(string.Empty)
		{
		}

		public ResolutionInfo(PsdBinaryReader reader, string name)
			: base(name)
		{
			HDpi = new UFixed16_16(reader.ReadUInt32());
			HResDisplayUnit = (ResUnit)reader.ReadInt16();
			WidthDisplayUnit = (Unit)reader.ReadInt16();
			VDpi = new UFixed16_16(reader.ReadUInt32());
			VResDisplayUnit = (ResUnit)reader.ReadInt16();
			HeightDisplayUnit = (Unit)reader.ReadInt16();
		}

		protected override void WriteData(PsdBinaryWriter writer)
		{
			writer.Write(HDpi.Integer);
			writer.Write(HDpi.Fraction);
			writer.Write((short)HResDisplayUnit);
			writer.Write((short)WidthDisplayUnit);
			writer.Write(VDpi.Integer);
			writer.Write(VDpi.Fraction);
			writer.Write((short)VResDisplayUnit);
			writer.Write((short)HeightDisplayUnit);
		}
	}
}
