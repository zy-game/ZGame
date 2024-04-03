using System.Net;
using System.Net.Sockets;
using UnityEngine;
using LiteNetLib;

public class GameClient : MonoBehaviour, INetEventListener
{
    private NetManager _netClient;

    [SerializeField] private GameObject _clientBall;
    [SerializeField] private GameObject _clientBallInterpolated;

    private float _newBallPosX;
    private float _oldBallPosX;
    private float _lerpTime;

    private void Start()
    {
        _netClient = new NetManager(this);
        _netClient.UnconnectedMessagesEnabled = true;
        _netClient.UpdateTime = 15;
        _netClient.Start();
    }

    private void Update()
    {
        _netClient.PollEvents();

        var peer = _netClient.FirstPeer;
        if (peer != null && peer.ConnectionState == ConnectionState.Connected)
        {
            //Fixed delta set to 0.05
            var pos = _clientBallInterpolated.transform.position;
            pos.x = Mathf.Lerp(_oldBallPosX, _newBallPosX, _lerpTime);
            _clientBallInterpolated.transform.position = pos;

            //Basic lerp
            _lerpTime += Time.deltaTime / Time.fixedDeltaTime;
        }
        else
        {
            _netClient.SendBroadcast(new byte[] {1}, 5000);
        }
    }

    private void OnDestroy()
    {
        if (_netClient != null)
            _netClient.Stop();
    }

    void INetEventListener.OnPeerConnected(NetPeer peer)
    {
        Debug.Log("[CLIENT] We connected to " + peer);
    }

    void INetEventListener.OnNetworkError(IPEndPoint endPoint, SocketError socketErrorCode)
    {
        Debug.Log("[CLIENT] We received error " + socketErrorCode);
    }

    void INetEventListener.OnNetworkReceive(NetPeer peer, NetPacketReader reader, byte channelNumber, DeliveryMethod deliveryMethod)
    {
        _newBallPosX = reader.GetFloat();

        var pos = _clientBall.transform.position;

        _oldBallPosX = pos.x;
        pos.x = _newBallPosX;

        _clientBall.transform.position = pos;

        _lerpTime = 0f;
    }

    void INetEventListener.OnNetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType)
    {
        if (messageType == UnconnectedMessageType.BasicMessage && _netClient.ConnectedPeersCount == 0 && reader.GetInt() == 1)
        {
            Debug.Log("[CLIENT] Received discovery response. Connecting to: " + remoteEndPoint);
            _netClient.Connect(remoteEndPoint, "sample_app");
        }
    }

    void INetEventListener.OnNetworkLatencyUpdate(NetPeer peer, int latency)
    {

    }

    void INetEventListener.OnConnectionRequest(ConnectionRequest request)
    {

    }

    void INetEventListener.OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
    {
        Debug.Log("[CLIENT] We disconnected because " + disconnectInfo.Reason);
    }
}
// EventBasedNetListener listener = new EventBasedNetListener();
// NetManager client = new NetManager(listener);
// client.Start();
// client.Connect("localhost" /* host ip or name */, 9050 /* port */, "SomeConnectionKey" /* text key or NetDataWriter */);
// listener.NetworkReceiveEvent += (fromPeer, dataReader, deliveryMethod, channel) =>
// {
//     Console.WriteLine("We got: {0}", dataReader.GetString(100 /* max length of string */));
//     dataReader.Recycle();
// };
//
// while (!Console.KeyAvailable)
// {
//     client.PollEvents();
//     Thread.Sleep(15);
// }
//
// client.Stop();

// EventBasedNetListener listener = new EventBasedNetListener();
// NetManager server = new NetManager(listener);
// server.Start(9050 /* port */);
//
// listener.ConnectionRequestEvent += request =>
// {
//     if(server.ConnectedPeersCount < 10 /* max connections */)
//         request.AcceptIfKey("SomeConnectionKey");
//     else
//         request.Reject();
// };
//
// listener.PeerConnectedEvent += peer =>
// {
//     Console.WriteLine("We got connection: {0}", peer);  // Show peer ip
//     NetDataWriter writer = new NetDataWriter();         // Create writer class
//     writer.Put("Hello client!");                        // Put some string
//     peer.Send(writer, DeliveryMethod.ReliableOrdered);  // Send with reliability
// };
//
// while (!Console.KeyAvailable)
// {
//     server.PollEvents();
//     Thread.Sleep(15);
// }
// server.Stop();