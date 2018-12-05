using com.zyd.common.proto.client;
using Google.ProtocolBuffers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

namespace Framework
{
    enum NetworkType
    {
        TCP = 1,
        UDP = 2,
    }

    class SocketConfig
    {
        public string host;
        public int port;
        public int connectedTimeout;
        public int loopTime;
        public int pipelineTime;
        public int heartBeatTime;
        public NetworkType type;
        public int blockSize;
        public SocketConfig(string host, int port, int type, int blockSize, int connectedTimeout, int loopTime, int pipelineTime, int heartBeatTime)
        {
            this.host = host;
            this.port = port;
            this.type = (NetworkType)type;
            this.connectedTimeout = connectedTimeout;
            this.loopTime = loopTime;
            this.pipelineTime = pipelineTime;
            this.heartBeatTime = heartBeatTime;
            this.blockSize = blockSize;
        }
    }

    /// <summary>
    /// 网络管理
    /// </summary>
    public class NetworkManager : Manager
    {
        public delegate void DelegateRPC(int error, List<byte[]> bytes);
        public delegate void DelegatePushRPC(string name, List<byte[]> bytes);
        private Socket m_socket;     //当前连接
        private SocketConfig m_Config;
        private DelegateRPC m_callback;
        private DelegatePushRPC pushrpc = null;
        private bool m_DealPipeline;

        private static bool m_isReady;      //
        private static bool m_isQueue;

        private int m_receiveHeadSize;              //头内容
        int m_receiveHeadShift;
        byte[] m_receiveBuff;               //接受的容器
        int m_receiveBuffPos;
       
        bool m_Loop;
        private bool m_HeartBeat;
        List<byte[]> m_Pipeline;
        long m_HeartBeatLast;
        List<byte[]> m_receivePacket = new List<byte[]>();//收到的包

        #region Public Method

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="sync"></param>
        public void Init(bool sync = false)
        {
            Release();
            m_isReady = false;
            //m_isQueue = false;
        }


        public void Connect()
        {
            Close();
            //todo callback
            OpenSocket();
        }

        public void ClearSending()
        {
            m_isReady = false;
        }

        public bool IsConnected()
        {
            return (m_socket != null && m_socket.Connected);
        }

        /// <summary>
        /// 释放连接
        /// </summary>
        public void Release()
        {
            Close();
        }

        /// <summary>
        /// 关闭连接
        /// </summary>
        /// <param name="cnst_network"></param>
        public void Close()
        {
            ReleaseNetwork();
        }

        public void Request<T>(string name, T body) where T : IMessageLite
        {
            MessageHeaderRequest.Builder header = MessageHeaderRequest.CreateBuilder();
            header.SetName(name);

            byte[] headerBytes = header.Build().ToByteArray();
            byte[] bodyBytes = body.ToByteArray();
            SendRequest(headerBytes, bodyBytes);
        }

        public void SetConfig(string host, int port, int type, int blockSize, int connectedTimeout, int loopTime, int pipelineTime, int heartBeatTime)
        {
            SocketConfig config = new SocketConfig(host, port, type, blockSize, connectedTimeout, loopTime, pipelineTime, heartBeatTime);
            m_Config = config;
        }

        SocketConfig GetConfig()
        {
            return m_Config;
        }

        #endregion Public Method

        #region Private Method

        /// <summary>
        /// 发送心跳包
        /// </summary>
        /// <returns></returns>
        private IEnumerator StartHeartBeat()
        {
            while (m_HeartBeat)
            {
                MessageHeaderRequest.Builder header = MessageHeaderRequest.CreateBuilder();
                header.SetName("Heart");
                byte[] headerBytes = header.Build().ToByteArray();
                List<byte[]> buffs = new List<byte[]>();
                buffs.Add(NetworkProtoParser.SizeToVariant32(headerBytes));
                buffs.Add(headerBytes);
                buffs.Add(new byte[1] { 0 });
                foreach (byte[] buff in buffs)
                {
                    m_socket.Send(buff);
                }
                yield return null;
            }
        }

