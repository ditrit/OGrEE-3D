using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows.WebCam;
using System.Linq;
using UnityEngine.Networking;
using TMPro;
using System.Threading.Tasks;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Utilities.Solvers;
using System;

public class Label
{
    public string site;
    public string room;
    public string rack;
}

public class ApiListener : MonoBehaviour
{
    public static ApiListener instance;
    [SerializeField]
    [Tooltip("Assign DialogLarge_192x192.prefab")]
    private GameObject dialogPrefabLarge;
    private string currentHost;
    public string customer;
    public string site = "NOE";
    public string deviceType = "rack";
    private string building;
    private string room;
    private string rack;
    private string customerAndSite;
    public GameObject ButtonPicture;
    public GameObject quadButtonPhoto;
    private PhotoCapture photoCaptureObject = null;
    public TextMeshPro apiResponseTMP = null;
    public bool PictureCoolDown = true;

    private void Awake()
    {
        if (!instance)
            instance = this;
        else
            Destroy(this);
    }

    // Start is called before the first frame update
    private async void Start()
    {
        while (String.IsNullOrEmpty(customer))
        {
            await Task.Delay(50);
            try
            {
                customer = GameManager.gm.configLoader.GetTenant();
                currentHost = GameManager.gm.configLoader.GetPythonApiUrl();
            }
            catch{}
        }
        while (!ApiManager.instance.isInit)
        {
            await Task.Delay(50);
        }
        customerAndSite = customer + '.' + site;
    }

    /// <summary>
    /// Large Dialog example prefab to display
    /// </summary>
    public GameObject DialogPrefabLarge
    {
        get => dialogPrefabLarge;
        set => dialogPrefabLarge = value;
    }

    ///<summary>
    /// Call the function that takes a picture
    ///</summary>
    public void CapturePhoto()
    {
        if (PictureCoolDown)
        {
            PhotoCapture.CreateAsync(false, OnPhotoCaptureCreated);
            PictureCoolDown = false;
        }
    }

    ///<summary>
    /// Set parameters and format of the picture to take
    ///</summary>
    ///<param name="_captureObject">Photo</param>
    private void OnPhotoCaptureCreated(PhotoCapture _captureObject)
    {
        photoCaptureObject = _captureObject;

        Resolution cameraResolution = PhotoCapture.SupportedResolutions.OrderByDescending((res) => res.width * res.height).First();

        CameraParameters c = new CameraParameters();
        c.hologramOpacity = 0.0f;
        c.cameraResolutionWidth = cameraResolution.width;
        c.cameraResolutionHeight = cameraResolution.height;
        c.pixelFormat = CapturePixelFormat.JPEG;

        _captureObject.StartPhotoModeAsync(c, OnPhotoModeStarted);
    }

    ///<summary>
    /// Release the camera
    ///</summary>
    ///<param name="_result">result of the pipe</param>
    private void OnStoppedPhotoMode(PhotoCapture.PhotoCaptureResult _result)
    {
        photoCaptureObject.Dispose();
        photoCaptureObject = null;
    }

    ///<summary>
    /// take a picture
    ///</summary>
    ///<param name="_result">result of the pipe</param>
    private void OnPhotoModeStarted(PhotoCapture.PhotoCaptureResult _result)
    {
        if (_result.success)
        {
            photoCaptureObject.TakePhotoAsync(OnCapturedPhotoToMemory);
        }
        else
        {
            Debug.LogError("Unable to start photo mode!");
        }
    }

    ///<summary>
    /// process the photo on memory
    ///</summary>
    ///<param name="_result">result of the pipe</param>
    ///<param name="_photoCaptureFrame">frame captured</param>
    private async void OnCapturedPhotoToMemory(PhotoCapture.PhotoCaptureResult _result, PhotoCaptureFrame _photoCaptureFrame)
    {
        if (_result.success)
        {
            List<byte> imageBufferList = new List<byte>();
            // Copy the raw IMFMediaBuffer data into our empty byte list.
            _photoCaptureFrame.CopyRawImageDataIntoBuffer(imageBufferList);
            byte[] imageByteArray = imageBufferList.ToArray();
            if (imageByteArray != null)
            {
                await UploadByteAsync(imageByteArray);
            }
        }
        photoCaptureObject.StopPhotoModeAsync(OnStoppedPhotoMode);
    }

