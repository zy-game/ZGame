using System;
using System.IO;

[Serializable]
public class CarPoint
{
	[NonSerialized]
	public int index;

	[NonSerialized]
	public int Lap;

	[NonSerialized]
	public CarPoint Next;

	public Vector3Serializer pos;

	public QuaternionSerializer rot;

	private ushort utime;

	public float TotalPastTime;

	public byte Flag;

	public float rotateZ;

	public const int Size = 21;

	public float DeltaTime
	{
		get
		{
			return (float)(int)utime * 0.001f;
		}
		set
		{
			utime = (ushort)(value * 1000f);
		}
	}

	public void Write(BinaryWriter writer, int version)
	{
		writer.Write(pos.z);
		writer.Write((short)(pos.y * 100f));
		writer.Write(pos.x);
		writer.Write(rot.sx);
		writer.Write(rot.sy);
		writer.Write(rot.sz);
		writer.Write(rot.sw);
		writer.Write(utime);
		writer.Write(Flag);
		if (version == 3 || version == 4)
		{
			writer.Write(rotateZ);
		}
	}

	public void Read(BinaryReader reader, byte version)
	{
		pos.z = reader.ReadSingle();
		pos.y = (float)reader.ReadInt16() * 0.01f;
		pos.x = reader.ReadSingle();
		rot.sx = reader.ReadInt16();
		rot.sy = reader.ReadInt16();
		rot.sz = reader.ReadInt16();
		rot.sw = reader.ReadInt16();
		utime = reader.ReadUInt16();
		Flag = reader.ReadByte();
		if (version == 3 || version == 4)
		{
			rotateZ = reader.ReadSingle();
		}
	}
}
