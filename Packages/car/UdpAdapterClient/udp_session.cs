using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace UdpAdapterClient
{
	public class udp_session
	{
		public static DateTime s_base_time = DateTime.Now;

		public Socket m_sock_fd;

		public IPEndPoint m_sock_addr;

		private ushort m_send_seq_reliable;

		private ushort m_send_seq_unreliable;

		private int m_send_count;

		private int m_send_len;

		private int m_send_response_count;

		private int m_last_send_response;

		private int m_fast_retry;

		private int m_timeout_retry;

		private safeNoLockQueue<byte[]> m_send_buf_list_reliable;

		private safeNoLockQueue<byte[]> m_send_buf_list_unreliable;

		private LinkedList<udp_packet> m_send_list_reliable;

		private safeNoLockQueue<udp_packet> m_send_list_readyfor;

		internal ushort m_recv_seq_reliable;

		internal ushort m_request_seq_reliable;

		internal ushort m_last_response_seq_reliable;

		private int m_recv_count;

		private int m_recv_len;

		private int m_recv_ack_count;

		private safeNoLockQueue<udp_packet> m_recv_buf_list;

		private LinkedList<udp_packet> m_recv_order_list;

		private safeNoLockQueue<byte[]> m_ready_buf_list;

		private long m_srtt;

		private long m_rto;

		private DateTime m_last_send_time;

		private DateTime m_lase_recv_time;

		private DateTime m_need_request_time;

		private int m_request_time = 200;

		private bool m_run;

		private static Random ran = new Random(10);

		private bool waitSyn = true;

		private int curSynStep;

		private bool startSynTime;

		private DateTime sendSynTime;

		public int m_session_state = 2;

		public udp_session()
		{
			m_send_buf_list_reliable = new safeNoLockQueue<byte[]>();
			m_send_buf_list_unreliable = new safeNoLockQueue<byte[]>();
			m_send_list_reliable = new LinkedList<udp_packet>();
			m_send_list_readyfor = new safeNoLockQueue<udp_packet>();
			m_recv_buf_list = new safeNoLockQueue<udp_packet>();
			m_recv_order_list = new LinkedList<udp_packet>();
			m_ready_buf_list = new safeNoLockQueue<byte[]>();
			reset();
		}

		~udp_session()
		{
			close();
		}

		internal void connect(Socket __sock_fd, IPEndPoint __sock_addr)
		{
			udp_define.assert(!m_run);
			reset();
			m_sock_fd = __sock_fd;
			m_sock_addr = __sock_addr;
			m_run = true;
		}

		internal void close()
		{
			udp_define.assert(m_run);
			send_packet(m_sock_fd, m_sock_addr, new udp_packet(9, 0));
			reset();
			m_sock_fd = null;
			m_sock_addr = null;
			m_run = false;
			m_send_buf_list_reliable.Clear();
			m_send_buf_list_unreliable.Clear();
			m_send_list_reliable.Clear();
			m_recv_buf_list.Clear();
			m_recv_order_list.Clear();
			m_ready_buf_list.Clear();
		}

		protected void reset()
		{
			m_send_seq_reliable = 0;
			m_send_count = 0;
			m_send_len = 0;
			m_send_response_count = 0;
			m_last_send_response = 0;
			m_fast_retry = 0;
			m_timeout_retry = 0;
			m_recv_seq_reliable = 0;
			m_recv_count = 0;
			m_recv_len = 0;
			m_recv_ack_count = 0;
			m_srtt = 0L;
			m_rto = 3000000000L;
			m_request_seq_reliable = 0;
			m_last_response_seq_reliable = 0;
			m_last_send_time = DateTime.Now;
			m_lase_recv_time = DateTime.Now;
			m_need_request_time = DateTime.Now;
			sendSynTime = DateTime.Now;
			waitSyn = true;
			m_recv_order_list.Clear();
			m_recv_buf_list.Clear();
			m_ready_buf_list.Clear();
			m_send_buf_list_reliable.Clear();
			m_send_buf_list_unreliable.Clear();
			m_send_list_reliable.Clear();
			m_send_list_readyfor.Clear();
		}

		public static void send_packet(Socket __sock_fd, IPEndPoint __sock_addr, udp_packet __send)
		{
			byte[] array = __send.serialize();
			int num = array.Length;
			udp_define.assert(array != null && num > 0);
			array[0] = udp_packet.checksum(array, num);
			try
			{
				__sock_fd.Send(array);
				__send.print_packet(__sock_addr);
			}
			catch
			{
			}
		}

		internal void loop()
		{
			int send_count = m_send_count;
			int send_response_count = m_send_response_count;
			int recv_count = m_recv_count;
			int recv_ack_count = m_recv_ack_count;
			sync_time();
			proc_recv();
			proc_send();
			checkNeedHeartbeat();
			if (send_count != m_send_count || send_response_count != m_send_response_count || recv_count != m_recv_count || recv_ack_count != m_recv_ack_count)
			{
				print_status();
				udp_define.printf("\n");
			}
		}

		internal void push_send_buf(byte[] __buf, int __sendType)
		{
			if (__buf != null && __buf.Length != 0)
			{
				switch (__sendType)
				{
					case 0:
						m_send_buf_list_reliable.Enqueue(__buf);
						break;
					case 1:
						m_send_buf_list_unreliable.Enqueue(__buf);
						break;
				}
			}
		}

		public void push_recv_buf(udp_packet pack)
		{
			udp_define.assert(pack != null);
			udp_time.CalculateServerDelay(udp_time.GameNow() - pack.m_head.m_time_stamp);
			m_recv_buf_list.Enqueue(pack);
		}

		internal byte[] pop_ready_buf()
		{
			if (m_ready_buf_list.IsEmpty)
			{
				return null;
			}
			return m_ready_buf_list.Dequeue();
		}

		internal bool is_run()
		{
			return m_run;
		}

		protected void proc_send()
		{
			if (waitSyn || m_session_state == 2)
			{
				DateTime dateTime = sendSynTime;
				if ((double)(DateTime.Now.Ticks - sendSynTime.Ticks) * 0.0001 > 100.0)
				{
					udp_packet _send = new udp_packet(6, 0);
					send_packet(_send);
					sendSynTime = DateTime.Now;
				}
				return;
			}
			proc_send_reliable();
			proc_send_unreliable();
			while (!m_send_list_readyfor.IsEmpty)
			{
				udp_packet udp_packet2 = m_send_list_readyfor.Dequeue();
				if (udp_packet2 != null)
				{
					send_data(udp_packet2);
				}
			}
		}

		protected void proc_send_reliable()
		{
			udp_packet next_send_packet_reliable = get_next_send_packet_reliable();
			if (next_send_packet_reliable != null)
			{
				m_send_list_reliable.AddLast(next_send_packet_reliable);
				m_send_list_readyfor.Enqueue(next_send_packet_reliable);
			}
		}

		protected void proc_send_unreliable()
		{
			udp_packet next_send_packet_unreliable = get_next_send_packet_unreliable();
			if (next_send_packet_unreliable != null)
			{
				m_send_list_readyfor.Enqueue(next_send_packet_unreliable);
			}
		}

		protected void check_request()
		{
			if (m_recv_order_list.Count != 0)
			{
				if (udp_define.before(udp_define.next_seq(m_recv_seq_reliable), m_recv_order_list.First.Value.m_head.m_seq))
				{
					m_request_seq_reliable = udp_define.next_seq(m_recv_seq_reliable);
				}
				if (udp_define.next_seq(m_recv_seq_reliable) == m_request_seq_reliable && (double)(DateTime.Now.Ticks - m_need_request_time.Ticks) * 0.001 > (double)m_request_time)
				{
					send_request(udp_define.next_seq(m_recv_seq_reliable));
					m_need_request_time = DateTime.Now;
				}
			}
		}

		protected void sync_time()
		{
			switch (curSynStep)
			{
				case 1:
					if (!startSynTime)
					{
						udp_packet _send2 = new udp_packet(4, 1);
						send_packet(_send2);
						startSynTime = true;
					}
					break;
				case 2:
					if (udp_time.syn_time_3 == 0)
					{
						udp_packet _send = new udp_packet(4, 1);
						udp_time.syn_time_3 = udp_time.NowGMT();
						send_packet(_send);
					}
					break;
			}
		}

		protected void proc_recv()
		{
			while (!m_recv_buf_list.IsEmpty)
			{
				udp_packet udp_packet2 = m_recv_buf_list.Dequeue();
				m_lase_recv_time = DateTime.Now;
				udp_time.SetClientDelay(udp_packet2.m_head.m_net_delay);
				switch (udp_packet2.m_head.m_flags)
				{
					case 0:
						if (udp_packet2.m_head.m_seq == 0)
						{
							push_ready_buf(udp_packet2);
						}
						else
						{
							parse_recv(udp_packet2);
						}
						break;
					case 1:
						parse_ack(udp_packet2);
						break;
					case 2:
						parse_request(udp_packet2);
						break;
					case 3:
						push_ready_buf(udp_packet2);
						break;
					case 4:
						parse_sync_time(udp_packet2);
						break;
					case 6:
						reset();
						break;
					case 7:
						parse_sync_respone(udp_packet2);
						break;
				}
				udp_packet2.print_packet(m_sock_addr, bRev: true);
			}
			send_response();
			check_request();
		}

		protected void parse_recv(udp_packet __recv)
		{
			if (udp_define.after(__recv.m_head.m_seq, m_recv_seq_reliable))
			{
				if (udp_define.next_seq(m_recv_seq_reliable) == __recv.m_head.m_seq)
				{
					push_ready_buf(__recv);
					m_recv_seq_reliable = __recv.m_head.m_seq;
					m_need_request_time = DateTime.Now;
				}
				else
				{
					insert_recv_packet(__recv);
				}
			}
			while (m_recv_order_list.Count > 0 && udp_define.next_seq(m_recv_seq_reliable) == m_recv_order_list.First.Value.m_head.m_seq)
			{
				push_ready_buf(m_recv_order_list.First.Value);
				m_recv_seq_reliable = m_recv_order_list.First.Value.m_head.m_seq;
				m_recv_order_list.RemoveFirst();
			}
		}

		protected void parse_request(udp_packet __recv)
		{
			ushort seq = __recv.m_head.m_seq;
			foreach (udp_packet item in m_send_list_reliable)
			{
				if (item.m_head.m_seq == seq)
				{
					m_send_list_readyfor.Enqueue(item);
					break;
				}
			}
		}

		protected void parse_sync_respone(udp_packet __recv)
		{
			waitSyn = false;
			m_session_state = 1;
			udp_time.syn_time_1 = __recv.m_head.m_time_stamp;
			udp_time.syn_time_2 = udp_time.NowGMT();
			curSynStep = 2;
		}

		protected void parse_sync_time(udp_packet __recv)
		{
			switch (curSynStep)
			{
				case 1:
					udp_time.syn_time_1 = __recv.m_head.m_time_stamp;
					udp_time.syn_time_2 = udp_time.NowGMT();
					udp_define.printf("syn1:{0},syn2:{1}", udp_time.syn_time_1, udp_time.syn_time_2);
					curSynStep = 2;
					break;
				case 2:
					udp_time.syn_time_4 = __recv.m_head.m_time_stamp;
					udp_time.CalculateSyncTime();
					curSynStep = 3;
					break;
			}
		}

		protected void parse_ack(udp_packet __recv)
		{
			udp_define.assert(__recv.m_head.m_seq > 0);
			if (m_send_list_reliable.Count != 0)
			{
				while (m_send_list_reliable.Count > 0 && !udp_define.after(m_send_list_reliable.First.Value.m_head.m_seq, __recv.m_head.m_seq))
				{
					m_send_list_reliable.RemoveFirst();
				}
			}
		}

		protected udp_packet get_next_send_packet_unreliable()
		{
			udp_packet result = null;
			if (m_send_buf_list_unreliable.IsEmpty)
			{
				return result;
			}
			byte _flag = 3;
			m_send_seq_unreliable = udp_define.next_seq(m_send_seq_unreliable);
			result = new udp_packet(_flag, m_send_seq_unreliable);
			while (!m_send_buf_list_unreliable.IsEmpty)
			{
				byte[] array = m_send_buf_list_reliable.Dequeue();
				if (array.Length != 0)
				{
					result.push_data(array);
				}
			}
			return result;
		}

		protected udp_packet get_next_send_packet_reliable()
		{
			udp_packet result = null;
			if (m_send_buf_list_reliable.IsEmpty)
			{
				return result;
			}
			byte _flag = 0;
			m_send_seq_reliable = udp_define.next_seq(m_send_seq_reliable);
			result = new udp_packet(_flag, m_send_seq_reliable);
			while (!m_send_buf_list_reliable.IsEmpty)
			{
				byte[] array = m_send_buf_list_reliable.Dequeue();
				if (array.Length != 0)
				{
					result.push_data(array);
				}
			}
			return result;
		}

		protected void send_packet(udp_packet __send)
		{
			send_packet(m_sock_fd, m_sock_addr, __send);
			if (udp_define.has_flag(__send.m_head.m_flags, 1))
			{
				m_last_send_response = __send.m_head.m_seq;
			}
			m_last_send_time = DateTime.Now;
		}

		protected void send_data(udp_packet __send)
		{
			m_send_count++;
			send_packet(__send);
		}

		protected void send_request(ushort request_seq)
		{
			udp_packet _send = new udp_packet(2, request_seq);
			send_packet(_send);
		}

		protected void send_response()
		{
			if (m_recv_seq_reliable < 0 || m_recv_seq_reliable > m_last_response_seq_reliable)
			{
				udp_packet _send = new udp_packet(1, m_recv_seq_reliable);
				m_send_response_count++;
				m_last_response_seq_reliable = m_recv_seq_reliable;
				send_packet(_send);
			}
		}

		protected int insert_recv_packet(udp_packet __recv)
		{
			if (m_recv_order_list.Count == 0 || udp_define.after(__recv.m_head.m_seq, m_recv_order_list.Last.Value.m_head.m_seq))
			{
				m_recv_order_list.AddLast(__recv);
			}
			else
			{
				for (LinkedListNode<udp_packet> linkedListNode = m_recv_order_list.First; linkedListNode != null; linkedListNode = linkedListNode.Next)
				{
					if (linkedListNode.Value.m_head.m_seq == __recv.m_head.m_seq)
					{
						return -1;
					}
					if (!udp_define.after(__recv.m_head.m_seq, linkedListNode.Value.m_head.m_seq))
					{
						m_recv_order_list.AddBefore(linkedListNode, __recv);
						break;
					}
				}
			}
			return 0;
		}

		protected int push_ready_buf(udp_packet __recv)
		{
			if (__recv.m_data_list.Count == 0)
			{
				return 0;
			}
			int num = 0;
			foreach (byte[] item in __recv.m_data_list)
			{
				m_ready_buf_list.Enqueue(item);
				num += item.Length;
			}
			m_recv_len += num;
			return num;
		}

		protected long create_send_timestamp()
		{
			return udp_define.datatime_to_ns(DateTime.Now);
		}

		protected long get_timestamp_elapse(long __ts)
		{
			return Math.Abs(udp_define.datatime_to_ns(DateTime.Now) - __ts);
		}

		private void checkNeedHeartbeat()
		{
			if ((double)(DateTime.Now.Ticks - m_lase_recv_time.Ticks) * 0.0001 > 20000.0)
			{
				udp_define.printf("CONN_OVER_TIME!!!!!!!!!!!");
			}
			else if ((double)(DateTime.Now.Ticks - m_last_send_time.Ticks) * 0.0001 > 2000.0)
			{
				udp_packet _send = new udp_packet(8, 0);
				send_packet(_send);
			}
		}

		protected void update_rtt_rto(long __rtt)
		{
			if (m_srtt == 0)
			{
				m_srtt = __rtt;
			}
			else
			{
				m_srtt = (long)(0.875f * (float)m_srtt + 0.125f * (float)__rtt);
			}
			m_rto = Math.Min(Math.Max(m_srtt * 2, 300000000L), 6000000000L);
		}

		protected void print_status()
		{
		}
	}
}
