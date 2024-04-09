using System.IO;

namespace UdpAdapterClient
{
	public class _udp_data_t
	{
		public ushort m_len;

		public byte[] m_buf;

		public const int size = 2;

		public const int head_size = 2;

		public _udp_data_t()
		{
		}

		public _udp_data_t(byte[] __buf)
		{
			m_len = (ushort)__buf.Length;
			m_buf = __buf;
		}

		public void to_bytes(BinaryWriter __writer)
		{
			__writer.Write(m_len);
			__writer.Write(m_buf);
		}

		public void from_bytes(BinaryReader __reader)
		{
			m_len = __reader.ReadUInt16();
			m_buf = __reader.ReadBytes(m_len);
		}
	}
}
