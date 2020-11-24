using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.RegularExpressions;
using UnityEngine;

public class ApiManager : MonoBehaviour
{
    public struct SRequest
    {
        public string type;
        public string path;
        public string json;
        public string objToUpdate;
    }
    public static ApiManager instance;

    private HttpClient client = new HttpClient();

    [SerializeField] private bool isReady = false;
    [SerializeField] private string server;
    [SerializeField] private string login;
    [SerializeField] private string token;

    [SerializeField] private Queue<SRequest> requestsToSend = new Queue<SRequest>();

    private void Awake()
    {
        if (!instance)
            instance = this;
        else
            Destroy(this);
    }

    private void Update()
    {
        if (isReady && requestsToSend.Count > 0)
        {
            if (requestsToSend.Peek().type == "get")
                GetHttpData();
            else if (requestsToSend.Peek().type == "put")
                PutHttpData();
            else if (requestsToSend.Peek().type == "post")
                PostHttpData();
            // else if (messagesToSend.Peek().type == "delete")
            //     DeleteHttpData();
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
    ///<param name="_request">The request to enqueue</param>
    public void EnqueueRequest(SRequest _request)
    {
        requestsToSend.Enqueue(_request);
    }

    ///<summary>
    /// Send a get request to the api. Create an Ogree object with response.
    ///</summary>
    private async void GetHttpData()
    {
        isReady = false;

        SRequest req = requestsToSend.Dequeue();
        string fullPath = server + req.path;
        try
        {
            string response = await client.GetStringAsync(fullPath);
            GameManager.gm.AppendLogLine(response);
            CreateItemFromJson(req.path, response);
        }
        catch (HttpRequestException e)
        {
            GameManager.gm.AppendLogLine(e.Message, "red");
        }
        
        isReady = true;
    }

    ///<summary>
    /// Send a put request to the api.
    ///</summary>
    private async void PutHttpData()
    {
        isReady = false;

        SRequest req = requestsToSend.Dequeue();
        string fullPath = server + req.path;
        StringContent content = new StringContent(req.json, System.Text.Encoding.UTF8, "application/json");
        try
        {
            HttpResponseMessage response = await client.PutAsync(fullPath, content);
            string responseStr = response.Content.ReadAsStringAsync().Result;
            GameManager.gm.AppendLogLine(responseStr);
        }
        catch (HttpRequestException e)
        {
            GameManager.gm.AppendLogLine(e.Message, "red");
        }
        
        isReady = true;
    }

    ///<summary>
    /// Send a post request to the api. Then, update object's id with response.
    ///</summary>
    private async void PostHttpData()
    {
        isReady = false;

        SRequest req = requestsToSend.Dequeue();
        string fullPath = server + req.path;
        StringContent content = new StringContent(req.json, System.Text.Encoding.UTF8, "application/json");
        try
        {
            HttpResponseMessage response = await client.PostAsync(fullPath, content);
            string responseStr = response.Content.ReadAsStringAsync().Result;
            GameManager.gm.AppendLogLine(responseStr);
            UpdateObjId(req.objToUpdate, responseStr);
        }
        catch (HttpRequestException e)
        {
            GameManager.gm.AppendLogLine(e.Message, "red");
        }
        
        isReady = true;
    }

    ///<summary>
    /// Send a delete request to the api.
    ///</summary>
    private async void DeleteHttpData()
    {
        isReady = false;

        SRequest req = requestsToSend.Dequeue();
        string fullPath = server + req.path;
        try
        {
            HttpResponseMessage response = await client.DeleteAsync(fullPath);
            string responseStr = response.Content.ReadAsStringAsync().Result;
            GameManager.gm.AppendLogLine(responseStr);
        }
        catch (HttpRequestException e)
        {
            GameManager.gm.AppendLogLine(e.Message, "red");
        }

        isReady = true;
    }

    ///<summary>
    /// Create an Ogree item from Json.
    /// Look in request path to the type of object to create
    ///</summary>
    private void CreateItemFromJson(string _path, string _json)
    {
        if (Regex.IsMatch(_path, "customers/[0-9]+$"))
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

    ///<summary>
    /// Update object's id with the id given by the api.
    ///</summary>
    ///<param name="_objName">The name of the object to update</param>
    ///<param name="_jsonId">The json containing the id</param>
    private void UpdateObjId(string _objName, string _jsonId)
    {
        string strId = Regex.Replace(_jsonId, "(.*id\":)|(})", "");
        int id = int.Parse(strId);
        if (GameManager.gm.tenants.ContainsKey(_objName))
            GameManager.gm.tenants[_objName].UpdateId(id);
        else
            GameManager.gm.FindByAbsPath(_objName).GetComponent<AServerItem>().UpdateId(id);
    }

}
