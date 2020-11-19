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
            // else if (messagesToSend.Peek().type == "delete")
            //     StartCoroutine(DeleteData());
        }
    }

    ///<summary>
    /// Initialiaze the manager with server, login and token.
    ///</summary>
    ///<param name="_serverUrl">The url to save</param>
    ///<param name="_login">The login to save</param>
    ///<param name="_token">The token to save</param>
    public void Initialize(string _serverUrl, string _login, string _token)
    {
        server = _serverUrl;
        login = _login;
        token = _token;
        isReady = true;
    }

    ///<summary>
    /// Enqueue a request to for the api.
    ///</summary>
    ///<param name="_type">The type of request</param>
    ///<param name="_path">The relative path of the request</param>
    public void EnqueueMessage(string _type, string _path)
    {
        messagesToSend.Enqueue(new SRequest(_type, _path));
    }

    ///<summary>
    /// Enqueue a request to for the api.
    ///</summary>
    ///<param name="_type">The type of request</param>
    ///<param name="_path">The relative path of the request</param>
    ///<param name="_json">The json to send</param>
    public void EnqueueMessage(string _type, string _path, string _json)
    {
        messagesToSend.Enqueue(new SRequest(_type, _path, _json));
    }

    ///<summary>
    /// Send a get request to the api. Create an Ogree object with response.
    ///</summary>
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
            isReady = true;
            yield break;
        }
        GameManager.gm.AppendLogLine(www.downloadHandler.text);
        CreateItemFromJson(req.path, www.downloadHandler.text);

        isReady = true;
    }

    ///<summary>
    /// Send a put request to the api.
    ///</summary>
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

    ///<summary>
    /// Create an Ogree item from Json.
    /// Look in request path to the type of object to create
    ///</summary>
    private void CreateItemFromJson(string _path, string _json)
    {
        if (Regex.IsMatch(_path, "customers/[0-9]+"))
        {
            Debug.Log("Create Customer");
            SCuFromJson cu = JsonUtility.FromJson<SCuFromJson>(_json);
            CustomerGenerator.instance.CreateCustomer(cu);
        }
        else if (Regex.IsMatch(_path, "sites/[0-9]+"))
        {
            Debug.Log("Create Datacenter (site)");
            SDcFromJson dc = JsonUtility.FromJson<SDcFromJson>(_json);
            dc.id = int.Parse(_path.Substring(_path.IndexOf('/') + 1));
            CustomerGenerator.instance.CreateDatacenter(dc);
        }
    }

}
