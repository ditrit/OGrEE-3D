using Newtonsoft.Json;
using System;
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
    /// Initialize the manager with url and token. 
    ///</summary>
    ///<param name="_serverUrl">The base url of the API to use</param>
    ///<param name="_token">The auth token of the API to use</param>
    public async Task Initialize(string _serverUrl, string _token)
    {
        if (string.IsNullOrEmpty(_serverUrl))
            GameManager.gm.AppendLogLine("Failed to connect with API: no url", true, eLogtype.errorApi);
        else if (string.IsNullOrEmpty(_token))
            GameManager.gm.AppendLogLine("Failed to connect with API: no token", true, eLogtype.errorApi);
        else
        {
            server = _serverUrl + "/api";
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", _token);
            try
            {
                string response = await httpClient.GetStringAsync($"{_serverUrl}/api/token/valid");
                isReady = true;
                isInit = true;
                GameManager.gm.AppendLogLine("Connected to API", true, eLogtype.successApi);
            }
            catch (HttpRequestException e)
            {
                GameManager.gm.AppendLogLine($"Error while connecting to API: {e.Message}", true, eLogtype.errorApi);
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
            GameManager.gm.AppendLogLine(responseStr, false, eLogtype.infoApi);
        }
        catch (HttpRequestException e)
        {
            GameManager.gm.AppendLogLine(e.Message, false, eLogtype.errorApi);
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
            GameManager.gm.AppendLogLine(responseStr, false, eLogtype.infoApi);
        }
        catch (HttpRequestException e)
        {
            GameManager.gm.AppendLogLine(e.Message, false, eLogtype.errorApi);
        }

        isReady = true;
    }

    ///<summary>
    /// Avoid requestsToSend 
    /// Get an Object from the api. Call a Task callback with the response.
    ///</summary>
    ///<param name="_input">The path to add a base server for API GET request</param>
    ///<param name="_callback">Function to call to use GET response</param>
    public async Task GetObject(string _input, Func<string, Task> _callback)
    {
        if (!isInit)
        {
            GameManager.gm.AppendLogLine("Not connected to API", true, eLogtype.warning);
            return;
        }
        EventManager.Instance.Raise(new ChangeCursorEvent() { type = CursorChanger.CursorType.Loading });

        string fullPath = $"{server}/{_input}";
        try
        {
            string response = await httpClient.GetStringAsync(fullPath);
            GameManager.gm.AppendLogLine($"{response}", false, eLogtype.infoApi);
            await _callback(response);
        }
        catch (HttpRequestException e)
        {
            GameManager.gm.AppendLogLine(e.Message, false, eLogtype.errorApi);
            EventManager.Instance.Raise(new ChangeCursorEvent() { type = CursorChanger.CursorType.Loading });
        }
    }
    
    ///<summary>
    /// Avoid requestsToSend 
    /// Get an Object from the api. Call a Task<T> callback with the response.
    ///</summary>
    ///<param name="_input">The path to add a base server for API GET request</param>
    ///<param name="_callback">Function to call to use GET response</param>
    public async Task<T> GetObject<T>(string _input, Func<string, Task<T>> _callback)
    {
        if (!isInit)
        {
            GameManager.gm.AppendLogLine("Not connected to API", true, eLogtype.warningApi);
            return default;
        }
        EventManager.Instance.Raise(new ChangeCursorEvent() { type = CursorChanger.CursorType.Loading });

        string fullPath = $"{server}/{_input}";
        try
        {
            string response = await httpClient.GetStringAsync(fullPath);
            GameManager.gm.AppendLogLine($"From API: {response}", false, eLogtype.infoApi);
            return await _callback(response);
        }
        catch (HttpRequestException e)
        {
            GameManager.gm.AppendLogLine(e.Message, false, eLogtype.errorApi);
            EventManager.Instance.Raise(new ChangeCursorEvent() { type = CursorChanger.CursorType.Loading });
            return default;
        }
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
            GameManager.gm.AppendLogLine("Not connected to API", true, eLogtype.warningApi);
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
            GameManager.gm.AppendLogLine(responseStr, false, eLogtype.infoApi);

            if (responseStr.Contains("success"))
                await CreateItemFromJson(responseStr);
            else
                GameManager.gm.AppendLogLine($"Fail to post on server", false, eLogtype.errorApi);
        }
        catch (HttpRequestException e)
        {
            GameManager.gm.AppendLogLine(e.Message, false, eLogtype.errorApi);
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
            GameManager.gm.AppendLogLine("Not connected to API", true, eLogtype.warningApi);
            return;
        }
        Debug.Log(_json);
        string fullPath = $"{server}/{_type}-templates";

        StringContent content = new StringContent(_json, System.Text.Encoding.UTF8, "application/json");
        try
        {
            HttpResponseMessage response = await httpClient.PostAsync(fullPath, content);
            string responseStr = response.Content.ReadAsStringAsync().Result;
            GameManager.gm.AppendLogLine(responseStr, false, eLogtype.infoApi);

            if (responseStr.Contains("success"))
                await CreateTemplateFromJson(responseStr, _type);
            else
                GameManager.gm.AppendLogLine($"Fail to post on server", false, eLogtype.errorApi);
        }
        catch (HttpRequestException e)
        {
            GameManager.gm.AppendLogLine(e.Message, false, eLogtype.errorApi);
        }
    }

    ///<summary>
    /// Call the CreateXFromJson() method corresponding to the given API response.
    ///</summary>
    ///<param name="_input">The API response to use</param>
    public async Task DrawObject(string _input)
    {
        if (_input.Contains("successfully got query for object") || _input.Contains("successfully got object"))
            await CreateItemFromJson(_input);
        else if (_input.Contains("successfully got obj_template"))
            await CreateTemplateFromJson(_input, "obj");
        else if (_input.Contains("successfully got room_template"))
            await CreateTemplateFromJson(_input, "room");
        else
            GameManager.gm.AppendLogLine("Unknown object received", true, eLogtype.errorApi);
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

        GameManager.gm.AppendLogLine($"{physicalObjects.Count + logicalObjects.Count} object(s) created", false, eLogtype.successApi);
    }

    ///<summary>
    /// Use the given template json to instantiate an object or a room template.
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
