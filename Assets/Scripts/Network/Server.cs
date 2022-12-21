using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class Server : MonoBehaviour
{
    private enum EConnectionType
    {
        udp,
        tcp
    }

    readonly CliParser parser = new CliParser();

    [Header("Client config")]
    [SerializeField] private EConnectionType protocol;
    private AConnection connection;

    [SerializeField] private int cliPort;
    public int timer = 0;

    [Header("Debug")]
    [SerializeField] private bool triggerSend = false;
    [SerializeField] private string debugMsg;

    private Coroutine dequeueCoroutine;

    private void Update()
    {
        // Debug from Editor
        if (triggerSend)
        {
            triggerSend = false;
            connection.Send(debugMsg);
        }

        if (connection.incomingQueue.Count > 0 && dequeueCoroutine == null)
            dequeueCoroutine = StartCoroutine(DequeueAndParse());
    }

    private void OnDestroy()
    {
        connection.Stop();
    }

    ///<summary>
    /// Dequeue connection.incomingQueue and call parser.DeserializeInput() to execute received command from CLI
    ///</summary>
    private IEnumerator DequeueAndParse()
    {
        string msg = connection.incomingQueue.Dequeue();
        yield return new WaitForSeconds(timer);
        GameManager.instance.AppendLogLine(msg, false, ELogtype.infoCli);
        Task parse = parser.DeserializeInput(msg);
        yield return new WaitUntil(() => parse.IsCompleted);
        dequeueCoroutine = null;
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
        if (protocol == EConnectionType.udp)
            connection = new UdpConnection();
        else if (protocol == EConnectionType.tcp)
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
