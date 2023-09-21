using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using ProtoBuf;

namespace ZEngine.Network
{
    public class KCPChannel : IChannel
    {
        private UdpClient client;
        private volatile bool running;
        private Kcp kcp;
        private IPEndPoint serverAddr;
        private Object LOCK = new Object(); //加锁访问收到的数据
        private Object SEND_LOCK = new Object(); //加锁访问发送列表
        private LinkedList<ByteBuf> received;
        private LinkedList<ByteBuf> sendList;
        private volatile bool needUpdate;
        private long timeout; //超时
        private DateTime lastTime; //上次检测时间
        private IPEndPoint curAddr; //当前的客户端地址
        private int cid;
        private ushort port;

        public bool IsConnected => running;
        public string address { get; set; }

        public bool connected { get; }

        public void Dispose()
        {
        }

        public IChannelClosedExecuteHandle Close()
        {
            running = false;
            try
            {
                this.client.Close();
            }
            catch (Exception ex)
            {
            }

            return default;
        }

        public void SetConv(int id)
        {
            this.cid = id;
        }

        public IChannelConnectExecuteHandle Connect(string address, int id = 0)
        {
            try
            {
                this.received = new LinkedList<ByteBuf>();
                this.sendList = new LinkedList<ByteBuf>();
                this.address = address;
                this.timeout = (10 * 1000);
                this.kcp.SetMinRto(10);
                UriBuilder uri = new UriBuilder(address);
                serverAddr = new IPEndPoint(IPAddress.Parse(uri.Host), uri.Port);
                client = new UdpClient(port);
                kcp = new Kcp(cid, output, null);
                kcp.SetConv(cid);
                kcp.NoDelay(1, 10, 2, 1);
                kcp.WndSize(64, 64);
                kcp.SetMtu(512);
                if (serverAddr != null)
                {
                    this.client.Connect(serverAddr);
                }

                client.BeginReceive(Received, client);
                this.running = true;
                Thread t = new Thread(new ThreadStart(run)); //状态更新
                t.IsBackground = true;
                t.Start();
            }
            catch (Exception ex)
            {
            }

            return default;
        }

        public IChannelWriteExecuteHandle WriteAndFlush(byte[] bytes)
        {
            Send(new ByteBuf(bytes));
            return default;
        }


        private void run()
        {
            while (running)
            {
                DateTime st = DateTime.Now;
                this.Update();
                if (this.needUpdate)
                {
                    continue;
                }

                DateTime end = DateTime.Now;
                while ((end - st).TotalMilliseconds < 10)
                {
                    if (this.needUpdate)
                    {
                        break;
                    }

                    Thread.Sleep(0);
                    end = DateTime.Now;
                }
            }
        }


        private void output(ByteBuf msg, Object user)
        {
            this.client.Send(msg.GetRaw(), msg.ReadableBytes());
        }

        private void Received(IAsyncResult ar)
        {
            UdpClient client = (UdpClient)ar.AsyncState;
            try
            {
                byte[] data = client.EndReceive(ar, ref this.curAddr);
                lock (LOCK)
                {
                    this.received.AddLast(new ByteBuf(data));
                    this.needUpdate = true;
                    this.lastTime = DateTime.Now;
                }

                client.BeginReceive(Received, ar.AsyncState);
            }
            catch (Exception ex)
            {
            }
        }

        /// <summary>
        /// 发送
        /// </summary>
        /// <param name="content"></param>
        private void Send(ByteBuf content)
        {
            lock (this.SEND_LOCK)
            {
                this.sendList.AddLast(content);
                this.needUpdate = true;
            }
        }

        /// <summary>
        /// 更新
        /// </summary>
        private void Update()
        {
            //input
            lock (LOCK)
            {
                while (this.received.Count > 0)
                {
                    ByteBuf bb = this.received.First.Value;
                    kcp.Input(bb);
                    this.received.RemoveFirst();
                }
            }

            //receive
            int len;
            while ((len = kcp.PeekSize()) > 0)
            {
                ByteBuf bb = new ByteBuf(len);
                int n = kcp.Receive(bb);
                if (n > 0)
                {
                    //todo 收到消息
                }
            }

            //send
            lock (this.SEND_LOCK)
            {
                while (this.sendList.Count > 0)
                {
                    ByteBuf item = this.sendList.First.Value;
                    this.kcp.Send(item);
                    this.sendList.RemoveFirst();
                }
            }

            //update kcp status
            int cur = (int)DateTime.Now.Ticks;
            if (this.needUpdate || cur >= kcp.GetNextUpdate())
            {
                kcp.Update(cur);
                kcp.SetNextUpdate(kcp.Check(cur));
                this.needUpdate = false;
            }

            //check timeout
            if (this.timeout > 0 && lastTime != DateTime.MinValue)
            {
                double del = (DateTime.Now - this.lastTime).TotalMilliseconds;
                if (del > this.timeout)
                {
                    Engine.Console.Error(new TimeoutException(address));
                }
            }
        }
    }
}