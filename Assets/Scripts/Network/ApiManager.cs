using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Localization;

public class ApiManager : MonoBehaviour
{
    #region Structures from API
    public struct SRequest
    {
        public string type;
        public string path;
        public string json;
    }

    private struct SObjectResp
    {
        public string message;
        public SApiObject data;
    }

    private struct STempUnitResp
    {
        public string message;
        public STempUnit data;
    }

    private struct SVersionResp
    {
        public Dictionary<string, string> data;
        public bool status;
    }

    private struct STagResp
    {
        public string message;
        public SApiTag data;
    }

    private struct SLayerContentResp
    {
        public string message;
        public List<SApiObject> data;
    }
    #endregion

    public static ApiManager instance;

    private readonly HttpClient httpClient = new();
    private readonly HttpClient sseHttpClient = new();
    private Thread sseThread;

    public bool isInit = false;

    [SerializeField] private bool isReady = false;
    [SerializeField] private string server;

    [SerializeField] private Queue<SRequest> requestsToSend = new();

    private readonly ReadFromJson rfJson = new();
    private readonly CommandParser parser = new();
    public readonly Queue<Action> mainThreadQueue = new();

#pragma warning disable IDE1006 // Name assignment styles
    public string url { get; private set; }
#pragma warning restore IDE1006 // Name assignment styles
    private string token;
    private bool canDraw = true;

    private void Awake()
    {
        if (!instance)
            instance = this;
        else
            Destroy(this);
    }

    private void Start()
    {
        EventManager.instance.CancelGenerate.Add(OnCancelGenenerate);
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
        while (mainThreadQueue.Count > 0)
            mainThreadQueue.Dequeue().Invoke();
    }

    private void OnDestroy()
    {
        EventManager.instance.CancelGenerate.Remove(OnCancelGenenerate);
        ResetApi();
    }

