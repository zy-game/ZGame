using System;
using System.IO;
using UnityEngine;

namespace CarLogic
{
	public abstract class RacePack
	{
		public const ushort RACE_PACK_HEAD_SIZE = 7;

		public const int RACE_PACK_LENGTH_OFFSET = 2;

		public const int RACE_CHECK_SUM_OFFSET = 8;

		private byte mainType;

		private byte subType;

		private byte operate;

		private ushort length;

		private ushort chkSum;

		public byte MainType => mainType;

		public byte SubType => subType;

		public byte Operation => operate;

		public ushort Length => length;

		protected RacePack(byte MainType, byte subType, byte op)
		{
			mainType = MainType;
			this.subType = subType;
			operate = op;
		}

		protected RacePack(byte[] data)
		{
			BinaryReader reader = new BinaryReader(new MemoryStream(data));
			try
			{
				readHead(reader);
				readData(reader);
			}
			catch (Exception ex)
			{
				Debug.LogWarning($"RacePack Parse Error :  Protocol {(RaceProtocol)subType}\n{ex.ToString()}\n{BitConverter.ToString(data)}");
			}
		}

		protected virtual void writeHead(BinaryWriter writer)
		{
			writer.BaseStream.Seek(0L, SeekOrigin.Begin);
			writer.Write('C');
			writer.Write('A');
			writer.Write('R');
			writer.Write(mainType);
			writer.Write(subType);
			writer.Write(length);
			writer.Write(operate);
			writer.Write((ushort)0);
		}

		protected void writeChkSum(byte[] data)
		{
			ushort num = CalCKS(data);
			data[8] = (byte)num;
			data[9] = (byte)(num >> 8);
		}

		protected virtual void readHead(BinaryReader reader)
		{
			reader.BaseStream.Seek(0L, SeekOrigin.Begin);
			reader.ReadBytes(3);
			mainType = reader.ReadByte();
			subType = reader.ReadByte();
			length = reader.ReadUInt16();
			operate = reader.ReadByte();
			chkSum = reader.ReadUInt16();
		}

		public byte[] ToArray()
		{
			MemoryStream memoryStream = new MemoryStream(256);
			BinaryWriter writer = new BinaryWriter(memoryStream);
			writeHead(writer);
			writeData(writer);
			byte[] array = memoryStream.ToArray();
			writeLength(array);
			writeChkSum(array);
			return array;
		}

		private void writeLength(byte[] data)
		{
			if (data.Length >= 7)
			{
				length = (ushort)data.Length;
				data[5] = (byte)length;
				data[6] = (byte)(length >> 8);
			}
		}

		protected abstract void writeData(BinaryWriter writer);

		protected abstract void readData(BinaryReader reader);

		public static ushort CalCKS(byte[] ds)
		{
			uint num = 0u;
			int num2 = ds.Length;
			int num3 = 0;
			while (num2 > 1)
			{
				ushort num4 = (ushort)(ds[num3] | (ds[num3 + 1] << 8));
				num += num4;
				num2 -= 2;
				num3 += 2;
			}
			if (num2 != 0)
			{
				num += ds[ds.Length - 1];
			}
			while (num >> 16 != 0)
			{
				num = (num >> 16) + (num & 0xFFFF);
			}
			return (ushort)(~num);
		}

		public static byte GetSubType(byte[] data)
		{
			if (data != null && data.Length > 4)
			{
				return data[4];
			}
			return 0;
		}
	}
}
