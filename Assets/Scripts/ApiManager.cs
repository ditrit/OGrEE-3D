﻿using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    public static ApiManager instance;

    private HttpClient httpClient = new HttpClient();

    public bool isInit = false;

    [SerializeField] private bool isReady = false;
    [SerializeField] private string server;
    // [SerializeField] private string login;
    // [SerializeField] private string token;

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
    public async Task GetObject(string _input, bool _isSingleObj)
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
                CreateItemFromJson(response, _isSingleObj);
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
    private void CreateItemFromJson(string _json, bool _singleObject)
    {
        List<SApiObject> objsToCreate = new List<SApiObject>();

        if (_singleObject)
        {
            SObjRespSingle resp = JsonConvert.DeserializeObject<SObjRespSingle>(_json);
            objsToCreate.Add(resp.data);
        }
        else
        {
            SObjRespArray resp = JsonConvert.DeserializeObject<SObjRespArray>(_json);            
            foreach (SApiObject obj in resp.data.objects)
                objsToCreate.Add(obj);
        }

        GameManager.gm.AppendLogLine($"{objsToCreate.Count} object(s) created", "green");

        objsToCreate.OrderBy(o => o.category == "tenant")
                     .ThenBy(o => o.category == "site")
                     .ThenBy(o => o.category == "building")
                     .ThenBy(o => o.category == "room")
                     .ThenBy(o => o.category == "rack")
                     .ThenBy(o => o.category == "device")
                     .ThenBy(o => o.category == "corridor")
                     .ThenBy(o => o.category == "group");

        foreach (SApiObject obj in objsToCreate)
        {
            switch (obj.category)
            {
                case "tenant":
                    CustomerGenerator.instance.CreateTenant(obj);
                    break;
                case "site":
                    CustomerGenerator.instance.CreateSite(obj);
                    break;
                case "building":
                    BuildingGenerator.instance.CreateBuilding(obj);
                    break;
                case "room":
                    BuildingGenerator.instance.CreateRoom(obj);
                    break;
                case "rack":
                    ObjectGenerator.instance.CreateRack(obj);
                    break;
                case "device":
                    ObjectGenerator.instance.CreateDevice(obj);
                    break;
                    // case "group":
                    //     ObjectGenerator.instance.CreateGroup(obj);
                    //     break;
                    // case "corridor":
                    //     ObjectGenerator.instance.CreateCorridor(obj);
                    //     break;
                    // case "separator":
                    //     BuildingGenerator.instance.CreateSeparator(obj);
                    //     break;
            }
        }
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
            SObjRespSingle resp = JsonConvert.DeserializeObject<SObjRespSingle>(_json);
            CreateItemFromJson(_json, true);
        }
        else
            GameManager.gm.AppendLogLine($"Fail to post on server", "red");
    }

}