    ///<summary>
    /// When called, set canDraw to false
    ///</summary>
    ///<param name="_e">The event's instance</param>
    private void OnCancelGenenerate(CancelGenerateEvent _e)
    {
        canDraw = false;
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

    /// <summary>
    /// Unregister <see cref="url"/>, <see cref="token"/>  & <see cref="server"/> and set <see cref="isInit"/> to false.
    /// </summary>
    public void ResetApi()
    {
        isInit = false;
        url = null;
        token = null;
        server = null;
        if (sseThread.IsAlive)
            sseThread.Abort();
        EventManager.instance.Raise(new ConnectApiEvent());
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
        SVersionResp apiResp = new();
        if (string.IsNullOrEmpty(url))
            GameManager.instance.AppendLogLine(new LocalizedString("Logs", "Failed to connect with API: no url"), ELogTarget.both, ELogtype.errorApi);
        else if (string.IsNullOrEmpty(token))
            GameManager.instance.AppendLogLine(new LocalizedString("Logs", "Failed to connect with API: no token"), ELogTarget.both, ELogtype.errorApi);
        else
        {
            server = url + "/api";
            httpClient.DefaultRequestHeaders.Authorization = new("bearer", token);
            try
            {
                string response = await httpClient.GetStringAsync($"{server}/version");
                GameManager.instance.AppendLogLine(response, ELogTarget.none, ELogtype.infoApi);
                apiResp = JsonConvert.DeserializeObject<SVersionResp>(response);
                isReady = true;
                isInit = true;
                GameManager.instance.AppendLogLine(new LocalizedString("Logs", "Connected to API"), ELogTarget.both, ELogtype.successApi);
                sseThread = new(GetStream)
                {
                    IsBackground = true
                };
                sseThread.Start();
            }
            catch (HttpRequestException e)
            {
                GameManager.instance.AppendLogLine(new ExtendedLocalizedString($"Logs", "Error while connecting to API", e.Message), ELogTarget.both, ELogtype.errorApi);
            }
        }
        if (!string.IsNullOrEmpty(apiResp.data["Customer"]))
            EventManager.instance.Raise(new ConnectApiEvent(apiResp.data));
    }

    ///<summary>
    /// Create an PUT request from _input.
    ///</summary>
    ///<param name="_obj">The OgreeObject to put</param>
    public void CreatePutRequest(OgreeObject _obj)
    {
        SApiObject apiObj = new(_obj);
        SRequest request = new()
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
        SRequest request = new()
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
        StringContent content = new(req.json, System.Text.Encoding.UTF8, "application/json");
        try
        {
            HttpResponseMessage response = await httpClient.PutAsync(fullPath, content);
            string responseStr = response.Content.ReadAsStringAsync().Result;
            GameManager.instance.AppendLogLine(responseStr, ELogTarget.none, ELogtype.infoApi);
        }
        catch (HttpRequestException e)
        {
            GameManager.instance.AppendLogLine(e.Message, ELogTarget.none, ELogtype.errorApi);
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
            GameManager.instance.AppendLogLine(responseStr, ELogTarget.none, ELogtype.infoApi);
        }
        catch (HttpRequestException e)
        {
            GameManager.instance.AppendLogLine(e.Message, ELogTarget.none, ELogtype.errorApi);
        }

        isReady = true;
    }

    /// <summary>
    /// Subscribe to stream from the API. For each received message, call <see cref="CommandParser.DeserializeInput(string)"/>
    /// </summary>
    public async void GetStream()
    {
        while (isInit)
        {
            try
            {
                sseHttpClient.DefaultRequestHeaders.Authorization = new("bearer", token);
                sseHttpClient.Timeout = Timeout.InfiniteTimeSpan;
                Debug.Log($"Getting Stream at {server}/events...");
                using Stream stream = await sseHttpClient.GetStreamAsync($"{server}/events");
                stream.ReadTimeout = Timeout.Infinite;
                using StreamReader reader = new(stream);
                while (isInit && !reader.EndOfStream)
                {
                    string message = await reader.ReadLineAsync();
                    if (!string.IsNullOrEmpty(message))
                    {
                        // Remove "data: " from SSE msg
                        message = message[6..];
                        mainThreadQueue.Enqueue(async () =>
                        {
                            GameManager.instance.AppendLogLine($"(SSE) {message}", ELogTarget.none, ELogtype.infoApi);
                            await parser.DeserializeInput(message);
                        });
                    }
                }
            }
            catch (HttpRequestException e)
            {
                Debug.LogError(e);
                mainThreadQueue.Enqueue(() =>
                {
                    GameManager.instance.AppendLogLine($"(SSE) {e.Message}", ELogTarget.logger, ELogtype.errorApi);
                    GameManager.instance.AppendLogLine($"(SSE) Reconnecting...", ELogTarget.logger, ELogtype.infoApi);
                });
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                await Task.Delay(TimeSpan.FromSeconds(10));
            }
        }
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
            GameManager.instance.AppendLogLine(new LocalizedString("Logs", "Not connected to API"), ELogTarget.both, ELogtype.warning);
            return;
        }
        EventManager.instance.Raise(new ChangeCursorEvent(CursorChanger.CursorType.Loading));

        string fullPath = $"{server}/{_input}";
        try
        {
            string response = await httpClient.GetStringAsync(fullPath);
            GameManager.instance.AppendLogLine(new ExtendedLocalizedString("Logs", "From API", response), ELogTarget.none, ELogtype.infoApi);
            await _callback(response);
        }
        catch (HttpRequestException e)
        {
            GameManager.instance.AppendLogLine($"{fullPath}: {e.Message}", ELogTarget.logger, ELogtype.errorApi);
            EventManager.instance.Raise(new ChangeCursorEvent(CursorChanger.CursorType.Idle));
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
            GameManager.instance.AppendLogLine(new LocalizedString("Logs", "Not connected to API"), ELogTarget.both, ELogtype.warning);
            return default;
        }
        EventManager.instance.Raise(new ChangeCursorEvent(CursorChanger.CursorType.Loading));

        string fullPath = $"{server}/{_input}";
        try
        {
            string response = await httpClient.GetStringAsync(fullPath);
            GameManager.instance.AppendLogLine(new ExtendedLocalizedString("Logs", "From API", response), ELogTarget.none, ELogtype.infoApi);
            return await _callback(response);
        }
        catch (HttpRequestException e)
        {
            GameManager.instance.AppendLogLine($"{fullPath}: {e.Message}", ELogTarget.logger, ELogtype.errorApi);
            EventManager.instance.Raise(new ChangeCursorEvent(CursorChanger.CursorType.Loading));
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
            GameManager.instance.AppendLogLine(new LocalizedString("Logs", "Not connected to API"), ELogTarget.both, ELogtype.warning);
            return;
        }
        string json = JsonConvert.SerializeObject(_obj);
        // Debug.Log(json);
        string fullPath = $"{server}/{_obj.category}s";

        StringContent content = new(json, System.Text.Encoding.UTF8, "application/json");
        try
        {
            HttpResponseMessage response = await httpClient.PostAsync(fullPath, content);
            string responseStr = response.Content.ReadAsStringAsync().Result;
            GameManager.instance.AppendLogLine(new ExtendedLocalizedString("Logs", "From API", responseStr), ELogTarget.none, ELogtype.infoApi);

            if (responseStr.Contains("success"))
                await CreateObjectFromJson(responseStr);
            else
                GameManager.instance.AppendLogLine(new LocalizedString("Logs", "Fail to post on server"), ELogTarget.logger, ELogtype.errorApi);
        }
        catch (HttpRequestException e)
        {
            GameManager.instance.AppendLogLine(e.Message, ELogTarget.none, ELogtype.errorApi);
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
            GameManager.instance.AppendLogLine(new LocalizedString("Logs", "Not connected to API"), ELogTarget.both, ELogtype.warning);
            return;
        }
        // Debug.Log(_json);
        string fullPath = $"{server}/{_type}-templates";

        StringContent content = new(_json, System.Text.Encoding.UTF8, "application/json");
        try
        {
            HttpResponseMessage response = await httpClient.PostAsync(fullPath, content);
            string responseStr = response.Content.ReadAsStringAsync().Result;
            GameManager.instance.AppendLogLine(new ExtendedLocalizedString("Logs", "From API", responseStr), ELogTarget.none, ELogtype.infoApi);

            if (responseStr.Contains("success"))
                await CreateTemplateFromJson(responseStr);
            else
                GameManager.instance.AppendLogLine(new LocalizedString("Logs", "Fail to post on server"), ELogTarget.logger, ELogtype.errorApi);
        }
        catch (HttpRequestException e)
        {
            GameManager.instance.AppendLogLine(e.Message, ELogTarget.none, ELogtype.errorApi);
        }
    }

