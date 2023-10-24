#define DEBUG
using System;
using System.Diagnostics;
using System.IO;

namespace PhotoshopFile
{
	public class RleWriter
	{
		private int maxPacketLength = 128;

		private object rleLock;

		private Stream stream;

		private byte[] data;

		private int offset;

		private bool isRepeatPacket;

		private int idxPacketStart;

		private int packetLength;

		private byte runValue;

		private int runLength;

		public RleWriter(Stream stream)
		{
			rleLock = new object();
			this.stream = stream;
		}

		public unsafe int Write(byte[] data, int offset, int count)
		{
			if (!Util.CheckBufferBounds(data, offset, count))
			{
				throw new ArgumentOutOfRangeException();
			}
			if (count == 0)
			{
				throw new ArgumentOutOfRangeException("count");
			}
			lock (rleLock)
			{
				long position = stream.Position;
				this.data = data;
				this.offset = offset;
				fixed (byte* ptr = &data[0])
				{
					byte* ptr2 = ptr + offset;
					byte* ptrEnd = ptr2 + count;
					int num = EncodeToStream(ptr2, ptrEnd);
					Debug.Assert(num == count, "Encoded byte count should match the argument.");
				}
				return (int)(stream.Position - position);
			}
		}

		private void ClearPacket()
		{
			isRepeatPacket = false;
			packetLength = 0;
		}

		private void WriteRepeatPacket(int length)
		{
			byte value = (byte)(1 - length);
			stream.WriteByte(value);
			stream.WriteByte(runValue);
		}

		private void WriteLiteralPacket(int length)
		{
			byte value = (byte)(length - 1);
			stream.WriteByte(value);
			stream.Write(data, idxPacketStart, length);
		}

		private void WritePacket()
		{
			if (isRepeatPacket)
			{
				WriteRepeatPacket(packetLength);
			}
			else
			{
				WriteLiteralPacket(packetLength);
			}
		}

		private void StartPacket(int count, bool isRepeatPacket, int runLength, byte value)
		{
			this.isRepeatPacket = isRepeatPacket;
			packetLength = runLength;
			this.runLength = runLength;
			runValue = value;
			idxPacketStart = offset + count;
		}

		private void ExtendPacketAndRun(byte value)
		{
			packetLength++;
			runLength++;
		}

		private void ExtendPacketStartNewRun(byte value)
		{
			packetLength++;
			runLength = 1;
			runValue = value;
		}

		private unsafe int EncodeToStream(byte* ptr, byte* ptrEnd)
		{
			StartPacket(0, false, 1, *ptr);
			int num = 1;
			ptr++;
			while (ptr < ptrEnd)
			{
				byte b = *ptr;
				if (packetLength == 1)
				{
					isRepeatPacket = b == runValue;
					if (isRepeatPacket)
					{
						ExtendPacketAndRun(b);
					}
					else
					{
						ExtendPacketStartNewRun(b);
					}
				}
				else if (packetLength == maxPacketLength)
				{
					WritePacket();
					StartPacket(num, false, 1, b);
				}
				else if (isRepeatPacket)
				{
					if (b == runValue)
					{
						ExtendPacketAndRun(b);
					}
					else
					{
						WriteRepeatPacket(packetLength);
						StartPacket(num, false, 1, b);
					}
				}
				else if (b == runValue)
				{
					ExtendPacketAndRun(b);
					if (runLength == 3)
					{
						Debug.Assert(packetLength > 3);
						WriteLiteralPacket(packetLength - 3);
						StartPacket(num - 2, true, 3, b);
					}
				}
				else
				{
					ExtendPacketStartNewRun(b);
				}
				ptr++;
				num++;
			}
			WritePacket();
			ClearPacket();
			return num;
		}
	}
}
