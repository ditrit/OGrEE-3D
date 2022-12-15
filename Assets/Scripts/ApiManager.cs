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

    private readonly HttpClient httpClient = new HttpClient();

    public bool isInit = false;

    [SerializeField] private bool isReady = false;
    [SerializeField] private string server;

    [SerializeField] private Queue<SRequest> requestsToSend = new Queue<SRequest>();

    private readonly ReadFromJson rfJson = new ReadFromJson();

    private string url;
    private string token;

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
    /// Save API url and token.
    ///</summary>
    ///<param name="_url">URL of the API to connect</param>
    ///<param name="_token">Corresponding authorisation token</param>
    public void RegisterApi(string _url, string _token)
    {
        url = _url;
        token = _token;
    }

    ///<summary>
    /// Get registered API url.
    ///</summary>
    ///<returns>The registered url</returns>
    public string GetApiUrl()
    {
        return url;
    }

    ///<summary>
    /// Initialize the manager with url and token. 
    ///</summary>
    public async Task Initialize()
    {
        if (string.IsNullOrEmpty(url))
            GameManager.instance.AppendLogLine("Failed to connect with API: no url", true, ELogtype.errorApi);
        else if (string.IsNullOrEmpty(token))
            GameManager.instance.AppendLogLine("Failed to connect with API: no token", true, ELogtype.errorApi);
        else
        {
            server = url + "/api";
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", token);
            try
            {
                string response = await httpClient.GetStringAsync($"{url}/api/token/valid");
                isReady = true;
                isInit = true;
                GameManager.instance.AppendLogLine("Connected to API", true, ELogtype.successApi);
            }
            catch (HttpRequestException e)
            {
                GameManager.instance.AppendLogLine($"Error while connecting to API: {e.Message}", true, ELogtype.errorApi);
            }
        }
        EventManager.instance.Raise(new ConnectApiEvent());
    }

    ///<summary>
    /// Create an PUT request from _input.
    ///</summary>
    ///<param name="_obj">The OgreeObject to put</param>
    public void CreatePutRequest(OgreeObject _obj)
    {
        SApiObject apiObj = new SApiObject(_obj);
        SRequest request = new SRequest
        {
            type = "put",
            path = $"/{apiObj.category}s/{apiObj.id}",
            json = JsonConvert.SerializeObject(apiObj)
        };
        requestsToSend.Enqueue(request);
    }

    ///<summary>
    /// Create an DELETE request from _input.
    ///</summary>
    ///<param name="_obj">The OgreeObject to delete</param>
    public void CreateDeleteRequest(OgreeObject _obj)
    {
        SRequest request = new SRequest
        {
            type = "delete",
            path = $"/{_obj.category}s/{_obj.id}"
        };
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
            GameManager.instance.AppendLogLine(responseStr, false, ELogtype.infoApi);
        }
        catch (HttpRequestException e)
        {
            GameManager.instance.AppendLogLine(e.Message, false, ELogtype.errorApi);
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
            GameManager.instance.AppendLogLine(responseStr, false, ELogtype.infoApi);
        }
        catch (HttpRequestException e)
        {
            GameManager.instance.AppendLogLine(e.Message, false, ELogtype.errorApi);
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
            GameManager.instance.AppendLogLine("Not connected to API", true, ELogtype.warning);
            return;
        }
        EventManager.instance.Raise(new ChangeCursorEvent() { type = CursorChanger.CursorType.Loading });

        string fullPath = $"{server}/{_input}";
        try
        {
            string response = await httpClient.GetStringAsync(fullPath);
            GameManager.instance.AppendLogLine($"{response}", false, ELogtype.infoApi);
            await _callback(response);
        }
        catch (HttpRequestException e)
        {
            GameManager.instance.AppendLogLine(e.Message, false, ELogtype.errorApi);
            EventManager.instance.Raise(new ChangeCursorEvent() { type = CursorChanger.CursorType.Idle });
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
            GameManager.instance.AppendLogLine("Not connected to API", true, ELogtype.warningApi);
            return default;
        }
        EventManager.instance.Raise(new ChangeCursorEvent() { type = CursorChanger.CursorType.Loading });

        string fullPath = $"{server}/{_input}";
        try
        {
            string response = await httpClient.GetStringAsync(fullPath);
            GameManager.instance.AppendLogLine($"From API: {response}", false, ELogtype.infoApi);
            return await _callback(response);
        }
        catch (HttpRequestException e)
        {
            GameManager.instance.AppendLogLine(e.Message, false, ELogtype.errorApi);
            EventManager.instance.Raise(new ChangeCursorEvent() { type = CursorChanger.CursorType.Loading });
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
            GameManager.instance.AppendLogLine("Not connected to API", true, ELogtype.warningApi);
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
            GameManager.instance.AppendLogLine(responseStr, false, ELogtype.infoApi);

            if (responseStr.Contains("success"))
                await CreateItemFromJson(responseStr);
            else
                GameManager.instance.AppendLogLine($"Fail to post on server", false, ELogtype.errorApi);
        }
        catch (HttpRequestException e)
        {
            GameManager.instance.AppendLogLine(e.Message, false, ELogtype.errorApi);
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
            GameManager.instance.AppendLogLine("Not connected to API", true, ELogtype.warningApi);
            return;
        }
        Debug.Log(_json);
        string fullPath = $"{server}/{_type}-templates";

        StringContent content = new StringContent(_json, System.Text.Encoding.UTF8, "application/json");
        try
        {
            HttpResponseMessage response = await httpClient.PostAsync(fullPath, content);
            string responseStr = response.Content.ReadAsStringAsync().Result;
            GameManager.instance.AppendLogLine(responseStr, false, ELogtype.infoApi);

            if (responseStr.Contains("success"))
                await CreateTemplateFromJson(responseStr, _type);
            else
                GameManager.instance.AppendLogLine($"Fail to post on server", false, ELogtype.errorApi);
        }
        catch (HttpRequestException e)
        {
            GameManager.instance.AppendLogLine(e.Message, false, ELogtype.errorApi);
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
        {
            GameManager.instance.AppendLogLine("Unknown object received", true, ELogtype.errorApi);
            EventManager.instance.Raise(new ChangeCursorEvent() { type = CursorChanger.CursorType.Idle });
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

        GameManager.instance.AppendLogLine($"{physicalObjects.Count + logicalObjects.Count} object(s) created", false, ELogtype.successApi);
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
        EventManager.instance.Raise(new ChangeCursorEvent() { type = CursorChanger.CursorType.Loading });
    }
}
