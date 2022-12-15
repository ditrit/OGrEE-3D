using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class UdpConnection : AConnection
{
    private UdpClient udpClient;

    private Thread receiveThread;
    private bool threadRunning = false;
    private string senderIp;
    private int cliPort;

    public override void StartConnection(int _receivePort)
    {
        try
        {
            udpClient = new UdpClient(_receivePort);
            cliPort = _receivePort;
        }
        catch (Exception e)
        {
            Debug.Log($"Failed to listen for UDP at port {_receivePort}: {e.Message}");
            return;
        }
        Debug.Log($"Created receiving client at ip  and port {_receivePort}");
        StartReceiveThread();
    }

    private void StartReceiveThread()
    {
        receiveThread = new Thread(() => ListenForMessages(udpClient))
        {
            IsBackground = true
        };
        threadRunning = true;
        receiveThread.Start();
    }

    private void ListenForMessages(UdpClient _client)
    {
        IPEndPoint remoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);

        while (threadRunning)
        {
            try
            {
                Byte[] receiveBytes = _client.Receive(ref remoteIpEndPoint); // Blocks until a message returns on this socket from a remote host.
                string returnData = Encoding.UTF8.GetString(receiveBytes);

                lock (incomingQueue)
                {
                    incomingQueue.Enqueue(returnData);
                    // Set sender data according to received client.
                    senderIp = remoteIpEndPoint.Address.ToString();
                    cliPort = remoteIpEndPoint.Port;
                    Debug.Log($"=> Received msg from {remoteIpEndPoint.Address}:{remoteIpEndPoint.Port}: {returnData}");

                    // Then, send automatic message (debug).
                    Send("Roger Roger");
                }
            }
            catch (SocketException e)
            {
                // 10004 thrown when socket is closed
                if (e.ErrorCode != 10004) Debug.Log("Socket exception while receiving data from udp client: " + e.Message);
            }
            catch (Exception e)
            {
                Debug.Log("Error receiving data from udp client: " + e.Message);
            }
            Thread.Sleep(1);
        }
    }

    public override string[] GetMessages()
    {
        string[] pendingMessages = new string[0];
        lock (incomingQueue)
        {
            pendingMessages = new string[incomingQueue.Count];
            int i = 0;
            while (incomingQueue.Count != 0)
            {
                pendingMessages[i] = incomingQueue.Dequeue();
                i++;
            }
        }
        return pendingMessages;
    }

    public override void Send(string _message)
    {
        Debug.Log(String.Format($"Send msg to ip:{senderIp}:{cliPort} msg: {_message}"));
        IPEndPoint serverEndpoint = new IPEndPoint(IPAddress.Parse(senderIp), cliPort);
        Byte[] sendBytes = Encoding.UTF8.GetBytes(_message);
        udpClient.Send(sendBytes, sendBytes.Length, serverEndpoint);
    }

    public override void Stop()
    {
        threadRunning = false;
        if (receiveThread.IsAlive)
            receiveThread.Abort();
        udpClient.Close();
    }
}