using System;
using System.IO;

namespace CarLogic
{
	public class RacePlayerMsg : RacePack
	{
		public long WorldId;

		public RacePlayerMsg(byte MainType, byte subType, byte op)
			: base(MainType, subType, op)
		{
		}

		public RacePlayerMsg(byte[] data)
			: base(data)
		{
		}

		protected override void writeHead(BinaryWriter writer)
		{
			base.writeHead(writer);
			writer.Write(WorldId);
		}

		protected override void readHead(BinaryReader reader)
		{
			base.readHead(reader);
			WorldId = reader.ReadInt64();
		}

		protected override void writeData(BinaryWriter writer)
		{
			throw new NotImplementedException();
		}

		protected override void readData(BinaryReader reader)
		{
			throw new NotImplementedException();
		}
	}
}