    ///<summary>
    /// Send picture to API and receive json
    ///</summary>
    ///<param name="_byteArray"> image in the format of a byte array</param>
    public async Task UploadByteAsync(byte[] _byteArray)
    {
        WWWForm form = new WWWForm();
        form.AddBinaryData("Label_Rack", _byteArray);
        form.AddField("Tenant_Name", customerAndSite);
        //form.AddField("deviceType", deviceType);
        quadButtonPhoto.GetComponent<Renderer>().material.color = Color.yellow;
        apiResponseTMP.gameObject.SetActive(true);
        apiResponseTMP.text = $"Start POST request with url = {currentHost} and site = {customerAndSite}";
        using (UnityWebRequest www = UnityWebRequest.Post($"http://{currentHost}", form))
        {
            www.SendWebRequest();
            while (!www.isDone)
                await Task.Delay(50);
            PictureCoolDown = true;
            if (www.result == UnityWebRequest.Result.ConnectionError)
            {
                apiResponseTMP.text = www.error;
                quadButtonPhoto.GetComponent<Renderer>().material.color = Color.red;
            }
            else
            {
                string text = www.downloadHandler.text;
                Debug.Log(text);
                Label labelReceived = JsonUtility.FromJson<Label>(text);
                site = labelReceived.site;

                if (site == "PCY")
                {
                    room = "UC" + labelReceived.room;
                }
                room = labelReceived.room;
                if (room.Length > 3)
                    room = room.Substring(3);
                rack = labelReceived.rack;

                apiResponseTMP.text = $"The label read is {site}{room}-{rack}";
                if (string.IsNullOrEmpty(room) && string.IsNullOrEmpty(rack))
                {
                    quadButtonPhoto.GetComponent<Renderer>().material.color = Color.red;
                    apiResponseTMP.text = apiResponseTMP.text + "\nCould not read the room and the rack label";
                }
                else if (string.IsNullOrEmpty(room))
                {
                    quadButtonPhoto.GetComponent<Renderer>().material.color = Color.red;
                    apiResponseTMP.text = apiResponseTMP.text + "\nCould not read the room label";
                }
                else if (string.IsNullOrEmpty(rack))
                {
                    quadButtonPhoto.GetComponent<Renderer>().material.color = Color.red;
                    apiResponseTMP.text = apiResponseTMP.text + "\nCould not read the rack label";
                }

                else if (!string.IsNullOrEmpty(rack))
                {
                    await SetBuilding(room);
                    if (building == "error" || building == null)
                    {
                        quadButtonPhoto.GetComponent<Renderer>().material.color = Color.red;
                        GameManager.gm.AppendLogLine(building);
                        apiResponseTMP.text = apiResponseTMP.text + "\nError while getting the parent building of the room";
                    }
                    else
                    {
                        quadButtonPhoto.GetComponent<Renderer>().material.color = Color.green;
                        apiResponseTMP.text = apiResponseTMP.text + "\nLoading Rack please wait...";

                        ButtonPicture.SetActive(false);
                        apiResponseTMP.gameObject.SetActive(false);
                        Dialog myDialog = Dialog.Open(DialogPrefabLarge, DialogButtonType.Confirm | DialogButtonType.Cancel, "Found Rack !", $"Please click on 'Confirm' to place the rack {site}{room}-{rack}.\nClick on 'Cancel' if the label was misread or if you want to take another picture.", true);
                        ConfigureDialog(myDialog);
                        while (myDialog.State != DialogState.Closed)
                        {
                            await Task.Delay(100);
                        }

                        if (myDialog.Result.Result == DialogButtonType.Confirm)
                        {
                            await LoadSingleRack(customer, site, building, room, rack);
                        }

                        if (myDialog.Result.Result == DialogButtonType.Cancel)
                        {
                            ButtonPicture.SetActive(true);
                            apiResponseTMP.gameObject.SetActive(true);
                        }
                    }
                }

                else
                {
                    quadButtonPhoto.GetComponent<Renderer>().material.color = Color.red;
                }
            }
        }
    }

    ///<summary>
    /// Retrieve parent building of a room
    ///</summary>
    ///<param name="_room">string refering to a room name</param>
    public async Task SetBuilding(string _room)
    {
        SApiObject obj = await ApiManager.instance.GetObject($"rooms?name=" + _room, ApiManager.instance.GetFirstSApiObject);
        string parentId = obj.parentId;
        obj = await ApiManager.instance.GetObject($"buildings/" + parentId, ApiManager.instance.GetFirstSApiObject);
        building = obj.name;
    }