    ///<summary>
    /// Create an Ogree object and its children from given Json
    ///</summary>
    ///<param name="_json">The API response to use</param>
    public async Task CreateObjectFromJson(string _json)
    {
        List<SApiObject> physicalObjects = new();
        List<SApiObject> logicalObjects = new();
        List<string> leafIds = new();

        SObjectResp resp = JsonConvert.DeserializeObject<SObjectResp>(_json);
        Utils.ParseNestedObjects(physicalObjects, logicalObjects, resp.data, leafIds);

        foreach (SApiObject obj in physicalObjects)
            if (canDraw)
                await OgreeGenerator.instance.CreateItemFromSApiObject(obj);

        foreach (SApiObject obj in logicalObjects)
            if (canDraw)
                await OgreeGenerator.instance.CreateItemFromSApiObject(obj);

        foreach (string id in leafIds)
            if (Utils.GetObjectById(id) is GameObject leaf)
                Utils.RebuildLods(leaf.transform);

        if (canDraw)
            GameManager.instance.AppendLogLine(new ExtendedLocalizedString("Logs", "X objects created", physicalObjects.Count + logicalObjects.Count), ELogTarget.logger, ELogtype.successApi);
        canDraw = true;
    }

    ///<summary>
    /// Call the CreateXTemplate method of <see cref="rfJson"/> corresponding to given slug in <paramref name="_input"/>
    ///</summary>
    ///<param name="_input">The API response to use</param>
    public async Task CreateTemplateFromJson(string _input)
    {
        string dataStr = JsonConvert.DeserializeObject<Hashtable>(_input)["data"].ToString();
        Hashtable data = JsonConvert.DeserializeObject<Hashtable>(dataStr);

        switch (data["category"])
        {
            case Category.Building:
                SBuildingFromJson buildingData = JsonConvert.DeserializeObject<SBuildingFromJson>(dataStr);
                rfJson.CreateBuildingTemplate(buildingData);
                break;
            case Category.Room:
                SRoomFromJson roomData = JsonConvert.DeserializeObject<SRoomFromJson>(dataStr);
                rfJson.CreateRoomTemplate(roomData);
                break;
            case Category.Rack:
            case Category.Device:
            case Category.Generic:
                STemplate deviceData = JsonConvert.DeserializeObject<STemplate>(dataStr);
                await rfJson.CreateObjectTemplate(deviceData);
                break;
        }
        EventManager.instance.Raise(new ChangeCursorEvent(CursorChanger.CursorType.Loading));
    }

