using System.IO;

public class GasToggle
{
	public float time;

	public byte percent;

	public void Read(BinaryReader reader)
	{
		time = reader.ReadSingle();
		percent = reader.ReadByte();
	}

	public void Write(BinaryWriter writer)
	{
		writer.Write(time);
		writer.Write(percent);
	}
}
