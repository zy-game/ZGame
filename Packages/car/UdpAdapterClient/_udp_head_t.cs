using System.IO;

namespace UdpAdapterClient
{
	public class _udp_head_t
	{
		public byte m_check_sum;

		public byte m_flags;

		public ushort m_seq;

		public long m_time_stamp;

		public ushort m_net_delay;

		public byte m_data_count;

		public uint m_data_size;

		public void to_bytes(BinaryWriter __writer)
		{
			__writer.Write(m_check_sum);
			__writer.Write(m_flags);
			__writer.Write(m_seq);
			__writer.Write(m_time_stamp);
			__writer.Write(m_net_delay);
			__writer.Write(m_data_count);
			__writer.Write(m_data_size);
		}

		public void from_bytes(BinaryReader __reader)
		{
			m_check_sum = __reader.ReadByte();
			m_flags = __reader.ReadByte();
			m_seq = __reader.ReadUInt16();
			m_time_stamp = __reader.ReadInt64();
			m_net_delay = __reader.ReadUInt16();
			m_data_count = __reader.ReadByte();
			m_data_size = __reader.ReadUInt32();
		}
	}
}