        private void OpenSocket()
        {
            SocketConfig config = GetConfig();
            if (config == null)
            {
                Debugger.LogError("No NetworkConfig, Connect Failed.");
                return;
            }
            Socket socket = null;
            switch (config.type)
            {
                case NetworkType.TCP:
                    IPAddress[] addr = Dns.GetHostAddresses(config.host);
                    var clientEndPoint = new IPEndPoint(addr[0], config.port);
                    m_socket = new Socket(addr[0].AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                    socket.Connect(clientEndPoint);
                    if (socket.Connected)
                    {
                        OnConnected(true);
                    }
                    else
                    {
                        Debug.Log("Socket Connected Failed!");
                    }
                    break;
            }
        }

        private void OnConnected(bool success)
        {
            if (success && m_socket!= null)
            {
                m_Pipeline = new List<byte[]>();
                m_DealPipeline = true;
                m_HeartBeat = true;
                m_Loop = true;
                StartCoroutine(OnDataReceived());
            }
            else
            {
                m_socket = null;
            }
        }

        private void OnDisconnected()
        {
            Debug.Log("Socket Disconnected!");
            ReleaseNetwork();
            //todo callback
        }

        private void SendRequest(byte[] header, byte[] body)
        {
            List<byte[]> buffs = new List<byte[]>();
            buffs.Add(NetworkProtoParser.SizeToVariant32(header));
            buffs.Add(header);
            buffs.Add(NetworkProtoParser.SizeToVariant32(body));
            buffs.Add(body);
            buffs.Add(new byte[1] { 0 });

            foreach (byte[] buff in buffs)
            {
                m_socket.Send(buff);
            }
            m_isReady = true;
        }

        /// <summary>
        /// 接收网络包
        /// </summary>
        /// <param name="packet"></param>
        private void OnPackedReceived(List<byte[]> packet)
        {
            if (packet.Count <= 0)
            {
                Debug.Log("Packets has no data!!");
                return;
            }
            int error = (int)RpcErrorCode.UNKNOWN;
            MessageHeaderResponse header = null;
            header = MessageHeaderResponse.ParseFrom(packet[0]);
            if (header != null)
            {
                error = header.Error;
                packet.RemoveAt(0);
                Debug.Log("Header Error Code = " + error);
            }

            //push 的消息
            if (header.Name.Contains("#"))
            {
                ResponsePush(header.Name, packet);
            }
            else if (m_isReady)
            {
                /// 正常收到的消息包
                ClearSending();
                ResponseCallback(m_callback, error, packet);
            }
        }

        protected void ResponseCallback(DelegateRPC callback, int error, List<byte[]> packet)
        {
            // 回调上层
            if (callback != null)
            {
                callback(error, packet);
            }
        }

        protected void ResponsePush(string name, List<byte[]> packet)
        {
            if (pushrpc != null)
            {
                pushrpc(name, packet);
            }
        }

        /// <summary>
        /// 接收数据
        /// </summary>
        /// <returns></returns>
        private IEnumerator OnDataReceived()
        {
            byte[] temp_buff = new byte[1];
            if(m_socket != null)
            {
                while (!m_socket.Connected)
                {
                    int available = m_socket.Available;
                    if (available > 0)
                    {                  
                        if (m_receiveBuff == null)
                        {
                            if (1 != m_socket.Receive(temp_buff, 1, SocketFlags.None))
                                yield return null;
                            byte data = temp_buff[0];
                            m_receiveHeadSize |= (data & 0x7f) << m_receiveHeadShift;

                            // still has data
                            if ((data & 0x80) != 0)
                            {
                                m_receiveHeadShift += 7;
                            }
                            else
                            {
                                // a zero size means buffer sequence ends, a whole packet is received.
                                if (m_receiveHeadSize == 0)
                                {
                                    m_receiveHeadSize = 0;
                                    m_receiveHeadShift = 0;
                                    OnPackedReceived(m_receivePacket);
                                    m_receivePacket.Clear();
                                    break;
                                }
                                else
                                {
                                    int buffer_size = m_receiveHeadSize - 1;
                                    m_receiveHeadSize = 0;
                                    m_receiveHeadShift = 0;
                                    m_receiveBuffPos = 0;

                                    if (buffer_size > 0)
                                        m_receiveBuff = new byte[buffer_size];
                                    else
                                        m_receivePacket.Add(new byte[0]);
                                }
                            }
                        }
                    }

                    // we are receiving the packet content
                    else
                    {
                        // size can read at this time.
                        int recv_size = m_receiveBuff.Length - m_receiveBuffPos;
                        if (recv_size > available)
                            recv_size = available;
                        // read data
                        int num_readed = m_socket.Receive(m_receiveBuff, m_receiveBuffPos, recv_size, SocketFlags.None);
                        // move read position
                        m_receiveBuffPos += num_readed;
                        // the entire buffer is received
                        if (m_receiveBuffPos >= m_receiveBuff.Length)
                        {
                            m_receivePacket.Add(m_receiveBuff);
                            m_receiveBuff = null;
                        }
                    }
                }
                OnDisconnected();
            }
        }

        /// <summary>
        /// 释放
        /// </summary>
        private void ReleaseNetwork()
        {
            Socket socket = null;
            if (m_socket != null) socket = m_socket;
            if (socket != null)
            {
                socket.Close();
                m_DealPipeline = false;
                m_HeartBeat = false;
                m_Loop = false;
                m_Pipeline = null;
            }
        }

        #endregion Private Method
    }
}
