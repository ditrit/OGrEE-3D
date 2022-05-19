﻿using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;
using Newtonsoft.Json.Utilities;
using UnityEngine.UI;
using TMPro;



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

    public static ApiManager instance;

    private HttpClient httpClient = new HttpClient();

    public bool isInit = false;

    [SerializeField] private bool isReady = false;
    [SerializeField] private string server;

    [SerializeField] private Queue<SRequest> requestsToSend = new Queue<SRequest>();

    ReadFromJson rfJson = new ReadFromJson();
    [Header("AR")]
    [SerializeField] private List<string> previousCalls = new List<string>();
    [SerializeField] private List<string> parentNames = new List<string>();
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
    ///<param name="_input">The path to add a base server for API GET request</param>
    public async void GetObjectVincent(string _input, string _parentName)
    {
        try
        {
            string previousCall = previousCalls[previousCalls.Count - 2];
            if (previousCall == _input)
            {
                previousCalls.RemoveAt(previousCalls.Count - 1);
                previousCalls.RemoveAt(previousCalls.Count - 1);
                parentNames.RemoveAt(parentNames.Count - 1);
                parentNames.RemoveAt(parentNames.Count - 1);
            }
        }
        catch
        {
            Debug.Log("No previous calls");
        }
        if (!isInit)
        {
            GameManager.gm.AppendLogLine("Not connected to API", "yellow");
            return;
        }
        string fullPath = $"{server}/{_input}";
        Debug.Log($"fullpath is {fullPath}");
        try
        {
            HttpResponseMessage responseHTTP = await httpClient.GetAsync(fullPath);
            string response = responseHTTP.Content.ReadAsStringAsync().Result;
            GameManager.gm.AppendLogLine(response);
            if (response.Contains("successfully got query for object") || response.Contains("successfully got object"))
            {
                previousCalls.Add(_input);
                parentNames.Add(_parentName);
                CreateListFromJsonVincent(response);                
            }
            else
                GameManager.gm.AppendLogLine("Unknown object received", "red");
        }
        catch (HttpRequestException e)
        {
            GameManager.gm.AppendLogLine(e.Message, "red");
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

    ///<summary>
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
        string fullPath = $"{server}/{_input}";
        try
        {
            string response = await httpClient.GetStringAsync(fullPath);
            GameManager.gm.AppendLogLine(response);
            if (response.Contains("successfully got query for object") || response.Contains("successfully got object"))
                await CreateItemFromJson(response);
            else if (response.Contains("successfully got obj_template"))
                await CreateTemplateFromJson(response);
            else
                GameManager.gm.AppendLogLine("Unknown object received", "red");
        }
        catch (HttpRequestException e)
        {
            GameManager.gm.AppendLogLine(e.Message, "red");
        }
    }

    ///<summary>
    /// Avoid requestsToSend 
    /// Get an Object from the api. Create an ogreeObject with response.
    ///</summary>
    ///<param name="_input">The path to add a base server for API GET request</param>
    public async Task<string> GetObjectParentId(string _input)
    {
        
        List<SApiObject> physicalObjects = new List<SApiObject>();
        List<SApiObject> logicalObjects = new List<SApiObject>();

        if (!isInit)
        {
            GameManager.gm.AppendLogLine("Not connected to API", "yellow");
            return "error";
        }

        string fullPath = $"{server}/{_input}";
        try
        {
            string response = await httpClient.GetStringAsync(fullPath);
            if (Regex.IsMatch(response, "\"data\":{\"objects\":\\["))
            {
                SObjRespArray resp = JsonConvert.DeserializeObject<SObjRespArray>(response);
                foreach (SApiObject obj in resp.data.objects)
                    physicalObjects.Add(obj);
            }
            else
            {
                SObjRespSingle resp = JsonConvert.DeserializeObject<SObjRespSingle>(response);
                ParseNestedObjects(physicalObjects, logicalObjects, resp.data);
            }

            foreach (SApiObject obj in physicalObjects)
            {
                return obj.parentId;
            }
            return "error";
        }
        catch (HttpRequestException e)
        {
            GameManager.gm.AppendLogLine(e.Message, "red");
            return "error";
        }
    }


    ///<summary>
    /// Avoid requestsToSend 
    /// Get an Object from the api. Create an ogreeObject with response.
    ///</summary>
    ///<param name="_input">The path to add a base server for API GET request</param>
    public async Task<string> GetObjectName(string _input)
    {
        
        List<SApiObject> physicalObjects = new List<SApiObject>();
        List<SApiObject> logicalObjects = new List<SApiObject>();

        if (!isInit)
        {
            GameManager.gm.AppendLogLine("Not connected to API", "yellow");
            return "error";
        }

        string fullPath = $"{server}/{_input}";
        try
        {
            string response = await httpClient.GetStringAsync(fullPath);
            if (Regex.IsMatch(response, "\"data\":{\"objects\":\\["))
            {
                SObjRespArray resp = JsonConvert.DeserializeObject<SObjRespArray>(response);
                foreach (SApiObject obj in resp.data.objects)
                    physicalObjects.Add(obj);
            }
            else
            {
                SObjRespSingle resp = JsonConvert.DeserializeObject<SObjRespSingle>(response);
                ParseNestedObjects(physicalObjects, logicalObjects, resp.data);
            }

            foreach (SApiObject obj in physicalObjects)
            {
                return obj.name;
            }
            return "error";
        }
        catch (HttpRequestException e)
        {
            GameManager.gm.AppendLogLine(e.Message, "red");
            return "error";
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
    ///<param name="_obj">The serialized STemplate to post</param>
    public async Task PostTemplateObject(string _json)
    {
        if (!isInit)
        {
            GameManager.gm.AppendLogLine("Not connected to API", "yellow");
            return;
        }
        Debug.Log(_json);
        string fullPath = $"{server}/obj-templates";

        StringContent content = new StringContent(_json, System.Text.Encoding.UTF8, "application/json");
        try
        {
            HttpResponseMessage response = await httpClient.PostAsync(fullPath, content);
            string responseStr = response.Content.ReadAsStringAsync().Result;
            GameManager.gm.AppendLogLine(responseStr);

            if (responseStr.Contains("success"))
                await CreateTemplateFromJson(responseStr);
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
            ParseNestedObjects(physicalObjects, logicalObjects, resp.data);
        }

        foreach (SApiObject obj in physicalObjects)
        {
            if (obj.category != "tenant" && !GameManager.gm.allItems.Contains(obj.domain))
                await GetObject($"tenants?name={obj.domain}");

            if ((obj.category == "rack" || obj.category == "device") && !string.IsNullOrEmpty(obj.attributes["template"])
                && !GameManager.gm.objectTemplates.ContainsKey(obj.attributes["template"]))
            {
                Debug.Log($"Get template \"{obj.attributes["template"]}\" from API");
                await GetObject($"obj-templates/{obj.attributes["template"]}");
            }

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
                    if (obj.attributes["template"] == "")
                        ObjectGenerator.instance.CreateRack(obj);
                    else
                        ObjectGenerator.instance.CreateRack(obj, null, false);
                    break;
                case "device":
                    if (obj.attributes["template"] == "")
                        ObjectGenerator.instance.CreateDevice(obj);
                    else
                        ObjectGenerator.instance.CreateDevice(obj, null, false);
                    break;
                case "corridor":
                    ObjectGenerator.instance.CreateCorridor(obj);
                    break;
            }
        }
        foreach (SApiObject obj in logicalObjects)
        {
            if (obj.category != "tenant" && !GameManager.gm.allItems.Contains(obj.domain))
                await GetObject($"tenants?name={obj.domain}");

            switch (obj.category)
            {
                case "group":
                    ObjectGenerator.instance.CreateGroup(obj);
                    break;
            }
        }
        GameManager.gm.AppendLogLine($"{physicalObjects.Count + logicalObjects.Count} object(s) created", "green");
        EventManager.Instance.Raise(new ImportFinishedEvent());
    }

    ///<summary>
    /// Create an Ogree item from Json.
    /// Look in request path to the type of object to create a 3D list with the response.
    ///</summary>
    ///<param name="_json">The API response to use</param>
    private void CreateListFromJsonVincent(string _json)
    {
        List<SApiObject> physicalObjects = new List<SApiObject>();

        if (Regex.IsMatch(_json, "\"data\":{\"objects\":\\["))
        {
            SObjRespArray resp = JsonConvert.DeserializeObject<SObjRespArray>(_json);
            foreach (SApiObject obj in resp.data.objects)
                physicalObjects.Add(obj);
        }
        ListGenerator.instance.ClearParentList();
        //ListGenerator.instance.CreateList(physicalObjects, parentName);
        ListGenerator.instance.InstantiateByIndex(physicalObjects, parentNames, 0, previousCalls);
        GameManager.gm.AppendLogLine($"{physicalObjects.Count} object(s) created", "green");
        EventManager.Instance.Raise(new ImportFinishedEvent());
    }

    ///<summary>
    /// Use the given template json to instantiate an object template.
    ///</summary>
    ///<param name="_json">The json given by the API</param>
    private async Task CreateTemplateFromJson(string _json)
    {
        STemplateResp resp = JsonConvert.DeserializeObject<STemplateResp>(_json);
        await rfJson.CreateObjectTemplate(resp.data);
    }

    ///<summary>
    /// Parse a nested SApiObject and add each item to a given list.
    ///</summary>
    ///<param name="_physicalList">The list of physical objects to complete</param>
    ///<param name="_logicalList">The list of logical objects to complete</param>
    ///<param name="_src">The head of nested SApiObjects</param>
    private void ParseNestedObjects(List<SApiObject> _physicalList, List<SApiObject> _logicalList, SApiObject _src)
    {
        if (_src.category == "group")
            _logicalList.Add(_src);
        else
            _physicalList.Add(_src);
        if (_src.children != null)
        {
            foreach (SApiObject obj in _src.children)
                ParseNestedObjects(_physicalList, _logicalList, obj);
        }
    }

}
