using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class TcpConnection : AConnection
{
    #region private members 	
    /// <summary> 	
    /// TCPListener to listen for incomming TCP connection 	
    /// requests. 	
    /// </summary> 	
    private TcpListener tcpListener;
    /// <summary> 
    /// Background thread for TcpServer workload. 	
    /// </summary> 	
    private Thread receiveThread;
    /// <summary> 	
    /// Create handle to connected tcp client. 	
    /// </summary> 	
    private TcpClient connectedTcpClient;

    private string remoteIp;
    private int receivePort;
    private int sendPort;
    #endregion

    private bool threadRunning = false;

    ///
    public override void StartConnection(int _receivePort, int _sendPort)
    {
        receivePort = _receivePort;
        sendPort = _sendPort;
        try
        {
            tcpListener = new TcpListener(IPAddress.Any, _receivePort);
        }
        catch (Exception e)
        {
            GameManager.gm.AppendLogLine($"Failed to listen for TCP at port {receivePort}: {e.Message}", false, eLogtype.error);
            return;
        }
        StartReceiveThread();
    }

    ///
    private void StartReceiveThread()
    {
        receiveThread = new Thread(new ThreadStart(ListenForMessages));
        // receiveThread = new Thread(new ThreadStart(ListenForMessagesLegacy));
        receiveThread.IsBackground = true;
        threadRunning = true;
        receiveThread.Start();
    }

    /// <summary> 	
    /// Runs in background TcpServerThread; Handles incomming TcpClient requests 	
    /// </summary> 	
    private void ListenForMessages()
    {
        try
        {
            tcpListener.Start();
            GameManager.gm.AppendLogLine($"Tcp Server is listening at port {receivePort}", false, eLogtype.info);
            while (threadRunning)
            {
                Byte[] bytes = new Byte[1024];
                using (connectedTcpClient = tcpListener.AcceptTcpClient())
                {
                    // Catch CLI IP to be able to send messages in Send()
                    remoteIp = ((IPEndPoint)connectedTcpClient.Client.RemoteEndPoint).Address.ToString();
                    Debug.Log(remoteIp);

                    string completeMessage = "";
                    // Get a stream object for reading 					
                    using (NetworkStream stream = connectedTcpClient.GetStream())
                    {
                        int length;
                        // Read incomming stream into byte arrary. 						
                        while (stream.DataAvailable)
                        {
                            length = stream.Read(bytes, 0, bytes.Length);
                            string clientMessage = Encoding.ASCII.GetString(bytes, 0, length);
                            Debug.Log("=>" + clientMessage);
                            completeMessage += clientMessage;

                            byte[] msg = Encoding.ASCII.GetBytes("Roger\n");
                            stream.Write(msg, 0, msg.Length);
                        }
                        if (!string.IsNullOrEmpty(completeMessage))
                        {
                            lock (incomingQueue)
                            {
                                incomingQueue.Enqueue(completeMessage);
                                completeMessage = "";
                            }
                        }
                    }
                    connectedTcpClient.Close();
                }
            }
        }
        catch (SocketException socketException)
        {
            GameManager.gm.AppendLogLine("SocketException " + socketException.ToString(), false, eLogtype.error);
        }
    }

    private void ListenForMessagesLegacy()
    {
        try
        {
            tcpListener.Start();
            Debug.Log("Tcp Server is listening");
            while (threadRunning)
            {
                Byte[] bytes = new Byte[1024];
                using (connectedTcpClient = tcpListener.AcceptTcpClient())
                {
                    remoteIp = ((IPEndPoint)connectedTcpClient.Client.RemoteEndPoint).Address.ToString();
                    Debug.Log(remoteIp);
                    string completeMessage = "";
                    // Get a stream object for reading 					
                    using (NetworkStream stream = connectedTcpClient.GetStream())
                    {
                        int length;
                        // Read incomming stream into byte arrary. 						
                        while ((length = stream.Read(bytes, 0, bytes.Length)) != 0)
                        {
                            var incommingData = new byte[length];
                            Array.Copy(bytes, 0, incommingData, 0, length);
                            // Convert byte array to string message. 							
                            string clientMessage = Encoding.ASCII.GetString(incommingData);
                            Debug.Log("=>" + clientMessage);
                            completeMessage += clientMessage;

                            byte[] msg = Encoding.ASCII.GetBytes("Roger\n");
                            stream.Write(msg, 0, msg.Length);
                            Debug.Log("After sending");
                            // stream.Close();
                            if (completeMessage[completeMessage.Length - 1] == '}')
                            {
                                Debug.Log("Entering if()");
                                lock (incomingQueue)
                                {
                                    incomingQueue.Enqueue(completeMessage);
                                    completeMessage = "";
                                }
                            }
                        }
                    }
                }
            }
        }
        catch (SocketException socketException)
        {
            GameManager.gm.AppendLogLine("SocketException " + socketException.ToString(), false, eLogtype.errorCli);
        }
    }

    ///
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

    /// <summary> 	
    /// Send message to client using socket connection. 	
    /// </summary> 	
    public override async void Send(string _message)
    {
        if (connectedTcpClient == null)
            return;

        try
        {
            // remoteIp = "192.168.254.23";
            // remoteIp = "192.168.1.28"; // Hack for sending msg to JS TCP listener
            Debug.Log($"Send msg to {remoteIp}:{sendPort}");
            TcpClient client = new TcpClient(remoteIp, sendPort);

            // Get a stream object for writing. 			
            NetworkStream stream = client.GetStream();
            if (stream.CanWrite)
            {
                // Convert string message to byte array.                 
                byte[] serverMessageAsByteArray = Encoding.ASCII.GetBytes(_message);
                // Write byte array to socketConnection stream.               
                stream.Write(serverMessageAsByteArray, 0, serverMessageAsByteArray.Length);
                await Task.Delay(1000);
                stream.Close();
            }
            client.Close();
        }
        catch (SocketException socketException)
        {
            GameManager.gm.AppendLogLine("Socket exception: " + socketException, false, eLogtype.error);
        }
    }

    ///
    public override void Stop()
    {
        threadRunning = false;
        if (receiveThread.IsAlive)
            receiveThread.Abort();
        tcpListener.Stop();
    }
}