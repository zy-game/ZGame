using System;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

namespace UdpAdapterClient
{
	public class udp_adapter
	{
		public const int UDP_MSG_UNSAFE = 256;

		public const int UDP_MSG_PUSH = 512;

		public Socket m_sock_fd;

		public IPEndPoint m_sock_addr;

		protected safeNoLockQueue<udp_packet> m_sock_buf_list;

		protected udp_session m_session;

		private bool m_run = true;

		public int m_state = 2;

		private byte[] m_recvBuf = new byte[65535];

		public udp_adapter()
		{
			m_sock_fd = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
			m_sock_fd.Blocking = true;
			m_sock_fd.SendTimeout = 60;
			m_sock_fd.ReceiveTimeout = 60;
			m_sock_buf_list = new safeNoLockQueue<udp_packet>();
			m_session = new udp_session();
		}

		~udp_adapter()
		{
			close();
		}

		public int connect(string __addr, int __port)
		{
			lock (m_session)
			{
				udp_define.assert(!m_session.is_run());
				m_sock_addr = new IPEndPoint(IPAddress.Parse(__addr), __port);
				try
				{
					m_sock_fd.Connect(m_sock_addr);
				}
				catch (Exception ex)
				{
					Debug.Log(ex.ToString());
				}
				m_session.connect(m_sock_fd, m_sock_addr);
				udp_define.printf("[connect] [addr]:{0}\n\n", m_sock_addr.ToString());
			}
			return 0;
		}

		public int close()
		{
			lock (m_session)
			{
				udp_define.assert(m_session.is_run());
				udp_define.printf("[close] [addr]:{0}\n\n", m_sock_addr.ToString());
				m_run = false;
				m_session.close();
				m_sock_fd.Close();
			}
			return 0;
		}

		public int send(byte[] __buf, int __sendType)
		{
			udp_define.assert(m_session.is_run());
			m_session.push_send_buf(__buf, __sendType);
			return __buf.Length;
		}

		public byte[] recv(bool isMutiThreading = false)
		{
			byte[] array = null;
			while (m_run && array == null)
			{
				if (m_session.is_run())
				{
					if (!udp_define.isMultiThreading() && !isMutiThreading)
					{
						loop_recv();
					}
					proc_session();
					m_state = m_session.m_session_state;
					array = m_session.pop_ready_buf();
				}
				else
				{
					m_state = 2;
				}
			}
			return array;
		}

		public void proc()
		{
			while (m_run)
			{
				if (m_session.is_run())
				{
					loop_recv();
				}
				else
				{
					m_state = 2;
				}
			}
		}

		protected void loop_recv()
		{
			try
			{
				int num = m_sock_fd.Receive(m_recvBuf);
				if (checkValid(m_recvBuf, num) && m_session.is_run())
				{
					m_session.push_recv_buf(new udp_packet(m_recvBuf, num));
				}
			}
			catch
			{
			}
		}

		protected void proc_session()
		{
			lock (m_session)
			{
				if (m_session.is_run())
				{
					m_session.loop();
				}
			}
		}

		protected bool checkValid(byte[] __buf, int dataSize)
		{
			if (dataSize <= 0)
			{
				return false;
			}
			if (__buf[0] != udp_packet.checksum(__buf, dataSize))
			{
				return false;
			}
			return true;
		}

		public bool isConnect()
		{
			if (m_session.is_run())
			{
				return true;
			}
			return false;
		}
	}
}
