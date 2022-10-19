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

    [SerializeField] private int cliPort;
    public int timer = 0;

    [Header("Debug")]
    [SerializeField] private bool triggerSend = false;
    [SerializeField] private string debugMsg;

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
            GameManager.gm.AppendLogLine(msg, false, eLogtype.infoCli);
            await parser.DeserializeInput(msg);
        }
    }

    private void OnDestroy()
    {
        connection.Stop();
    }

    ///<summary>
    /// Set values for listenPort and sendPort.
    ///</summary>
    ///<param name="_cliPort">The value to set for listenPort</param>
    ///<param name="_sendPort">The value to set for sendPort</param>
    public void SetupPorts(int _cliPort)
    {
        cliPort = _cliPort;
    }

    ///<summary>
    /// Initialize a connection and start it.
    ///</summary>
    public void StartServer()
    {
        if (protocol == eConnectionType.udp)
            connection = new UdpConnection();
        else if (protocol == eConnectionType.tcp)
            connection = new TcpConnection();

        connection.StartConnection(cliPort);
    }

    ///<summary>
    /// Send a message to the client.
    ///</summary>
    ///<param name="_msg">The message to send</param>
    public void Send(string _msg)
    {
        connection.Send(_msg);
    }

}
