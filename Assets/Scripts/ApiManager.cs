using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Networking;

public class ApiManager : MonoBehaviour
{
    private struct SRequest
    {
        public string type;
        public string path;
        public string json;

        public SRequest(string _type, string _path)
        {
            type = _type;
            path = _path;
            json = null;
        }
        public SRequest(string _type, string _path, string _json)
        {
            type = _type;
            path = _path;
            json = _json;
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
        {
            if (messagesToSend.Peek().type == "get")
                StartCoroutine(GetData());
            else if (messagesToSend.Peek().type == "put")
                StartCoroutine(PutData());
                // delete
        }
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
    public void EnqueueMessage(string _type, string _path)
    {
        messagesToSend.Enqueue(new SRequest(_type, _path));
    }
    public void EnqueueMessage(string _type, string _path, string _json)
    {
        messagesToSend.Enqueue(new SRequest(_type, _path, _json));
    }

    ///
    private IEnumerator GetData()
    {
        isReady = false;

        SRequest req = messagesToSend.Dequeue();
        string fullPath = server + req.path;

        UnityWebRequest www = UnityWebRequest.Get(fullPath);

        yield return www.SendWebRequest();
        if (www.isHttpError || www.isNetworkError)
        {
            GameManager.gm.AppendLogLine(www.error, "red");
            isReady = false;
            yield break;
        }
        GameManager.gm.AppendLogLine(www.downloadHandler.text);

        isReady = true;
    }

    private IEnumerator PutData()
    {
        isReady = false;

        SRequest req = messagesToSend.Dequeue();
        string fullPath = server + req.path;

        UnityWebRequest www = UnityWebRequest.Put(fullPath, req.json);
        yield return www.SendWebRequest();

        if (www.isHttpError || www.isNetworkError)
            GameManager.gm.AppendLogLine(www.error, "red");
        else
            GameManager.gm.AppendLogLine(www.downloadHandler.text);
        
        isReady = true;
    }

    #region CreateMethods

    #endregion

}
