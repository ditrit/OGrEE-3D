using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
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
    #endregion

    private bool threadRunning = false;

    public override void StartConnection(int _receivePort)
    {
        try
        {
            tcpListener = new TcpListener(IPAddress.Any, _receivePort);
        }
        catch (Exception e)
        {
            Debug.Log($"Failed to listen for TCP at port {_receivePort}: {e.Message}");
            return;
        }
        StartReceiveThread();
    }

    private void StartReceiveThread()
    {
        receiveThread = new Thread(new ThreadStart(ListenForMessages));
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
            Debug.Log("Tcp Server is listening");
            while (threadRunning)
            {
                Byte[] bytes = new Byte[1024];
                using (connectedTcpClient = tcpListener.AcceptTcpClient())
                {
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
                            lock (incomingQueue)
                            {
                                incomingQueue.Enqueue(clientMessage);
                                // Debug.Log("=> Client message received: " + clientMessage);
                                // Send("Roger Roger");
                            }
                        }
                    }
                }
            }
        }
        catch (SocketException socketException)
        {
            Debug.Log("SocketException " + socketException.ToString());
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

    /// <summary> 	
    /// Send message to client using socket connection. 	
    /// </summary> 	
    public override void Send(string _message)
    {
        if (connectedTcpClient == null)
            return;

        try
        {
            // Get a stream object for writing. 			
            NetworkStream stream = connectedTcpClient.GetStream();
            if (stream.CanWrite)
            {
                // Convert string message to byte array.                 
                byte[] serverMessageAsByteArray = Encoding.ASCII.GetBytes(_message);
                // Write byte array to socketConnection stream.               
                stream.Write(serverMessageAsByteArray, 0, serverMessageAsByteArray.Length);
                Debug.Log("Server sent his message - should be received by client");
            }
        }
        catch (SocketException socketException)
        {
            Debug.Log("Socket exception: " + socketException);
        }
    }

    public override void Stop()
    {
        threadRunning = false;
        if (receiveThread.IsAlive)
            receiveThread.Abort();
        tcpListener.Stop();
    }
}