    /// <summary>
    /// Attemps to draw an object or it's parent if not already drawn. <b>Given "data" must be an array</b>
    /// </summary>
    /// <param name="_input"></param>
    public async Task CreateObjectAndParents(string _input)
    {
        string dataStr = JsonConvert.DeserializeObject<Hashtable>(_input)["data"].ToString();
        List<SApiObject> objects = JsonConvert.DeserializeObject<List<SApiObject>>(dataStr);
        await OgreeGenerator.instance.CreateOrGetParent(objects[0]);
    }

    /// <summary>
    /// Give a list of <see cref="SApiObject"/> from received data. <b>Given "data" must be an array</b>
    /// </summary>
    /// <param name="_input"></param>
    /// <returns>The first SApiObject received</returns>
    public Task<List<SApiObject>> GetSApiObjects(string _input)
    {
        string dataStr = JsonConvert.DeserializeObject<Hashtable>(_input)["data"].ToString();
        List<SApiObject> objects = JsonConvert.DeserializeObject<List<SApiObject>>(dataStr);
        return Task.Run(() => objects);
    }

    /// <summary>
    /// Use response from API to get a temperatureUnit.
    /// </summary>
    /// <param name="_input">The API response</param>
    /// <returns>A temperature unit if correct response from API or an empty string</returns>
    public Task<string> TempUnitFromAPI(string _input)
    {
        if (_input.Contains("successfully got temperatureUnit from object's parent site"))
        {
            STempUnitResp resp = JsonConvert.DeserializeObject<STempUnitResp>(_input);
            return Task.FromResult(resp.data.temperatureUnit);
        }
        GameManager.instance.AppendLogLine(new LocalizedString("Logs", "Retrieving temperature unit error"), ELogTarget.both, ELogtype.errorApi);
        return Task.FromResult("");
    }

    /// <summary>
    /// Use response from API to get zone colors from API
    /// </summary>
    /// <param name="_input">The API response</param>
    /// <returns>A dictionary containing "usableColor", "reservedColor" and "technicalColor"</returns>
    public Task<Dictionary<string, string>> SiteColorsFromAPI(string _input)
    {
        if (_input.Contains("successfully got attribute from object's parent site"))
        {
            Hashtable resp = JsonConvert.DeserializeObject<Hashtable>(_input);
            return Task.FromResult(JsonConvert.DeserializeObject<Dictionary<string, string>>(resp["data"].ToString()));
        }
        GameManager.instance.AppendLogLine(new LocalizedString("Logs", "Retrieving colors error"), ELogTarget.both, ELogtype.errorApi);
        return Task.FromResult(new Dictionary<string, string>());
    }

