using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
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
    private struct SAuthResp
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

    private struct SObjResp
    {
        public string message;
        public string status;
        public SApiObject data;
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

    public bool isInit = false;

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
            if (requestsToSend.Peek().type == "put")
                PutHttpData();
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
    public async void Initialize(string _serverUrl, string _login, string _pwd)
    {
        SAuth auth = new SAuth();
        auth.email = _login;
        auth.password = _pwd;

        StringContent content = new StringContent(JsonUtility.ToJson(auth), System.Text.Encoding.UTF8, "application/json");
        string fullPath = _serverUrl + "/api/user";
        try
        {
            HttpResponseMessage response = await httpClient.PostAsync(fullPath, content);
            string responseStr = response.Content.ReadAsStringAsync().Result;
            // GameManager.gm.AppendLogLine(responseStr);
            // "{\"account\":{\"ID\":641717123263660033,\"CreatedAt\":\"2021-03-16T16:02:04.432625555+01:00\",\"UpdatedAt\":\"2021-03-16T16:02:04.432625555+01:00\",\"DeletedAt\":null,\"Email\":\"iamlegend@gmail.com\",\"Password\":\"\",\"token\":\"eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJVc2VySWQiOjY0MTcxNzEyMzI2MzY2MDAzM30.TfF8sYnWvIS3nr5lncXShDnkRAVirALJxKtFI9P9Y20\"},\"message\":\"Account has been created\",\"status\":true}"
            server = fullPath;

            SAuthResp resp = new SAuthResp();
            resp.account = new SAccount();
            resp = Newtonsoft.Json.JsonConvert.DeserializeObject<SAuthResp>(responseStr);
            Debug.Log(resp.account.token);
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", resp.account.token);

            isReady = true;
            isInit = true;
        }
        catch (HttpRequestException e)
        {
            GameManager.gm.AppendLogLine(e.Message, "red");
        }

    }

    ///<summary>
    /// Initialize the manager with url and token. 
    ///</summary>
    ///<param name="_serverUrl">The base url of the API to use</param>
    ///<param name="_token">The auth token of the API to use</param>
    public async void Initialize(string _serverUrl, string _token)
    {
        if (string.IsNullOrEmpty(_serverUrl))
            GameManager.gm.AppendLogLine("Failed to connect with API: no url", "red");
        else if (string.IsNullOrEmpty(_token))
            GameManager.gm.AppendLogLine("Failed to connect with API: no token", "red");
        else
        {
            server = _serverUrl + "/api/user";
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", _token);
            try
            {
                string response = await httpClient.GetStringAsync($"{_serverUrl}/api/token/valid");
                isReady = true;
                isInit = true;
                GameManager.gm.AppendLogLine("Connected to API", "green");
            }
            catch (HttpRequestException e)
            {
                Debug.Log(e.Message);
                if (e.Message.Contains("403"))
                    GameManager.gm.AppendLogLine("Wrong token", "red");
                else
                    GameManager.gm.AppendLogLine("Wrong api url", "red");
            }
        }
    }

    ///<summary>
    /// Create an PUT request from _input.
    ///</summary>
    ///<param name="_obj">The OgreeObject to put</param>
    public void CreatePutRequest(OgreeObject _obj)
    {
        SRequest request = new SRequest();
        request.type = "put";

        SApiObject apiObj = new SApiObject(_obj);
        request.path = $"/{apiObj.category}s/{apiObj.id}";
        request.json = JsonConvert.SerializeObject(apiObj);
        requestsToSend.Enqueue(request);
    }

    ///<summary>
    /// Create an DELETE request from _input.
    ///</summary>
    ///<param name="_obj">The OgreeObject to delete</param>
    public void CreateDeleteRequest(OgreeObject _obj)
    {
        SRequest request = new SRequest();
        request.type = "delete";
        request.path = $"/{_obj.category}s/{_obj.id}";
        requestsToSend.Enqueue(request);
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
    /// Avoid requestsToSend 
    /// Get an Object from the api. Create an ogreeObject with response.
    ///</summary>
    public async Task GetObject(string _input)
    {
        if (!isInit)
        {
            GameManager.gm.AppendLogLine("Not connected to API", "yellow");
            return;
        }
        string fullPath = $"{server}/{_input}";
        try
        {
            string response = await httpClient.GetStringAsync(fullPath);
            GameManager.gm.AppendLogLine(response);
            if (response.Contains("success"))
                CreateItemFromJson(response);
        }
        catch (HttpRequestException e)
        {
            GameManager.gm.AppendLogLine(e.Message, "red");
        }
    }

    ///<summary>
    /// Avoid requestsToSend 
    /// Post an object to the api. Then, create it from server's response.
    ///</summary>
    public async Task PostObject(SApiObject _obj)
    {
        if (!isInit)
        {
            GameManager.gm.AppendLogLine("Not connected to API", "yellow");
            return;
        }
        string json = JsonConvert.SerializeObject(_obj);
        string fullPath = $"{server}/{_obj.category}s";

        StringContent content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
        try
        {
            HttpResponseMessage response = await httpClient.PostAsync(fullPath, content);
            string responseStr = response.Content.ReadAsStringAsync().Result;
            GameManager.gm.AppendLogLine(responseStr);
            CreateObjFromResp(responseStr);
        }
        catch (HttpRequestException e)
        {
            GameManager.gm.AppendLogLine(e.Message, "red");
        }
    }

    ///<summary>
    /// Create an Ogree item from Json.
    /// Look in request path to the type of object to create
    ///</summary>
    ///<param name="_json">The API response to use</param>
    private void CreateItemFromJson(string _json)
    {
        // Is a list of objects
        if (_json.Contains("\"data\":["))
            return;

        // Is an object: generate the corresponding OGrEE object
        SObjResp resp = JsonConvert.DeserializeObject<SObjResp>(_json);
        switch (resp.data.category)
        {
            case "tenant":
                CustomerGenerator.instance.CreateTenant(resp.data);
                break;
            case "site":
                CustomerGenerator.instance.CreateSite(resp.data);
                break;
            case "building":
                BuildingGenerator.instance.CreateBuilding(resp.data);
                break;
            case "room":
                BuildingGenerator.instance.CreateRoom(resp.data);
                break;
            case "rack":
                ObjectGenerator.instance.CreateRack(resp.data);
                break;
            case "device":
                ObjectGenerator.instance.CreateDevice(resp.data);
                break;
                // case "group":
                //     ObjectGenerator.instance.CreateGroup(resp.data);
                //     break;
                // case "corridor":
                //     ObjectGenerator.instance.CreateCorridor(resp.data);
                //     break;
                // case "separator":
                //     BuildingGenerator.instance.CreateSeparator(resp.data);
                //     break;
        }
    }

    ///<summary>
    /// Update object's id with the id given by the api.
    ///</summary>
    ///<param name="_objName">The name of the object to update</param>
    ///<param name="_json">The json containing the id</param>
    private void UpdateObjId(string _objName, string _json)
    {
        if (_json.Contains("success"))
        {
            _json = Regex.Replace(_json, "\"(tenant|site|building|room|rack|device)\":{", "\"data\":{");
            SObjResp resp = JsonConvert.DeserializeObject<SObjResp>(_json);
            // Debug.Log(resp.data.name + " / " + resp.data.id);
            GameObject obj = GameManager.gm.FindByAbsPath(_objName);
            if (obj)
                obj.GetComponent<OgreeObject>().UpdateId(resp.data.id);
            else
                Debug.LogError($"Can't find {_objName}");
        }
        else
            GameManager.gm.AppendLogLine($"Fail to post {_objName} on server", "yellow");
    }

    ///<summary>
    /// Parse the response and call CreateItemFromJson() to create the item
    ///</summary>
    ///<param name="_json">The API's response to parse</param>
    private void CreateObjFromResp(string _json)
    {
        if (_json.Contains("success"))
        {
            _json = Regex.Replace(_json, "\"(tenant|site|building|room|rack|device)\":{", "\"data\":{");
            SObjResp resp = JsonConvert.DeserializeObject<SObjResp>(_json);
            CreateItemFromJson(_json);
        }
        else
            GameManager.gm.AppendLogLine($"Fail to post on server", "red");
    }

}
