using System;
using System.Globalization;

namespace PhotoshopFile
{
	public abstract class ImageResource
	{
		private string signature;

		public string Signature
		{
			get
			{
				return signature;
			}
			set
			{
				if (value.Length != 4)
				{
					throw new ArgumentException("Signature must have length of 4");
				}
				signature = value;
			}
		}

		public string Name { get; set; }

		public abstract ResourceID ID { get; }

		protected ImageResource(string name)
		{
			Signature = "8BIM";
			Name = name;
		}

		public void Save(PsdBinaryWriter writer)
		{
			writer.WriteAsciiChars(Signature);
			writer.Write((ushort)ID);
			writer.WritePascalString(Name, 2);
			long position = writer.BaseStream.Position;
			using (new PsdBlockLengthWriter(writer))
			{
				WriteData(writer);
			}
			writer.WritePadding(position, 2);
		}

		protected abstract void WriteData(PsdBinaryWriter writer);

		public override string ToString()
		{
			return string.Format(CultureInfo.InvariantCulture, "{0} {1}", ID, Name);
		}
	}
}
