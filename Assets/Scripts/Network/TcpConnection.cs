using System.Collections;
using System.Collections.Generic;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class TcpConnection : AConnection
{
    private Thread comThread;
    private TcpListener server;
    private TcpClient client;
    private NetworkStream netStream;
    private int comPort;
    private bool threadRunning;

    public override void StartConnection(int _receivePort, int _sendPort)
    {
        comPort = _receivePort;
        comThread = new Thread(ConnexionLoop);
        comThread.IsBackground = true;
        threadRunning = true;
        comThread.Start();
    }

    private void ConnexionLoop()
    {
        try
        {
            server = new TcpListener(IPAddress.Any, comPort);
            server.Start();
            GameManager.gm.AppendLogLine($"Tcp Server is listening at port {comPort}", false, eLogtype.info);
            while (threadRunning)
            {
                client = server.AcceptTcpClient();
                netStream = client.GetStream();
                ReceiveLoop();
                client.Close();
                GameManager.gm.AppendLogLine("Connection with client lost.", false, eLogtype.errorCli);
            }
        }
        catch (SocketException socketException)
        {
            GameManager.gm.AppendLogLine("SocketException " + socketException.ToString(), false, eLogtype.error);
        }
    }

    private void ReceiveLoop()
    {
        try
        {
            while (client.Connected)
            {
                string msg = ReceiveMessage();
                if (!string.IsNullOrEmpty(msg))
                    incomingQueue.Enqueue(msg);
            }
        }
        catch (System.IO.IOException e)
        {
            GameManager.gm.AppendLogLine(e.Message, false, eLogtype.errorCli);
        }
    }

    private string ReceiveMessage()
    {
        byte[] sizeBuffer = new byte[4];
        netStream.Read(sizeBuffer, 0, 4);
        int size = BitConverter.ToInt32(sizeBuffer, 0);

        byte[] msgBuffer = new byte[size];
        netStream.Read(msgBuffer, 0, size);
        string msg = Encoding.UTF8.GetString(msgBuffer);
        return msg;
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
        if (client == null || !client.Connected)
        {
            GameManager.gm.AppendLogLine("TCP client is closed.", false, eLogtype.errorCli);
            return;
        }
        try
        {
            byte[] msgBuffer = Encoding.UTF8.GetBytes(_message);
            Int32 size = msgBuffer.Length;
            byte[] sizeBuffer = BitConverter.GetBytes(size);
            netStream.Write(sizeBuffer, 0, 4);
            netStream.Write(msgBuffer, 0, size);
        }
        catch (SocketException se)
        {
            GameManager.gm.AppendLogLine("Socket exception: " + se, false, eLogtype.error);
        }
    }

    public override void Stop()
    {
        threadRunning = false;
        if (comThread.IsAlive)
            comThread.Abort();
        server.Stop();
    }
}
