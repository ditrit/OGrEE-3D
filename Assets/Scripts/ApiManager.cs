using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class ApiManager : MonoBehaviour
{
    private struct SRequest
    {
        public string type;
        public string msg;

        public SRequest(string _type, string _message)
        {
            type = _type;
            msg = _message;
        }
    }
    public static ApiManager instance;

    [SerializeField] private bool isReady = false;
    [SerializeField] private string server;
    [SerializeField] private string login;
    [SerializeField] private string token;

    [SerializeField] private Queue<SRequest> messagesToSend = new Queue<SRequest>();

    private void Awake()
    {
        if (!instance)
            instance = this;
        else
            Destroy(this);
    }

    private void Update()
    {
        if (isReady && messagesToSend.Count > 0)
            StartCoroutine(GetData());
    }

    ///
    public void Initialize(string _serverUrl, string _login, string _token)
    {
        server = _serverUrl;
        login = _login;
        token = _token;
        isReady = true;
    }

    ///
    public void EnqueueMessage(string _type, string _msg)
    {
        messagesToSend.Enqueue(new SRequest(_type, _msg));
    }

    ///
    private IEnumerator GetData()
    {
        isReady = false;

        string response;
        string request = server + messagesToSend.Dequeue().msg;
        Debug.LogWarning(request);

        UnityWebRequest www = UnityWebRequest.Get(request);
        www.downloadHandler = new DownloadHandlerBuffer();

        yield return www.SendWebRequest();
        if (www.isHttpError || www.isNetworkError)
            response = $"Error while connecting to API: {www.error}";
        else
            response = www.downloadHandler.text;

        GameManager.gm.AppendLogLine(response);
        isReady = true;
    }

}
