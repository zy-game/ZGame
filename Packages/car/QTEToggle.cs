using System.IO;

public class QTEToggle
{
	public float time;

	public byte flag;

	public void Read(BinaryReader reader)
	{
		time = reader.ReadSingle();
		flag = reader.ReadByte();
	}

	public void Write(BinaryWriter writer)
	{
		writer.Write(time);
		writer.Write(flag);
	}
}
