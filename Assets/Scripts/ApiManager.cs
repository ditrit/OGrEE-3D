using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;
using Newtonsoft.Json.Utilities;
using UnityEngine.UI;
using TMPro;
using System;



public class ApiManager : MonoBehaviour
{
    public struct SRequest
    {
        public string type;
        public string path;
        public string json;
    }

    private struct SObjRespSingle
    {
        public string message;
        public string status;
        public SApiObject data;
    }
    private struct SObjRespArray
    {
        public string message;
        public string status;
        public SObjectArray data;
    }
    private struct SObjectArray
    {
        public SApiObject[] objects;
    }

    private struct STemplateResp
    {
        public string message;
        public string status;
        public ReadFromJson.STemplate data;
    }

    private struct SRoomResp
    {
        public string message;
        public string status;
        public ReadFromJson.SRoomFromJson data;
    }

    public static ApiManager instance;

    private HttpClient httpClient = new HttpClient();

    public bool isInit = false;

    [SerializeField] private bool isReady = false;
    [SerializeField] private string server;

    [SerializeField] private Queue<SRequest> requestsToSend = new Queue<SRequest>();

    private ReadFromJson rfJson = new ReadFromJson();

    private void Awake()
    {
        AotHelper.EnsureList<ReadFromJson.STemplateChild>();
        AotHelper.EnsureList<int>();
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
            else if (requestsToSend.Peek().type == "delete")
                DeleteHttpData();
        }
    }

    ///<summary>
    /// Initialize the manager with url and token. 
    ///</summary>
    ///<param name="_serverUrl">The base url of the API to use</param>
    ///<param name="_token">The auth token of the API to use</param>
    public async Task Initialize(string _serverUrl, string _token)
    {
        if (string.IsNullOrEmpty(_serverUrl))
            GameManager.gm.AppendLogLine("Failed to connect with API: no url", "red");
        else if (string.IsNullOrEmpty(_token))
            GameManager.gm.AppendLogLine("Failed to connect with API: no token", "red");
        else
        {
            server = _serverUrl + "/api";
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
                GameManager.gm.AppendLogLine($"Error while connecting to API: {e.Message}", "red");
            }
        }
    }

    ///<summary>
    /// Create a PUT request from _input.
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
    /// Create a GET request.
    ///</summary>
    ///<param name="_obj">The OgreeObject to put</param>
    public void CreateGetRequest(string _tenants)
    {
        SRequest request = new SRequest();
        request.type = "get";

        request.path = $"/tenants/{_tenants}/sites";
        request.json = "";
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
    /// Send a put request to the api.
    ///</summary>
    public async void GetHttpData()
    {
        isReady = false;

        SRequest req = requestsToSend.Dequeue();
        string fullPath = server + req.path;
        try
        {
            HttpResponseMessage response = await httpClient.GetAsync(fullPath);
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
    /// Get an Object from the api.
    ///</summary>
    ///<param name="_input">The endpoint to add to the base server address for API GET request</param>
    ///<returns>A list containing all the objects in the json</returns>
    public async Task<List<SApiObject>> GetObjectVincent(string _input)
    {
        if (!isInit)
        {
            GameManager.gm.AppendLogLine("Not connected to API", "yellow");
            return null;
        }
        else
        {
            string fullPath = $"{server}/{_input}";
            Debug.Log($"fullpath is {fullPath}");
            try
            {
                HttpResponseMessage responseHTTP = await httpClient.GetAsync(fullPath);
                string response = responseHTTP.Content.ReadAsStringAsync().Result;
                GameManager.gm.AppendLogLine(response);
                Debug.Log(response);
                if (response.Contains("successfully got query for object") || response.Contains("successfully got object"))
                {
                    List<SApiObject> physicalObjects = CreateListFromJsonVincent(response);
                    return physicalObjects;
                }
                else
                    GameManager.gm.AppendLogLine("Unknown object received", "red");
                return null;
            }
            catch (HttpRequestException e)
            {
                GameManager.gm.AppendLogLine(e.Message, "red");
                return null;
            }
        }
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

    /*///<summary>
    /// Avoid requestsToSend 
    /// Get an Object from the api. Create an ogreeObject with response.
    ///</summary>
    ///<param name="_input">The path to add a base server for API GET request</param>
    public async Task GetObject(string _input)
    {
        if (!isInit)
        {
            GameManager.gm.AppendLogLine("Not connected to API", "yellow");
            return;
        }
        EventManager.Instance.Raise(new ChangeCursorEvent() { type = CursorChanger.CursorType.Loading });

        string fullPath = $"{server}/{_input}";
        try
        {
            string response = await httpClient.GetStringAsync(fullPath);
            GameManager.gm.AppendLogLine(response);
            if (response.Contains("successfully got query for object") || response.Contains("successfully got object"))
                await CreateItemFromJson(response);
            else if (response.Contains("successfully got obj_template"))
                await CreateTemplateFromJson(response, "obj");
            else if (response.Contains("successfully got room_template"))
                await CreateTemplateFromJson(response, "room");
            else
                GameManager.gm.AppendLogLine("Unknown object received", "red");
        }
        catch (HttpRequestException e)
        {
            GameManager.gm.AppendLogLine(e.Message, "red");
            EventManager.Instance.Raise(new ChangeCursorEvent() { type = CursorChanger.CursorType.Loading });
        }
    }*/

    ///<summary>
    /// Avoid requestsToSend 
    /// Get an Object from the api. Create an ogreeObject with response.
    ///</summary>
    ///<param name="_input">The path to add a base server for API GET request</param>
    ///<param name="_callback">Function to call to use GET response</param>
    public async Task<T> GetObject<T>(string _input, Func<string, Task<T>> _callback)
    {
        if (!isInit)
        {
            GameManager.gm.AppendLogLine("Not connected to API", "yellow");
            return default;
        }
        EventManager.Instance.Raise(new ChangeCursorEvent() { type = CursorChanger.CursorType.Loading });

        string fullPath = $"{server}/{_input}";
        try
        {
            string response = await httpClient.GetStringAsync(fullPath);
            GameManager.gm.AppendLogLine(response);
            return await _callback(response);
        }
        catch (HttpRequestException e)
        {
            GameManager.gm.AppendLogLine(e.Message, "red");
            EventManager.Instance.Raise(new ChangeCursorEvent() { type = CursorChanger.CursorType.Loading });
            return default;
        }
    }
    public async Task GetObject(string _input, Func<string, Task> _callback)
    {
        if (!isInit)
        {
            GameManager.gm.AppendLogLine("Not connected to API", "yellow");
            return;
        }
        EventManager.Instance.Raise(new ChangeCursorEvent() { type = CursorChanger.CursorType.Loading });

        string fullPath = $"{server}/{_input}";
        try
        {
            string response = await httpClient.GetStringAsync(fullPath);
            GameManager.gm.AppendLogLine(response);
            await _callback(response);
        }
        catch (HttpRequestException e)
        {
            GameManager.gm.AppendLogLine(e.Message, "red");
            EventManager.Instance.Raise(new ChangeCursorEvent() { type = CursorChanger.CursorType.Loading });
        }
    }

    ///<summary>
    /// Avoid requestsToSend 
    /// Create an ogreeObject with response.
    ///</summary>
    ///<param name="_response">The response for API GET request</param>
    public async Task DrawObjects(string _response)
    {
        GameManager.gm.AppendLogLine(_response);
        if (_response.Contains("successfully got query for object") || _response.Contains("successfully got object"))
            await CreateItemFromJson(_response);
        else if (_response.Contains("successfully got obj_template"))
            await CreateTemplateFromJson(_response, "obj");
        else if (_response.Contains("successfully got room_template"))
            await CreateTemplateFromJson(_response, "room");
        else
            GameManager.gm.AppendLogLine("Unknown object received", "red");
    }

    ///<summary>
    /// Avoid requestsToSend 
    /// Get an Object from the api. Create an ogreeObject with response.
    ///</summary>
    ///<param name="_response">The response for API GET request</param>
    ///<returns>A string containing the parent id of the first object created by the _response</returns>
    public async Task<string> GetParentId(string _response)
    {
        List<SApiObject> physicalObjects = new List<SApiObject>();
        List<SApiObject> logicalObjects = new List<SApiObject>();

        if (Regex.IsMatch(_response, "\"data\":{\"objects\":\\["))
        {
            SObjRespArray resp = JsonConvert.DeserializeObject<SObjRespArray>(_response);
            foreach (SApiObject obj in resp.data.objects)
                physicalObjects.Add(obj);
        }
        else
        {
            SObjRespSingle resp = JsonConvert.DeserializeObject<SObjRespSingle>(_response);
            Utils.ParseNestedObjects(physicalObjects, logicalObjects, resp.data);
        }

        foreach (SApiObject obj in physicalObjects)
        {
            return obj.parentId;
        }
        return "error";
    }
    
    ///<summary>
    /// Avoid requestsToSend 
    /// Get an Object from the api. Create an ogreeObject with response.
    ///</summary>
    ///<param name="_response">The response for API GET request</param>
    ///<returns>A string containing the name of the first object created by the _response</returns>
    public async Task<string> GetName(string _response)
    {

        List<SApiObject> physicalObjects = new List<SApiObject>();
        List<SApiObject> logicalObjects = new List<SApiObject>();

        if (Regex.IsMatch(_response, "\"data\":{\"objects\":\\["))
        {
            SObjRespArray resp = JsonConvert.DeserializeObject<SObjRespArray>(_response);
            foreach (SApiObject obj in resp.data.objects)
                physicalObjects.Add(obj);
        }
        else
        {
            SObjRespSingle resp = JsonConvert.DeserializeObject<SObjRespSingle>(_response);
            Utils.ParseNestedObjects(physicalObjects, logicalObjects, resp.data);
        }

        foreach (SApiObject obj in physicalObjects)
        {
            return obj.name;
        }
        return "error";
    }

    ///<summary>
    /// Avoid requestsToSend 
    /// Post an object to the api. Then, create it from server's response.
    ///</summary>
    ///<param name="_obj">The SApiObject to post</param>
    public async Task PostObject(SApiObject _obj)
    {
        if (!isInit)
        {
            GameManager.gm.AppendLogLine("Not connected to API", "yellow");
            return;
        }
        string json = JsonConvert.SerializeObject(_obj);
        // Debug.Log(json);
        string fullPath = $"{server}/{_obj.category}s";

        StringContent content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
        try
        {
            HttpResponseMessage response = await httpClient.PostAsync(fullPath, content);
            string responseStr = response.Content.ReadAsStringAsync().Result;
            GameManager.gm.AppendLogLine(responseStr);

            if (responseStr.Contains("success"))
                await CreateItemFromJson(responseStr);
            else
                GameManager.gm.AppendLogLine($"Fail to post on server", "red");
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
    ///<param name="_json">The serialized STemplate to post</param>
    ///<param name="_type">obj or room</param>
    public async Task PostTemplateObject(string _json, string _type)
    {
        if (!isInit)
        {
            GameManager.gm.AppendLogLine("Not connected to API", "yellow");
            return;
        }
        Debug.Log(_json);
        string fullPath = $"{server}/{_type}-templates";

        StringContent content = new StringContent(_json, System.Text.Encoding.UTF8, "application/json");
        try
        {
            HttpResponseMessage response = await httpClient.PostAsync(fullPath, content);
            string responseStr = response.Content.ReadAsStringAsync().Result;
            GameManager.gm.AppendLogLine(responseStr);

            if (responseStr.Contains("success"))
                await CreateTemplateFromJson(responseStr, _type);
            else
                GameManager.gm.AppendLogLine($"Fail to post on server", "red");
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
    private async Task CreateItemFromJson(string _json)
    {
        List<SApiObject> physicalObjects = new List<SApiObject>();
        List<SApiObject> logicalObjects = new List<SApiObject>();

        if (Regex.IsMatch(_json, "\"data\":{\"objects\":\\["))
        {
            SObjRespArray resp = JsonConvert.DeserializeObject<SObjRespArray>(_json);
            foreach (SApiObject obj in resp.data.objects)
                physicalObjects.Add(obj);
        }
        else
        {
            SObjRespSingle resp = JsonConvert.DeserializeObject<SObjRespSingle>(_json);
            Utils.ParseNestedObjects(physicalObjects, logicalObjects, resp.data);
        }

        foreach (SApiObject obj in physicalObjects)
            await OgreeGenerator.instance.CreateItemFromSApiObject(obj);

        foreach (SApiObject obj in logicalObjects)
            await OgreeGenerator.instance.CreateItemFromSApiObject(obj);

        GameManager.gm.AppendLogLine($"{physicalObjects.Count + logicalObjects.Count} object(s) created", "green");
    }

    ///<summary>
    /// Look in request path to the type of object to create a 3D list with the response.
    ///</summary>
    ///<param name="_json">The API response to use</param>
    ///<returns>A list containing all the objects in the json</returns>
    private List<SApiObject> CreateListFromJsonVincent(string _json)
    {
        List<SApiObject> physicalObjects = new List<SApiObject>();

        if (Regex.IsMatch(_json, "\"data\":{\"objects\":\\["))
        {
            SObjRespArray resp = JsonConvert.DeserializeObject<SObjRespArray>(_json);
            foreach (SApiObject obj in resp.data.objects)
                physicalObjects.Add(obj);
        }
        return physicalObjects;
    }

    ///<summary>
    /// Use the given template json to instantiate an object template.
    ///</summary>
    ///<param name="_json">The json given by the API</param>
    private async Task CreateTemplateFromJson(string _json, string _type)
    {
        if (_type == "obj")
        {
            STemplateResp resp = JsonConvert.DeserializeObject<STemplateResp>(_json);
            await rfJson.CreateObjectTemplate(resp.data);
        }
        else if (_type == "room")
        {
            SRoomResp resp = JsonConvert.DeserializeObject<SRoomResp>(_json);
            rfJson.CreateRoomTemplate(resp.data);
        }
        EventManager.Instance.Raise(new ChangeCursorEvent() { type = CursorChanger.CursorType.Loading });
    }
}