    ///<summary>
    /// Activate a Gameobject if it is not active. Deactivate it if it is active.
    ///</summary>
    ///<param name="_g">Gameobject to activate/deactivate</param>
    public void ToggleGameobject(GameObject _g)
    {
        if (_g.activeSelf)
        {
            _g.SetActive(false);
        }
        else
        {
            _g.SetActive(true);
        }
    }

    ///<summary>
    /// Activate a Gameobject if it is not active and move it in front of the camera. Deactivate it if it is active.
    ///</summary>
    ///<param name="_g">Gameobject to activate/deactivate</param>
    public void ToggleAndMoveGameobject(GameObject _g)
    {
        if (_g.activeSelf)
        {
            _g.SetActive(false);
        }
        else
        {
            _g.SetActive(true);
            Utils.MoveObjectToCamera(_g, GameManager.gm.mainCamera, 0.6f, -0.25f, 0, 25);
        }
    }

    ///<summary>
    /// Load the 3D model of a rack
    ///</summary>
    ///<param name="_customer">string refering to a customer</param>
    ///<param name="_site">string refering to a site</param>
    ///<param name="_building">string refering to a building</param>
    ///<param name="_room">string refering to a room</param>
    ///<param name="_rack">string refering to a rack</param>
    public async Task LoadSingleRack(string _customer, string _site, string _building, string _room, string _rack)
    {
        GameObject customer = GameManager.gm.FindByAbsPath(_customer);
        await GameManager.gm.DeleteItem(customer, false);

        await ApiManager.instance.GetObject($"sites/" + _site, ApiManager.instance.DrawObject);
        await ApiManager.instance.GetObject($"tenants/" + _customer + "/sites/" + _site + "/buildings/" + _building, ApiManager.instance.DrawObject);
        await ApiManager.instance.GetObject($"tenants/" + _customer + "/sites/" + _site + "/buildings/" + _building + "/rooms/" + _room, ApiManager.instance.DrawObject);
        await ApiManager.instance.GetObject($"tenants/" + _customer + "/sites/" + _site + "/buildings/" + _building + "/rooms/" + _room + "/racks/" + _rack, ApiManager.instance.DrawObject);
        GameObject rack = GameManager.gm.FindByAbsPath(_customer + "." + _site + "." + _building + "." + _room + "." + _rack);
        if (rack == null)
            GameManager.gm.AppendLogLine("Rack NOT Found in the scene after loading from API", "red");
        else
        {
            GameManager.gm.AppendLogLine("Rack Found in the scene after loading from API", "green");
            Utils.MoveObjectToCamera(rack, GameManager.gm.mainCamera, 1.5f, -0.7f, 90, 0);

            OgreeObject ogree = rack.GetComponent<OgreeObject>();
            ogree.originalLocalRotation = rack.transform.localRotation;  //update the originalLocalRotation to not mess up when using reset button from TIM
            ogree.originalLocalPosition = rack.transform.localPosition;
            await Task.Delay(100); // await for Uroot object to be created
            var goArray = FindObjectsOfType<GameObject>();
            for (var i = 0; i < goArray.Length; i++)
            {
                if (goArray[i].layer == LayerMask.NameToLayer("Rack"))
                {
                    if (goArray[i].transform.GetChild(2))
                    {
                        goArray[i].transform.GetChild(2).GetComponent<HandInteractionHandler>().SelectThis();
                        return;
                    }
                }
            }
        }
    }

    ///<summary>
    /// Configure A Dialog with fixed parameters
    ///</summary>
    ///<param name="_myDialog"> The Dialog box to update</param>
    public void ConfigureDialog(Dialog _myDialog)
    {
        _myDialog.GetComponent<Follow>().MinDistance = 0.5f;
        _myDialog.GetComponent<Follow>().MaxDistance = 0.7f;
        _myDialog.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
        _myDialog.GetComponent<ConstantViewSize>().MinDistance = 0.5f;
        _myDialog.GetComponent<ConstantViewSize>().MinDistance = 0.7f;
        _myDialog.GetComponent<ConstantViewSize>().MinScale = 0.05f;
    }
}
