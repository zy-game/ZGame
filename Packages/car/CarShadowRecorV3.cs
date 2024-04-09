using System.Collections.Generic;
using System.IO;
using CarLogic;

public class CarShadowRecorV3 : CarShadowRecor
{
	public Dictionary<int, LinkedList<byte[]>> MoveDataProcWrite = new Dictionary<int, LinkedList<byte[]>>();

	public LinkedList<MovementData> MoveDataProcRead = new LinkedList<MovementData>();

	public Queue<QteData> QteDatas = new Queue<QteData>();

	public override byte Version()
	{
		return 5;
	}

	protected void SaveProc(BinaryWriter writer)
	{
		writer.Write(MoveDataProcWrite.Count);
		foreach (int key in MoveDataProcWrite.Keys)
		{
			LinkedList<byte[]> linkedList = MoveDataProcWrite[key];
			writer.Write(linkedList.Count);
			writer.Write(key);
			while (linkedList.Count > 0)
			{
				byte[] value = linkedList.First.Value;
				writer.Write(value);
				linkedList.RemoveFirst();
			}
		}
		MoveDataProcWrite.Clear();
	}

	protected void ReadProc(BinaryReader reader)
	{
		MoveDataProcRead.Clear();
		int num = reader.ReadInt32();
		for (int i = 0; i < num; i++)
		{
			int num2 = reader.ReadInt32();
			int count = reader.ReadInt32();
			for (int j = 0; j < num2; j++)
			{
				byte[] data = reader.ReadBytes(count);
				MovementData value = new MovementData(data);
				MoveDataProcRead.AddLast(value);
			}
		}
	}

	protected void ReadQte(BinaryReader reader)
	{
		QteDatas.Clear();
		int num = reader.ReadInt32();
		for (int i = 0; i < num; i++)
		{
			QteData qteData = new QteData();
			qteData.Read(reader);
			QteDatas.Enqueue(qteData);
		}
	}

	public void SaveQte(BinaryWriter writer)
	{
		writer.Write(QteDatas.Count);
		while (QteDatas.Count > 0)
		{
			QteData qteData = QteDatas.Dequeue();
			qteData.Write(writer);
		}
	}

	public override byte[] ToBytes()
	{
		MemoryStream memoryStream = new MemoryStream();
		BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
		binaryWriter.Write((ushort)0);
		saveHead(binaryWriter);
		SaveProc(binaryWriter);
		SaveQte(binaryWriter);
		byte[] result = memoryStream.ToArray();
		memoryStream.Close();
		return result;
	}

	public override void Load(byte[] data)
	{
		MemoryStream input = new MemoryStream(data);
		BinaryReader binaryReader = new BinaryReader(input);
		ushort num = binaryReader.ReadUInt16();
		binaryReader.BaseStream.Seek(num, SeekOrigin.Begin);
		ReadProc(binaryReader);
		ReadQte(binaryReader);
		binaryReader.BaseStream.Seek(2L, SeekOrigin.Begin);
		readHead(binaryReader, num);
		binaryReader.Close();
	}

	public override string ToString()
	{
		return $"CarShadowRecor: MapId={base.MapId}, TrackId={base.TrackId}, ItemStrategy={base.ItemStrategy}, TotalTime={base.TotalTime}, PlayerId = {base.PlayerId}, PlayerName={base.PlayerName}, CarId={base.CarId}, RaceTime={base.RaceTime}";
	}

	public new void OnDestroy()
	{
		MoveDataProcWrite.Clear();
		MoveDataProcRead.Clear();
		QteDatas.Clear();
	}
}