    /// <summary>
    /// Deserialize API response to <see cref="STagResp"/> and call <see cref="GameManager.CreateTag(SApiTag)"/>
    /// </summary>
    /// <param name="_input">The API response</param>
    public async Task CreateTag(string _input)
    {
        STagResp resp = JsonConvert.DeserializeObject<STagResp>(_input);
        await Task.Run(() => GameManager.instance.CreateTag(resp.data));
        UiManager.instance.tagsList.RebuildMenu(UiManager.instance.BuildTagButtons);
        EventManager.instance.Raise(new ChangeCursorEvent(CursorChanger.CursorType.Idle));
    }

    /// <summary>
    /// Deserialize API response to <see cref="SLayerResp"/>, create a Layer & add it to <see cref="LayerManager.layers"/>.
    /// </summary>
    /// <param name="_input">The API response</param>
    public Task CreateLayer(string _input)
    {
        try
        {
            string dataStr = JsonConvert.DeserializeObject<Hashtable>(_input)["data"].ToString();
            List<SApiLayer> resp = JsonConvert.DeserializeObject<List<SApiLayer>>(dataStr);
            foreach (SApiLayer al in resp)
                LayerManager.instance.CreateLayerFromSApiLayer(al);
            EventManager.instance.Raise(new ChangeCursorEvent(CursorChanger.CursorType.Idle));
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
        return Task.CompletedTask;
    }

    /// <summary>
    /// Deserialize API response to <see cref="SLayerContentResp"/> and return a list of corresponding GameObjects
    /// </summary>
    /// <param name="_input">The API response</param>
    /// <returns>The list of GameObjects correponding to the API response</returns>
    public Task<List<GameObject>> GetLayerContent(string _input)
    {
        List<GameObject> list = new();
        SLayerContentResp resp = JsonConvert.DeserializeObject<SLayerContentResp>(_input);
        foreach (SApiObject obj in resp.data)
            if (Utils.GetObjectById(obj.id) is GameObject go)
                list.Add(go);

        EventManager.instance.Raise(new ChangeCursorEvent(CursorChanger.CursorType.Idle));
        return Task.FromResult(list);
    }

    ///<summary>
    /// Modify an object in the api.
    ///</summary>
    ///<param name="_input">The path to add a base server for API PATCH request</param>
    ///<param name="_data">New partial data of the object</param>
    public async Task ModifyObject(string _input, Dictionary<string, object> _data)
    {
        if (!isInit)
        {
            GameManager.instance.AppendLogLine("Not connected to API", ELogTarget.both, ELogtype.warning);
            return;
        }
        EventManager.instance.Raise(new ChangeCursorEvent(CursorChanger.CursorType.Loading));

        string fullPath = $"{server}/{_input}";
        StringContent content = new(JsonConvert.SerializeObject(_data), System.Text.Encoding.UTF8, "application/json");
        try
        {
            HttpRequestMessage request = new(new HttpMethod("PATCH"), fullPath)
            {
                Content = content
            };
            HttpResponseMessage response = await httpClient.SendAsync(request);
            GameManager.instance.AppendLogLine(new ExtendedLocalizedString("Logs", "From API", response.Content.ReadAsStringAsync().Result), ELogTarget.none, ELogtype.infoApi);
            EventManager.instance.Raise(new ChangeCursorEvent(CursorChanger.CursorType.Idle));
        }
        catch (HttpRequestException e)
        {
            GameManager.instance.AppendLogLine($"{fullPath}: {e.Message}", ELogTarget.logger, ELogtype.errorApi);
            EventManager.instance.Raise(new ChangeCursorEvent(CursorChanger.CursorType.Idle));
        }
    }
}
