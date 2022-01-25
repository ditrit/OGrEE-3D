using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class Server : MonoBehaviour
{
    private enum eConnectionType
    {
        udp,
        tcp
    }

    [Header("Client config")]
    [SerializeField] private eConnectionType protocol;
    private UdpConnection udpConnection;
    private TcpConnection tcpConnection;

    [SerializeField] private int receivePort;
    // private string sendIP; // 192.168.1.28
    // private int sendPort; // 5600 ?

    [Header("Debug")]
    [SerializeField] private bool triggerSend = false;
    [SerializeField] private string msg;

    private void Start()
    {
        if (protocol == eConnectionType.udp)
        {
            udpConnection = new UdpConnection();
            udpConnection.StartConnection(/*sendIP, sendPort, */receivePort);
        }
        else if (protocol == eConnectionType.tcp)
        {
            tcpConnection = new TcpConnection();
            tcpConnection.StartConnection(receivePort);
        }
    }

    private void Update()
    {
        // Debug from Editor
        if (triggerSend)
        {
            triggerSend = false;

            if (protocol == eConnectionType.udp)
                udpConnection.Send(msg);
            else if (protocol == eConnectionType.tcp)
                tcpConnection.Send(msg);
        }

        if (tcpConnection.incomingQueue.Count > 0)
        {
            string msg = tcpConnection.incomingQueue.Dequeue();
            GameManager.gm.AppendLogLine(msg);
        }
    }

    private void OnDestroy()
    {
        if (protocol == eConnectionType.udp)
            udpConnection.Stop();
        else if (protocol == eConnectionType.tcp)
            tcpConnection.Stop();
    }

}
