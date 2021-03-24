using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
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
    struct SAuth
    {
        public string email;
        public string password;
    }
    private struct SResp
    {
        public SAccount account;
        public string message;
        public bool status;
    }
    private struct SAccount
    {
        public string ID;
        public string CreatedAt;
        public string UpdatedAt;
        public string DeletedAt;
        public string Email;
        public string Password;
        public string token;
    }

    // {
    //     "account":{
    //         "ID":641717123263660033,
    //         "CreatedAt":"2021-03-16T16:02:04.432625555+01:00",
    //         "UpdatedAt":"2021-03-16T16:02:04.432625555+01:00",
    //         "DeletedAt":null,
    //         "Email":"iamlegend@gmail.com",
    //         "Password":"",
    //         "token":"eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJVc2VySWQiOjY0MTcxNzEyMzI2MzY2MDAzM30.TfF8sYnWvIS3nr5lncXShDnkRAVirALJxKtFI9P9Y20"
    //     },
    //     "message":"Account has been created",
    //     "status":true
    // }
    public static ApiManager instance;

    private HttpClient httpClient = new HttpClient();

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
            else if (requestsToSend.Peek().type == "delete")
                DeleteHttpData();
        }
    }

    ///<summary>
    /// Initialiaze the manager with server, login and token.
    ///</summary>
    ///<param name="_serverUrl">The url to save</param>
    ///<param name="_login">The login to use</param>
    ///<param name="_pwd">The password to use</param>
    public /*async*/ void Initialize(string _serverUrl, string _login, string _pwd)
    {
        SAuth auth = new SAuth();
        auth.email = _login;
        auth.password = _pwd;

        StringContent content = new StringContent(JsonUtility.ToJson(auth), System.Text.Encoding.UTF8, "application/json");
        string fullPath = _serverUrl + "/api/user";
        try
        {
            // HttpResponseMessage response = await httpClient.PostAsync(fullPath, content);
            // string responseStr = response.Content.ReadAsStringAsync().Result;
            // GameManager.gm.AppendLogLine(responseStr);
            string responseStr = "{\"account\":{\"ID\":641717123263660033,\"CreatedAt\":\"2021-03-16T16:02:04.432625555+01:00\",\"UpdatedAt\":\"2021-03-16T16:02:04.432625555+01:00\",\"DeletedAt\":null,\"Email\":\"iamlegend@gmail.com\",\"Password\":\"\",\"token\":\"eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJVc2VySWQiOjY0MTcxNzEyMzI2MzY2MDAzM30.TfF8sYnWvIS3nr5lncXShDnkRAVirALJxKtFI9P9Y20\"},\"message\":\"Account has been created\",\"status\":true}";
            server = fullPath;

            SResp resp = new SResp();
            resp.account = new SAccount();
            resp = Newtonsoft.Json.JsonConvert.DeserializeObject<SResp>(responseStr);
            Debug.Log(resp.account.token);
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", resp.account.token);

            isReady = true;
        }
        catch (HttpRequestException e)
        {
            GameManager.gm.AppendLogLine(e.Message, "red");
        }

    }

    ///<summary>
    /// Create an GET request from _input.
    ///</summary>
    ///<param name="_input">The get request to send</param>
    public void CreateGetRequest(string _input)
    {
        SRequest request = new SRequest();
        request.type = "get";
        request.path = $"/{_input}";

        requestsToSend.Enqueue(request);
    }

    ///<summary>
    /// Create an PUT request from _input.
    ///</summary>
    ///<param name="_objName">The hierarchy name of the object to put</param>
    public void CreatePutRequest(OgreeObject _obj)
    {
        SRequest request = new SRequest();
        request.type = "put";

        SApiObject apiObj = Utils.ConvertToApiObj(_obj);
        request.path = $"/{apiObj.category}s/{apiObj.id}";
        request.json = JsonConvert.SerializeObject(apiObj);
        requestsToSend.Enqueue(request);
    }

    ///<summary>
    /// Create an POST request from _input.
    ///</summary>
    ///<param name="_objName">The hierarchy name of the object to post</param>
    public void CreatePostRequest(OgreeObject _obj)
    {
        SRequest request = new SRequest();
        request.type = "post";

        SApiObject apiObj = Utils.ConvertToApiObj(_obj);
        request.path = $"/{apiObj.category}s";
        /*
        int pointCount = _objName.Count(f => (f == '.'));
        if (pointCount == 0)
            request.path = "/tenants";
        else if (pointCount == 1)
            request.path = "/sites";
        else if (pointCount == 2)
            request.path = "/buildings"; //?
        else if (pointCount == 3)
            request.path = "/rooms"; //?
        else if (pointCount == 4)
            request.path = "/racks"; //?
        else
            request.path = "/devices"; //?
        */
        request.json = JsonConvert.SerializeObject(apiObj);
        request.objToUpdate = _obj.name;

        requestsToSend.Enqueue(request);
    }

    ///<summary>
    /// Create an DELETE request from _input.
    ///</summary>
    ///<param name="_objName">The hierarchy name of the object to delete</param>
    public void CreateDeleteRequest(OgreeObject _obj)
    {
        SRequest request = new SRequest();
        request.type = "delete";
        request.path = $"/{_obj.category}s/{_obj.id}";
        requestsToSend.Enqueue(request);
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
            string response = await httpClient.GetStringAsync(fullPath);
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
            HttpResponseMessage response = await httpClient.PutAsync(fullPath, content);
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
            HttpResponseMessage response = await httpClient.PostAsync(fullPath, content);
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
            HttpResponseMessage response = await httpClient.DeleteAsync(fullPath);
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
        if (_json.StartsWith("{\"data\":["))
            return;
        // regex replace tenant/site/etc en OgreeObj
        // faire une structure message,status,OgreeObj
        _json = Regex.Replace(_json, "{\"data\":|,\"message.*", "");
        SApiObject apiObj = JsonConvert.DeserializeObject<SApiObject>(_json);
        switch (apiObj.category)
        {
            case "tenant":
                Debug.Log("Create Customer");
                CustomerGenerator.instance.CreateTenant(apiObj, false);
                break;
            case "site":
                Debug.Log("Create Site");
                CustomerGenerator.instance.CreateSite(apiObj, null, false);
                break;
            case "building":
                BuildingGenerator.instance.CreateBuilding(apiObj, null, false);
                break;
            case "room":
                BuildingGenerator.instance.CreateRoom(apiObj, null, false);
                break;
        }
    }

    ///<summary>
    /// Update object's id with the id given by the api.
    ///</summary>
    ///<param name="_objName">The name of the object to update</param>
    ///<param name="_jsonResp">The json containing the id</param>
    private void UpdateObjId(string _objName, string _jsonResp)
    {
        if (_jsonResp.Contains("success"))
        {
            _jsonResp = Regex.Replace(_jsonResp, ".*\"(tenant|site)\":|}$", "");
            SApiObject respObj = JsonConvert.DeserializeObject<SApiObject>(_jsonResp);
            GameManager.gm.FindByAbsPath(_objName).GetComponent<OgreeObject>().UpdateId(respObj.id);
        }
        else
            GameManager.gm.AppendLogLine($"Fail to post {_objName} on server", "yellow");
    }

}
