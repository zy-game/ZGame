using System.IO;
using UnityEngine;

public class ItemToggle
{
	public float time;

	public ushort itemId;

	public byte index;

	public Vector3 pos;

	public void Read(BinaryReader reader)
	{
		time = reader.ReadSingle();
		itemId = reader.ReadUInt16();
	}

	public void Write(BinaryWriter writer)
	{
		writer.Write(time);
		writer.Write(itemId);
	}
}
