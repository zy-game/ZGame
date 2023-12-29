/*************************************************************************************************
 * Copyright 2022 Theai, Inc. (DBA Inworld)
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/

using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Inworld.Grpc;
using Inworld.Packets;
using Inworld.Runtime;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Ai.Inworld.Studio.V1Alpha;
using UnityEngine;
using AudioChunk = Inworld.Packets.AudioChunk;
using ActionEvent = Inworld.Packets.ActionEvent;
using ControlEvent = Inworld.Packets.ControlEvent;
using CustomEvent = Inworld.Packets.CustomEvent;
using EmotionEvent = Inworld.Packets.EmotionEvent;
using GrpcPacket = Inworld.Grpc.InworldPacket;
using InworldPacket = Inworld.Packets.InworldPacket;
using Routing = Inworld.Packets.Routing;
using TextEvent = Inworld.Packets.TextEvent;


namespace Inworld
{
    public enum RPCState
    {
        None,
        Failur,
        Success,
    }

    public class RPCMessage : IDisposable
    {
        public RPCState State;
        public GrpcPacket Packet;

        public RPCMessage()
        {
            State = RPCState.None;
        }

        public RPCMessage(InworldPacket packet)
        {
            State = RPCState.None;
            Packet = packet.ToGrpc();
        }

        public void Dispose()
        {
            State = RPCState.None;
        }
    }

    /// <summary>
    ///     This class used to save the communication data in runtime.
    /// </summary>
    class Connection
    {
        // Audio chunks ready to play.
        internal readonly ConcurrentQueue<AudioChunk> incomingAudioQueue = new ConcurrentQueue<AudioChunk>();

        // Events that need to be processed by NPC.
        internal readonly ConcurrentQueue<InworldPacket> incomingInteractionsQueue = new ConcurrentQueue<InworldPacket>();

        // Events ready to send to server.
        internal readonly ConcurrentQueue<RPCMessage> outgoingEventsQueue = new ConcurrentQueue<RPCMessage>();
    }

    public class InworldAuth
    {
        private const string k_Method = "ai.inworld.engine.WorldEngine/GenerateToken";
        private const string k_RequestHead = "IW1-HMAC-SHA256";
        private const string k_RequestTail = "iw1_request";
        private SessionAccessToken m_AccessToken;

        private string _CurrentUtcTime => DateTime.UtcNow.ToString("yyyyMMddHHmmss");

        public DateTime ExpireTime => this.m_AccessToken == null ? DateTime.MinValue : this.m_AccessToken.ExpirationTime.ToDateTime();

        public AccessToken Token { get; set; }

        public string SessionID => this.m_AccessToken?.SessionId;

        public bool IsExpired => DateTime.UtcNow > this.ExpireTime;

        private static string Nonce
        {
            get
            {
                System.Random r = new System.Random();
                string nonce = "";
                for (int index = 0; index < 11; ++index)
                    nonce += r.Next(0, 10).ToString();
                return nonce;
            }
        }

        private string _GenerateSignature(List<string> strings, string strSecret)
        {
            HMACSHA256 hmacshA256 = new HMACSHA256();
            hmacshA256.Key = Encoding.UTF8.GetBytes("IW1" + strSecret);
            foreach (string s in strings)
                hmacshA256.Key = hmacshA256.ComputeHash(Encoding.UTF8.GetBytes(s));
            return BitConverter.ToString(hmacshA256.Key).ToLower().Replace("-", "");
        }

        public string GetHeader(string studioServer, string apiKey, string apiSecret)
        {
            List<string> strings = new List<string>();
            string currentUtcTime = this._CurrentUtcTime;
            string nonce = InworldAuth.Nonce;
            strings.Add(currentUtcTime);
            strings.Add(new Uri(studioServer).Scheme);
            strings.Add("ai.inworld.engine.WorldEngine/GenerateToken");
            strings.Add(nonce);
            strings.Add("iw1_request");
            string signature = this._GenerateSignature(strings, apiSecret);
            return "IW1-HMAC-SHA256 ApiKey=" + apiKey + ",DateTime=" + currentUtcTime + ",Nonce=" + nonce + ",Signature=" + signature;
        }
    }

    /// <summary>
    ///     This is the logic class for Server communication.
    /// </summary>
    public class InworldClient
    {
        public InworldClient()
        {
            m_Channel = new Channel(InworldConfig.RuntimeServer, new SslCredentials());
            m_WorldEngineClient = new WorldEngine.WorldEngineClient(m_Channel);
        }

        #region Private Variables

        readonly WorldEngine.WorldEngineClient m_WorldEngineClient;
        readonly Channel m_Channel;
        AsyncDuplexStreamingCall<GrpcPacket, GrpcPacket> m_StreamingCall;
        Connection m_CurrentConnection;
        InworldAuth m_InworldAuth;
        string m_SessionKey = "";
        Metadata m_Header;
        public event Action<RuntimeStatus, string> RuntimeEvent;

        #endregion

        #region Properties

        public ConcurrentQueue<Exception> Errors { get; } = new ConcurrentQueue<Exception>();
        public bool SessionStarted { get; private set; }
        public bool HasInit => !m_InworldAuth.IsExpired;
        public string SessionID => m_InworldAuth?.Token.SessionId ?? "";
        public string LastState { get; set; }
        bool IsSessionInitialized => m_SessionKey.Length != 0;
        Timestamp Now => Timestamp.FromDateTime(DateTime.UtcNow);

        #endregion

        #region Private Functions

        void _ReceiveCustomToken(string sessionToken)
        {
            JObject data = JObject.Parse(sessionToken);
            if (data.ContainsKey("sessionId") && data.ContainsKey("token"))
            {
                Debug.Log("Init Success with Custom Token!");
                m_Header = new Metadata
                {
                    { "authorization", $"Bearer {data["token"]}" },
                    { "session-id", data["sessionId"]?.ToString() }
                };
                RuntimeEvent?.Invoke(RuntimeStatus.InitSuccess, "");
            }
            else
                RuntimeEvent?.Invoke(RuntimeStatus.InitFailed, "Token Invalid");
        }

        public void GetAppAuth(string fullName, string key, string secret, string sessionToken = "")
        {
            m_InworldAuth = new InworldAuth();
            if (string.IsNullOrEmpty(sessionToken))
            {
                GenerateTokenRequest gtRequest = new GenerateTokenRequest
                {
                    Key = key,
                    Resources =
                    {
                        fullName
                    }
                };
                Metadata metadata = new Metadata
                {
                    { "authorization", m_InworldAuth.GetHeader(InworldConfig.RuntimeServer, key, secret) }
                };
                try
                {
                    m_InworldAuth.Token = m_WorldEngineClient.GenerateToken(gtRequest, metadata, DateTime.UtcNow.AddHours(1));
                    Debug.Log("Init Success!");
                    m_Header = new Metadata
                    {
                        { "authorization", $"Bearer {m_InworldAuth.Token.Token}" },
                        { "session-id", m_InworldAuth.Token.SessionId }
                    };
                    RuntimeEvent?.Invoke(RuntimeStatus.InitSuccess, "");
                }
                catch (RpcException e)
                {
                    RuntimeEvent?.Invoke(RuntimeStatus.InitFailed, e.ToString());
                }
            }
            else
            {
                _ReceiveCustomToken(sessionToken);
            }
        }

        public async Task<LoadSceneResponse> LoadScene(string sceneName)
        {
            Debug.Log(sceneName);
            LoadSceneRequest lsRequest = new LoadSceneRequest
            {
                Name = sceneName,
                Capabilities = new CapabilitiesRequest
                {
                    Audio = true,
                    NarratedActions = true,
                    Emotions = true,
                    Relations = true,
                    Interruptions = true,
                    Text = true,
                    DebugInfo = true,
                    Triggers = true,
                    Continuation = false,
                    TurnBasedStt = !false,
                    PhonemeInfo = true
                },
                User = new UserRequest
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = "DEFAULT"
                },
                Client = new ClientRequest
                {
                    Id = "unity",
                    Version = "1.0.1"
                },
                UserSettings = new UserSettings
                {
                    ViewTranscriptConsent = true,
                    PlayerProfile = new UserSettings.Types.PlayerProfile
                    {
                        Fields =
                        {
                            new UserSettings.Types.PlayerProfile.Types.PlayerField()
                            {
                                FieldId = "Level",
                                FieldValue = "18"
                            }
                        }
                    }
                }
            };
            if (!string.IsNullOrEmpty(LastState))
            {
                lsRequest.SessionContinuation = new SessionContinuation
                {
                    PreviousState = ByteString.FromBase64(LastState)
                };
            }

            try
            {
                LoadSceneResponse response = await m_WorldEngineClient.LoadSceneAsync(lsRequest, m_Header);
                // Yan: They somehow use {WorkSpace}:{sessionKey} as "sessionKey" now. Need to remove the first part.
                m_SessionKey = response.Key.Split(':')[1];
                if (response.PreviousState != null)
                {
                    foreach (PreviousState.Types.StateHolder stateHolder in response.PreviousState.StateHolders)
                    {
                        Debug.Log($"Received Previous Packets: {stateHolder.Packets.Count}");
                    }
                }

                m_Header.Add("Authorization", $"Bearer {m_SessionKey}");
                RuntimeEvent?.Invoke(RuntimeStatus.LoadSceneComplete, m_SessionKey);
                return response;
            }
            catch (RpcException e)
            {
                RuntimeEvent?.Invoke(RuntimeStatus.LoadSceneFailed, e.ToString());
                return null;
            }
        }

        // Marks audio session start.
        public void StartAudio(Routing routing)
        {
            Debug.Log("Start Audio Event");
            if (SessionStarted)
                m_CurrentConnection?.outgoingEventsQueue.Enqueue
                (
                    new RPCMessage()
                    {
                        Packet = new GrpcPacket
                        {
                            Timestamp = Now,
                            Routing = routing.ToGrpc(),
                            Control = new Inworld.Grpc.ControlEvent
                            {
                                Action = Inworld.Grpc.ControlEvent.Types.Action.AudioSessionStart
                            }
                        }
                    }
                );
        }

        // Marks session end.
        public void EndAudio(Routing routing)
        {
            if (SessionStarted)
                m_CurrentConnection?.outgoingEventsQueue.Enqueue
                (
                    new RPCMessage()
                    {
                        Packet = new GrpcPacket
                        {
                            Timestamp = Now,
                            Routing = routing.ToGrpc(),
                            Control = new Inworld.Grpc.ControlEvent
                            {
                                Action = Inworld.Grpc.ControlEvent.Types.Action.AudioSessionEnd
                            }
                        }
                    }
                );
        }

        // Sends audio chunk to server.
        public void SendAudio(AudioChunk audioEvent)
        {
            if (SessionStarted)
                m_CurrentConnection?.outgoingEventsQueue.Enqueue(new RPCMessage()
                {
                    Packet = audioEvent.ToGrpc()
                });
        }

        public bool GetAudioChunk(out AudioChunk chunk)
        {
            if (m_CurrentConnection != null)
            {
                return m_CurrentConnection.incomingAudioQueue.TryDequeue(out chunk);
            }

            chunk = null;
            return false;
        }

        public void SendEvent(RPCMessage message)
        {
            if (SessionStarted)
                m_CurrentConnection?.outgoingEventsQueue.Enqueue(message);
        }

        public bool GetIncomingEvent(out InworldPacket incomingEvent)
        {
            if (m_CurrentConnection != null)
            {
                return m_CurrentConnection.incomingInteractionsQueue.TryDequeue(out incomingEvent);
            }

            incomingEvent = null;
            return false;
        }

        public async Task StartSession()
        {
            if (!IsSessionInitialized)
            {
                throw new ArgumentException("No sessionKey to start Inworld session, use CreateWorld first.");
            }

            // New queue for new session.
            Connection connection = new Connection();
            m_CurrentConnection = connection;

            SessionStarted = true;
            try
            {
                using (m_StreamingCall = m_WorldEngineClient.Session(m_Header))
                {
                    // https://grpc.github.io/grpc/csharp/api/Grpc.Core.IAsyncStreamReader-1.html
                    Task inputTask = Task.Run
                    (
                        async () =>
                        {
                            while (SessionStarted)
                            {
                                bool next;
                                try
                                {
                                    // Waiting response for some time before checking if done.
                                    next = await m_StreamingCall.ResponseStream.MoveNext();
                                }
                                catch (RpcException rpcException)
                                {
                                    if (rpcException.StatusCode == StatusCode.Cancelled)
                                    {
                                        next = false;
                                    }
                                    else
                                    {
                                        // rethrowing other errors.
                                        throw;
                                    }
                                }

                                if (next)
                                {
                                    _ResolveGRPCPackets(m_StreamingCall.ResponseStream.Current);
                                }
                                else
                                {
                                    Debug.Log("Session is closed.");
                                    break;
                                }
                            }
                        }
                    );
                    Task outputTask = Task.Run
                    (
                        async () =>
                        {
                            while (SessionStarted)
                            {
                                Task.Delay(100).Wait();
                                // Sending all outgoing events.
                                while (connection.outgoingEventsQueue.TryDequeue(out RPCMessage e))
                                {
                                    if (SessionStarted)
                                    {
                                        await m_StreamingCall.RequestStream.WriteAsync(e.Packet);
                                        e.State = RPCState.Success;
                                    }
                                }
                            }
                        }
                    );
                    await Task.WhenAll(inputTask, outputTask);
                }
            }
            catch (Exception e)
            {
                SessionStarted = false;
                Errors.Enqueue(e);
            }
            finally
            {
                SessionStarted = false;
            }
        }

        internal TextEvent ResolvePreviousPackets(GrpcPacket response) => response.Text != null ? new TextEvent(response) : null;

        void _ResolveGRPCPackets(GrpcPacket response)
        {
            m_CurrentConnection ??= new Connection();
            if (response.DataChunk != null)
            {
                switch (response.DataChunk.Type)
                {
                    case DataChunk.Types.DataType.Audio:
                        m_CurrentConnection.incomingAudioQueue.Enqueue(new AudioChunk(response));
                        break;
                    case DataChunk.Types.DataType.State:
                        StateChunk stateChunk = new StateChunk(response);
                        LastState = stateChunk.Chunk.ToBase64();
                        break;
                    default:
                        Debug.LogError($"Unsupported incoming event: {response}");
                        break;
                }
            }
            else if (response.Text != null)
            {
                m_CurrentConnection.incomingInteractionsQueue.Enqueue(new TextEvent(response));
            }
            else if (response.Control != null)
            {
                m_CurrentConnection.incomingInteractionsQueue.Enqueue(new ControlEvent(response));
            }
            else if (response.Emotion != null)
            {
                m_CurrentConnection.incomingInteractionsQueue.Enqueue(new EmotionEvent(response));
            }
            else if (response.Action != null)
            {
                m_CurrentConnection.incomingInteractionsQueue.Enqueue(new ActionEvent(response));
            }
            else if (response.Custom != null)
            {
                m_CurrentConnection.incomingInteractionsQueue.Enqueue(new CustomEvent(response));
            }
            else if (response.DebugInfo != null && response.DebugInfo.InfoCase == DebugInfoEvent.InfoOneofCase.Relation)
            {
                m_CurrentConnection.incomingInteractionsQueue.Enqueue(new RelationEvent(response));
            }
            else
            {
                Debug.LogError($"Unsupported incoming event: {response}");
            }
        }

        internal async Task EndSession()
        {
            if (SessionStarted)
            {
                m_CurrentConnection = null;
                SessionStarted = false;
                await m_StreamingCall.RequestStream.CompleteAsync();
                m_StreamingCall.Dispose();
            }
        }

        public void Destroy()
        {
#pragma warning disable CS4014
            EndSession();
#pragma warning restore CS4014
            m_Channel.ShutdownAsync();
        }

        #endregion
    }
}