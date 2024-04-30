using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;

public class Server : MonoBehaviour
{
    private enum EConnectionType
    {
        udp,
        tcp
    }

    private readonly CommandParser parser = new();

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
        if (connection == null)
            return;

        // Debug from Editor
        if (triggerSend)
        {
            triggerSend = false;
            connection.Send(debugMsg);
        }

        if (connection.incomingQueue.Count > 0 && dequeueCoroutine == null)
            dequeueCoroutine = StartCoroutine(DequeueAndParse());
        while (connection.mainThreadQueue.Count > 0)
            connection.mainThreadQueue.Dequeue().Invoke();
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
        GameManager.instance.AppendLogLine(msg, ELogTarget.none, ELogtype.infoCli);
        Task parse = parser.DeserializeInput(msg);
        yield return new WaitUntil(() => parse.IsCompleted);
        dequeueCoroutine = null;
    }


    ///<summary>
    /// Set values for listenPort and sendPort.
    ///</summary>
    ///<param name="_cliPort">The value to set for listenPort</param>
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
#if SERVER
        connection.StartConnection(cliPort);
#elif WEB_DEMO
        StartCoroutine(LoadDemo());
#endif
    }

    ///<summary>
    /// Send a message to the client.
    ///</summary>
    ///<param name="_msg">The message to send</param>
    public void Send(string _msg)
    {
        connection.Send(_msg);
    }

    /// <summary>
    /// Stop <see cref="connection"/> and start a new one.
    /// </summary>
    public void ResetConnection()
    {
        connection.Stop();
        StartServer();
    }

    /// <summary>
    /// Using Demo_Credentials to login into given API. Then, use Demo_LoadObjects for loading objects
    /// </summary>
    /// <returns></returns>
    private IEnumerator LoadDemo()
    {
        Debug.Log("Connecting to API...");
        TextAsset credentials = Resources.Load<TextAsset>("Demo_Credentials");
        Dictionary<string, string> creds = JsonConvert.DeserializeObject<Dictionary<string, string>>(credentials.ToString());

        Task register = ApiManager.instance.RegisterApi(creds["url"], creds["email"], creds["password"]);
        yield return new WaitUntil(() => register.IsCompleted);
        Task login = ApiManager.instance.Initialize();
        yield return new WaitUntil(() => login.IsCompleted);

        Debug.Log("Drawing objects...");
        TextAsset loadObjectCmd = Resources.Load<TextAsset>("Demo_LoadObjects");
        Task parseDraw = parser.DeserializeInput(loadObjectCmd.ToString());
        yield return new WaitUntil(() => parseDraw.IsCompleted);
    }

}
