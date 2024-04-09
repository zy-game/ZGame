using System.IO;
using System.Text;

public class CarShadowRecor : CarRecor
{
	private int[] _equipSuitPartIds = new int[5];

	public string PlayerName { get; set; }

	public short[] ClotheIds { get; set; }

	public ushort CarId { get; set; }

	public uint RaceTime { get; set; }

	public ulong RaceDate { get; set; }

	public int PetId { get; set; }

	public int[] EquipSuitPartIds
	{
		get
		{
			return _equipSuitPartIds;
		}
		set
		{
			_equipSuitPartIds = value;
		}
	}

	public override byte Version()
	{
		return 4;
	}

	protected override void saveHead(BinaryWriter writer)
	{
		writer.Write(Version());
		writer.Write(base.MapId);
		writer.Write(base.TrackId);
		writer.Write(seqId);
		writer.Write(totalTime);
		writer.Write(base.PlayerId);
		byte[] bytes = Encoding.UTF8.GetBytes(PlayerName);
		writer.Write((byte)bytes.Length);
		writer.Write(bytes);
		byte b = (byte)ClotheIds.Length;
		writer.Write(b);
		for (short num = 0; num < b; num++)
		{
			writer.Write(ClotheIds[num]);
		}
		writer.Write(CarId);
		writer.Write(RaceTime);
		writer.Write(RaceDate);
		writer.Write(PetId);
		byte b2 = (byte)_equipSuitPartIds.Length;
		writer.Write(b2);
		for (short num2 = 0; num2 < b2; num2++)
		{
			writer.Write(_equipSuitPartIds[num2]);
		}
		ushort num3 = (ushort)writer.BaseStream.Length;
		writer.Seek(0, SeekOrigin.Begin);
		writer.Write(num3);
		writer.Seek(num3, SeekOrigin.Begin);
	}

	protected override void readHead(BinaryReader reader, long len)
	{
		reader.ReadByte();
		base.MapId = reader.ReadUInt16();
		base.TrackId = reader.ReadByte();
		seqId = reader.ReadUInt32();
		totalTime = reader.ReadSingle();
		base.PlayerId = reader.ReadInt32();
		byte count = reader.ReadByte();
		byte[] bytes = reader.ReadBytes(count);
		PlayerName = Encoding.UTF8.GetString(bytes);
		byte b = reader.ReadByte();
		ClotheIds = new short[b];
		for (short num = 0; num < b; num++)
		{
			ClotheIds[num] = reader.ReadInt16();
		}
		CarId = reader.ReadUInt16();
		RaceTime = reader.ReadUInt32();
		RaceDate = reader.ReadUInt64();
		long position = reader.BaseStream.Position;
		if (reader.BaseStream.Position < len)
		{
			PetId = reader.ReadInt32();
		}
		if (reader.BaseStream.Position < len)
		{
			byte b2 = reader.ReadByte();
			if (b2 != _equipSuitPartIds.Length)
			{
				_equipSuitPartIds = new int[b2];
			}
			for (short num2 = 0; num2 < b2; num2++)
			{
				_equipSuitPartIds[num2] = reader.ReadInt32();
			}
		}
	}

	protected override void saveQTE(BinaryWriter writer)
	{
		ushort num = (ushort)qteToggles.Count;
		writer.Write(num);
		for (int i = 0; i < num; i++)
		{
			QTEToggle qTEToggle = qteToggles[i];
			qTEToggle.Write(writer);
		}
	}

	protected override void readQTE(BinaryReader reader)
	{
		ushort num = reader.ReadUInt16();
		qteToggles.Clear();
		qteToggles.Capacity = num;
		for (int i = 0; i < num; i++)
		{
			QTEToggle qTEToggle = new QTEToggle();
			qTEToggle.Read(reader);
			qteToggles.Add(qTEToggle);
		}
	}

	protected override void saveGas(BinaryWriter writer)
	{
		ushort num = (ushort)gasToggles.Count;
		writer.Write(num);
		for (int i = 0; i < num; i++)
		{
			GasToggle gasToggle = gasToggles[i];
			gasToggle.Write(writer);
		}
	}

	protected override void readGas(BinaryReader reader)
	{
		ushort num = reader.ReadUInt16();
		gasToggles.Clear();
		gasToggles.Capacity = num;
		for (int i = 0; i < num; i++)
		{
			GasToggle gasToggle = new GasToggle();
			gasToggle.Read(reader);
			gasToggles.Add(gasToggle);
		}
	}

	public override string ToString()
	{
		return $"CarShadowRecor: MapId={base.MapId}, TrackId={base.TrackId}, ItemStrategy={base.ItemStrategy}, TotalTime={base.TotalTime}, PlayerId = {base.PlayerId}, PlayerName={PlayerName}, CarId={CarId}, RaceTime={RaceTime}";
	}
}
