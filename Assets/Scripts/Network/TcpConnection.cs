using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using System.Threading.Tasks;

public class TcpConnection : AConnection
{
    private Thread comThread;
    private TcpListener server;
    private TcpClient client;
    private NetworkStream netStream;
    private int cliPort;
    private bool threadRunning;

    public override void StartConnection(int _receivePort)
    {
        cliPort = _receivePort;
        comThread = new(ConnexionLoop)
        {
            IsBackground = true
        };
        threadRunning = true;
        GameManager.instance.AppendLogLine($"Tcp Server will listen at port {cliPort}", ELogTarget.logger, ELogtype.info);
        comThread.Start();
    }

    private async void ConnexionLoop()
    {
        try
        {
            server = new(IPAddress.Any, cliPort);
            server.Start();
            while (threadRunning)
            {
                client = server.AcceptTcpClient();
                netStream = client.GetStream();
                await ReceiveLoop();
                client.Close();
                mainThreadQueue.Enqueue(() => GameManager.instance.AppendLogLine("Connection with client lost.", ELogTarget.logger, ELogtype.errorCli));
            }
        }
        catch (SocketException socketException)
        {
            mainThreadQueue.Enqueue(() => GameManager.instance.AppendLogLine("SocketException " + socketException.ToString(), ELogTarget.logger, ELogtype.error));
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }

    private async Task ReceiveLoop()
    {
        try
        {
            while (client.Connected)
            {
                string msg = await ReceiveMessage();
                if (!string.IsNullOrEmpty(msg))
                    incomingQueue.Enqueue(msg);
            }
        }
        catch (System.IO.IOException e)
        {
            GameManager.instance.AppendLogLine(e.Message, ELogTarget.none, ELogtype.errorCli);
        }
    }

    private async Task<string> ReceiveMessage()
    {
        byte[] sizeBuffer = await ReadBytes(4);
        int size = BitConverter.ToInt32(sizeBuffer, 0);
        // Debug.Log($"Size of incoming message: {size}");

        byte[] msgBuffer = await ReadBytes(size);
        string msg = Encoding.UTF8.GetString(msgBuffer);
        // Debug.Log($"Received message: {msg}");
        return msg;
    }

    private async Task<byte[]> ReadBytes(int n)
    {
        byte[] buffer = new byte[n];
        int offset = 0;
        while (offset < n)
        {
            offset += await netStream.ReadAsync(buffer, offset, n - offset);
        }
        return buffer;
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
            GameManager.instance.AppendLogLine("TCP client is closed.", ELogTarget.none, ELogtype.errorCli);
            return;
        }
        try
        {
            byte[] msgBuffer = Encoding.UTF8.GetBytes(_message);
            int size = msgBuffer.Length;
            byte[] sizeBuffer = BitConverter.GetBytes(size);
            netStream.Write(sizeBuffer, 0, 4);
            netStream.Write(msgBuffer, 0, size);
        }
        catch (SocketException se)
        {
            GameManager.instance.AppendLogLine("Socket exception: " + se.Message, ELogTarget.none, ELogtype.error);
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
