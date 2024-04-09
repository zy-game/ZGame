using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class CarRecor
{
	public float RealSpeed;

	public int N2StateLevel = -1;

	public List<CarPoint> trail = new List<CarPoint>(512);

	public List<ItemToggle> itemToggles = new List<ItemToggle>(4);

	public List<QTEToggle> qteToggles = new List<QTEToggle>(6);

	public List<GasToggle> gasToggles = new List<GasToggle>();

	public int Length;

	protected float totalTime;

	public static byte currentVersion = 3;

	protected uint seqId;

	public int TotalLap;

	public string FilePath { get; set; }

	public float TotalTime => totalTime;

	public ushort MapId { get; set; }

	public byte TrackId { get; set; }

	public byte AiId { get; set; }

	public int PlayerId { get; set; }

	public uint ItemStrategy => seqId;

	public virtual byte Version()
	{
		return 6;
	}

	public void AddPositionPoint(GameObject go, float costTime, int flag = 0, float rotateZ = 0f)
	{
		CarPoint carPoint = new CarPoint();
		carPoint.pos.Fill(go.transform.localPosition);
		carPoint.rot.Fill(go.transform.localRotation);
		carPoint.DeltaTime = costTime;
		carPoint.Flag = (byte)flag;
		totalTime += costTime;
		carPoint.rotateZ = rotateZ;
		trail.Add(carPoint);
	}

	public void AddItemPoint(float time, byte index, ushort itemId)
	{
		ItemToggle itemToggle = new ItemToggle();
		itemToggle.time = time;
		itemToggle.itemId = itemId;
		itemToggles.Add(itemToggle);
	}

	public void AddQtePoint(float time, byte flag)
	{
		QTEToggle qTEToggle = new QTEToggle();
		qTEToggle.time = time;
		qTEToggle.flag = flag;
		qteToggles.Add(qTEToggle);
	}

	public void AddGasPoint(float time, float percent)
	{
		percent = Math.Max(0f, percent);
		percent = Math.Min(1f, percent);
		GasToggle gasToggle = new GasToggle();
		gasToggle.time = time;
		int val = (int)(percent * 100f) + 1;
		val = Math.Min(val, 100);
		gasToggle.percent = (byte)val;
		gasToggles.Add(gasToggle);
	}

	protected virtual void saveHead(BinaryWriter writer)
	{
		writer.Write(Version());
		writer.Write(MapId);
		writer.Write(TrackId);
		writer.Write(seqId);
		writer.Write(totalTime);
		ushort num = (ushort)writer.BaseStream.Length;
		writer.Seek(0, SeekOrigin.Begin);
		writer.Write(num);
		writer.Seek(num, SeekOrigin.Begin);
	}

	protected virtual void savePoints(BinaryWriter writer)
	{
		Length = trail.Count;
		writer.Write(Length);
		for (int i = 0; i < trail.Count; i++)
		{
			CarPoint carPoint = trail[i];
			carPoint.Write(writer, Version());
		}
	}

	protected virtual void saveToggles(BinaryWriter writer)
	{
		writer.Write((ushort)itemToggles.Count);
		for (int i = 0; i < itemToggles.Count; i++)
		{
			ItemToggle itemToggle = itemToggles[i];
			itemToggle.Write(writer);
		}
	}

	protected virtual void saveQTE(BinaryWriter writer)
	{
	}

	protected virtual void saveGas(BinaryWriter writer)
	{
	}

	public virtual byte[] Save()
	{
		byte[] array = ToBytes();
		File.WriteAllBytes(FilePath, array);
		return array;
	}

	protected virtual void readHead(BinaryReader reader, long len)
	{
		reader.ReadByte();
		MapId = reader.ReadUInt16();
		TrackId = reader.ReadByte();
		seqId = reader.ReadUInt32();
		totalTime = reader.ReadSingle();
	}

	protected virtual void readPoints(BinaryReader reader)
	{
		Length = reader.ReadInt32();
		trail.Capacity = Length;
		for (int i = 0; i < Length; i++)
		{
			CarPoint carPoint = new CarPoint();
			carPoint.Read(reader, currentVersion);
			trail.Add(carPoint);
		}
	}

	protected void linkPoints()
	{
		for (int i = 1; i < trail.Count; i++)
		{
			trail[i - 1].Next = trail[i];
		}
		CarPoint carPoint = trail[0];
		CarPoint carPoint2 = trail[trail.Count - 1];
		if (Vector3.Distance(carPoint.pos.V3, carPoint2.pos.V3) < 10f)
		{
			carPoint2.Next = carPoint;
		}
	}

	protected virtual void readToggles(BinaryReader reader)
	{
		int num = reader.ReadUInt16();
		itemToggles.Clear();
		itemToggles.Capacity = num;
		for (int i = 0; i < num; i++)
		{
			ItemToggle itemToggle = new ItemToggle();
			itemToggle.Read(reader);
			itemToggles.Add(itemToggle);
		}
	}

	protected virtual void readQTE(BinaryReader reader)
	{
	}

	protected virtual void readGas(BinaryReader reader)
	{
	}

	public virtual void Load(byte[] pBytes)
	{
		if (pBytes == null || pBytes.Length == 0)
		{
			Debug.LogError("No AI info found.");
			return;
		}
		trail.Clear();
		MemoryStream input = new MemoryStream(pBytes);
		BinaryReader binaryReader = new BinaryReader(input);
		ushort num = binaryReader.ReadUInt16();
		binaryReader.BaseStream.Seek(num, SeekOrigin.Begin);
		readPoints(binaryReader);
		linkPoints();
		readToggles(binaryReader);
		readQTE(binaryReader);
		readGas(binaryReader);
		binaryReader.BaseStream.Seek(2L, SeekOrigin.Begin);
		readHead(binaryReader, num);
		binaryReader.Close();
		sortItemToggle();
		fillPastTime();
	}

	protected void sortItemToggle()
	{
		itemToggles.Sort((ItemToggle x, ItemToggle y) => (int)(x.time - y.time));
		for (int i = 0; i < itemToggles.Count; i++)
		{
			itemToggles[i].itemId = (ushort)i;
		}
	}

	protected void fillPastTime()
	{
		float num = 0f;
		for (int i = 0; i < trail.Count; i++)
		{
			CarPoint carPoint = trail[i];
			carPoint.index = i;
			num = (carPoint.TotalPastTime = num + carPoint.DeltaTime);
		}
	}

	protected void fillN2State()
	{
		for (int i = 0; i < trail.Count; i++)
		{
			trail[i].Flag |= (byte)(1 << N2StateLevel);
		}
	}

	public override string ToString()
	{
		return $"[CarRecor: TotalTime={TotalTime}, Vertion={Version()}, MapId={MapId}, Track={TrackId}, Length={Length}, ItemNum={itemToggles.Count}, AiId:{seqId}]";
	}

	public static byte ParseVersion(byte[] data)
	{
		if (data.Length < 3)
		{
			currentVersion = 1;
			return 1;
		}
		currentVersion = data[2];
		return data[2];
	}

	public virtual byte[] ToBytes()
	{
		MemoryStream memoryStream = new MemoryStream();
		BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
		binaryWriter.Write((ushort)0);
		saveHead(binaryWriter);
		savePoints(binaryWriter);
		saveToggles(binaryWriter);
		saveQTE(binaryWriter);
		saveGas(binaryWriter);
		byte[] result = memoryStream.ToArray();
		memoryStream.Close();
		return result;
	}

	public void OnDestroy()
	{
		trail.Clear();
		itemToggles.Clear();
		qteToggles.Clear();
	}

	public void ForceSetSpeed(float realSpeed)
	{
		RealSpeed = realSpeed;
		CarPoint carPoint = null;
		foreach (CarPoint item in trail)
		{
			if (carPoint == null)
			{
				carPoint = item;
				continue;
			}
			float num = Vector3.Distance(item.pos.V3, carPoint.pos.V3);
			item.DeltaTime = num / realSpeed;
			if (item.DeltaTime <= 0f)
			{
				item.DeltaTime = 0.01f;
			}
			carPoint = item;
		}
		fillPastTime();
	}

	public void ForceSetN2StateLevel(int level)
	{
		N2StateLevel = level;
		if (N2StateLevel >= 0)
		{
			fillN2State();
		}
	}

	public void MarkLapNum(int totalLap)
	{
		TotalLap = totalLap;
		if (trail[0].Lap > 0)
		{
			return;
		}
		if (totalLap == 0)
		{
			Debug.LogError("圈数为0这就没法生出来了!~");
		}
		if (totalLap == 1)
		{
			foreach (CarPoint item2 in trail)
			{
				item2.Lap = 1;
			}
			return;
		}
		List<int> list = new List<int>();
		for (int i = 1; i <= totalLap; i++)
		{
			int item = FindNearIndex(totalLap, i);
			list.Add(item);
		}
		for (int num = trail.Count - 1; num >= 0; num--)
		{
			CarPoint carPoint = trail[num];
			for (int num2 = list.Count - 1; num2 >= 0; num2--)
			{
				int lap = num2 + 1;
				int num3 = list[num2];
				if (carPoint.index > num3)
				{
					break;
				}
				carPoint.Lap = lap;
			}
		}
	}

	private int FindNearIndex(int totalLap, int lapNum)
	{
		if (lapNum == totalLap)
		{
			return trail.Count - 1;
		}
		Vector3 v = trail[0].pos.V3;
		int num = trail.Count / totalLap * lapNum;
		Debug.Log($"标杆Index: {num}");
		float num2 = Vector3.Distance(v, trail[num - 1].pos.V3);
		float num3 = Vector3.Distance(v, trail[num + 1].pos.V3);
		if (num2 < num3)
		{
			float num4 = Vector3.Distance(v, trail[num].pos.V3);
			for (int num5 = num - 1; num5 > 0; num5--)
			{
				float num6 = Vector3.Distance(v, trail[num5].pos.V3);
				if (num6 > num4)
				{
					return num5 + 1;
				}
				num4 = num6;
			}
		}
		else
		{
			float num7 = Vector3.Distance(v, trail[num].pos.V3);
			for (int i = num + 1; i < trail.Count; i++)
			{
				float num8 = Vector3.Distance(v, trail[i].pos.V3);
				if (num8 > num7)
				{
					return i - 1;
				}
				num7 = num8;
			}
		}
		return num;
	}
}
