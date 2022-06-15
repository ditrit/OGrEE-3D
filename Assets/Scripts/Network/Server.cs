using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class Server : MonoBehaviour
{
    private enum eConnectionType
    {
        udp,
        tcp
    }

    CliParser parser = new CliParser();

    [Header("Client config")]
    [SerializeField] private eConnectionType protocol;
    private AConnection connection;

    [SerializeField] private int receivePort;
    [SerializeField] private int sendPort;
    public int timer = 0;

    [Header("Debug")]
    [SerializeField] private bool triggerSend = false;
    [SerializeField] private string debugMsg;

    private void Awake()
    {
        if (protocol == eConnectionType.udp)
            connection = new UdpConnection();
        else if (protocol == eConnectionType.tcp)
            connection = new TcpConnection();

        connection.StartConnection(receivePort, sendPort);
    }

    private async void Update()
    {
        // Debug from Editor
        if (triggerSend)
        {
            triggerSend = false;
            connection.Send(debugMsg);
        }

        if (connection.incomingQueue.Count > 0)
        {
            string msg = connection.incomingQueue.Dequeue();
            await Task.Delay(timer);
            GameManager.gm.AppendLogLine(msg);
            await parser.DeserializeInput(msg);
        }
    }

    private void OnDestroy()
    {
        connection.Stop();
    }

    ///<summary>
    /// Send a message to the client.
    ///</summary>
    ///<param name="_msg">The message to send</param>
    public void Send(string _msg)
    {
        connection.Send(_msg + "\n");
    }


}
