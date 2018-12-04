using com.zyd.common.proto.client;
using Google.ProtocolBuffers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

/// <summary>
/// 网络管理
/// </summary>
public class NetworkManager : Manager{
    private static Socket m_socket;
    private static bool m_isReady;
    private static bool m_isQueue;
 
    int m_receiveHeadSize;//头内容
    int m_receiveHeadShift; 
    byte[] m_receiveBuff;//接受的容器
    int m_receiveBuffPos;

    double m_totalTime;
    List<byte[]> m_receivePacket = new List<byte[]>();// receiving packet

    #region Public Method

    public void Init(bool sync = false)
    {
        m_isReady = false;
        m_isQueue = false;

        if (m_socket != null)
        {
            m_socket.Close();
            m_socket = null;
        }
        if (m_socket == null)
        {
            IPAddress[] addr = Dns.GetHostAddresses(Config.ServerHost);
            var clientEndPoint = new IPEndPoint(addr[0], Config.ServerHostPort);
            m_socket = new Socket(addr[0].AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            if (sync)
            {
                Debug.Log("connect!");
                m_socket.Connect(clientEndPoint);
            }
        }
        else
        {
            Connected(null);
        }
    }

    public void Request<T>(string name, T body) where T : IMessageLite
    {   
        MessageHeaderRequest.Builder header = MessageHeaderRequest.CreateBuilder();
        header.SetName(name);

        byte[] headerBytes = header.Build().ToByteArray();
        byte[] bodyBytes = body.ToByteArray();
        SendRequest(headerBytes, bodyBytes);
    }

    #endregion Public Method

    #region Private Method

    private void Connected(IAsyncResult ar)
    {
        if (m_socket.Connected)
        {
            //m_isReady = true;
            //App.Manager.Thread.RunOnMainThread(() =>
            //{
            //    Debugger.Log("Connected");
            //});
            //开始监听
            //m_isQueue = true;
            //m_Receive = App.Manager.Thread.RunAsync(QueueReceive);
            //m_process = App.Manager.Thread.RunAsync(QueueProcess);
            //m_timeOut = App.Manager.Thread.RunAsync(TimeTick);
            //m_heartBeat = App.Manager.Thread.RunAsync(HeartBeatProcess);
        }
    }

    private void SendRequest(byte[] header, byte[] body)
    {
        List<byte[]> buffs = new List<byte[]>();
        buffs.Add(NetworkProtoParser.SizeToVariant32(header));
        buffs.Add(header);
        buffs.Add(NetworkProtoParser.SizeToVariant32(body));
        buffs.Add(body);
        buffs.Add(new byte[1] { 0 });

        foreach(byte[] buff in buffs)
        {
            m_socket.Send(buff); 
        }

        StartCoroutine(OnDataReceived());
    }

    /// <summary>
    /// 接收网络包
    /// </summary>
    /// <param name="packet"></param>
    private void OnPackedReceived(List<byte[]> packets)
    {
        if (packets.Count <= 0)
        {
            return;
        }
        int error = (int)RpcErrorCode.UNKNOWN;
        MessageHeaderResponse header = null;
        header = MessageHeaderResponse.ParseFrom(packets[0]);
        if (header != null)
        {
            error = header.Error;
            packets.RemoveAt(0);
            Debug.Log("Error Code = " + error);
        }
    }

    private IEnumerator OnDataReceived()
    {
        byte[] temp_buff = new byte[1];
        while (m_socket != null && m_socket.Available > 0)
        {
            int available = m_socket.Available;
            // there is no receive buffer, means that we are still reading packet size.
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
    }

    #endregion Private Method
}
