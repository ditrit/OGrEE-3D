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

public class Label2
{
    public string site;
    public List<string> label;
}

public class NewAPIListener : MonoBehaviour
{
    public static NewAPIListener instance;
    [SerializeField]
    [Tooltip("Assign DialogLarge_192x192.prefab")]
    private GameObject dialogPrefabLarge;
    private string currentHost = "";
    public string customer = "EDF";
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
    private string[] array;

    private void Awake()
    {
        if (!instance)
            instance = this;
        else
            Destroy(this);
    }
    
    // Start is called before the first frame update
    private void Start()
    {
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

    public void RequestSentColor()
    {
        quadButtonPhoto.GetComponent<Renderer>().material.color = Color.yellow;
    }

    public void RequestFailColor()
    {
        quadButtonPhoto.GetComponent<Renderer>().material.color = Color.red;
    }

    public void RequestSuccessColor()
    {
        quadButtonPhoto.GetComponent<Renderer>().material.color = Color.green;
    }


    ///<summary>
    /// Call the function that takes a picture
    ///</summary>
    public void CapturePhoto()
    {
        PhotoCapture.CreateAsync(false, OnPhotoCaptureCreated);
    }

    ///<summary>
    /// Set parameters and format of the picture to take
    ///</summary>
    ///<param name="captureObject">Photo</param>
    private void OnPhotoCaptureCreated(PhotoCapture captureObject)
    {
        photoCaptureObject = captureObject;

        Resolution cameraResolution = PhotoCapture.SupportedResolutions.OrderByDescending((res) => res.width * res.height).First();

        CameraParameters c = new CameraParameters();
        c.hologramOpacity = 0.0f;
        c.cameraResolutionWidth = cameraResolution.width;
        c.cameraResolutionHeight = cameraResolution.height;
        c.pixelFormat = CapturePixelFormat.JPEG;

        captureObject.StartPhotoModeAsync(c, OnPhotoModeStarted);
    }

    ///<summary>
    /// Release the camera
    ///</summary>
    ///<param name="result">result of the pipe</param>
    private void OnStoppedPhotoMode(PhotoCapture.PhotoCaptureResult result)
    {
        photoCaptureObject.Dispose();
        photoCaptureObject = null;
    }
    
    ///<summary>
    /// take a picture
    ///</summary>
    ///<param name="result">result of the pipe</param>
    private void OnPhotoModeStarted(PhotoCapture.PhotoCaptureResult result)
    {
        if (result.success)
        {
            photoCaptureObject.TakePhotoAsync(OnCapturedPhotoToMemory);
        }
        else
        {
            Debug.LogError("Unable to start photo mode!");
        }
    }

    ///<summary>
    /// process on memoty the photo
    ///</summary>
    ///<param name="result">result of the pipe</param>
    ///<param name="photoCaptureFrame">frame captured</param>
    private async void OnCapturedPhotoToMemory(PhotoCapture.PhotoCaptureResult result, PhotoCaptureFrame photoCaptureFrame)
    {
        if (result.success)
        {
            List<byte> imageBufferList = new List<byte>();
            // Copy the raw IMFMediaBuffer data into our empty byte list.
            photoCaptureFrame.CopyRawImageDataIntoBuffer(imageBufferList);
            byte[] imageByteArray = imageBufferList.ToArray();
            if (imageByteArray != null)
            {
                await UploadByteAsync(imageByteArray);
            }
        }
        photoCaptureObject.StopPhotoModeAsync(OnStoppedPhotoMode);
    }

    ///<summary>
    /// Retrieve parent building of a room
    ///</summary>
    ///<param name="_room">string refering to a room name</param>
    public async Task SetBuilding(string _room)
    {
        //string data = await ApiManager.instance.GetObjectParentId($"rooms?name=" + _room);
        //building = await ApiManager.instance.GetObjectName($"buildings/" + data);
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
    /// Load the 3D model of a rack
    ///</summary>
    ///<param name="_customer">string refering to a customer</param>
    ///<param name="_site">string refering to a site</param>
    ///<param name="_building">string refering to a building</param>
    ///<param name="_room">string refering to a room</param>
    ///<param name="_rack">string refering to a rack</param>
    public async Task LoadSingleRack(string _customer, string _site, string _building, string _room, string _rack)
    {
        GameObject room = GameManager.gm.FindByAbsPath(_customer + "." + _site + "." + _building + "." + _room);
        GameManager.gm.DeleteItem(room, false);
        await ApiManager.instance.GetObject($"sites/"+ _site, ApiManager.instance.DrawObject);
        await ApiManager.instance.GetObject($"tenants/" + _customer + "/sites/" + _site + "/buildings/" +_building, ApiManager.instance.DrawObject);
        await ApiManager.instance.GetObject($"tenants/" + _customer + "/sites/" + _site + "/buildings/" + _building + "/rooms/" + _room, ApiManager.instance.DrawObject);
        await ApiManager.instance.GetObject($"tenants/" + _customer + "/sites/" + _site + "/buildings/" + _building + "/rooms/" + _room + "/racks/" + _rack, ApiManager.instance.DrawObject);
        GameObject rack = GameManager.gm.FindByAbsPath(_customer + "." + _site + "." + _building + "." + _room + "." + _rack);
        if (rack != null)
            GameManager.gm.AppendLogLine("Rack Found in the scene after loading from API", "green");
        else 
            GameManager.gm.AppendLogLine("Rack NOT Found in the scene after loading from API", "red");
        Utils.MoveObjectToCamera(rack, GameManager.gm.mainCamera, 1.5f, -0.7f, 90, 0);
        OgreeObject ogree = rack.GetComponent<OgreeObject>();
        ogree.originalLocalRotation = rack.transform.localRotation;
        ogree.originalLocalPosition = rack.transform.localPosition;

        EventManager.Instance.Raise(new ImportFinishedEvent());
    }
    
    ///<summary>
    /// Send picture to API and receive json
    ///</summary>
    ///<param name="_byteArray"> image in the format of a byte array</param>
    public async Task UploadByteAsync(byte[] byteArray)
    {
        WWWForm form = new WWWForm();
        form.AddBinaryData("Label_Rack", byteArray);
        form.AddField("Tenant_Name", customerAndSite);
        form.AddField("deviceType", "rack");
        RequestSentColor();
        apiResponseTMP.gameObject.SetActive(true);
        apiResponseTMP.text = $"Start POST request with url = {currentHost} and site = {customerAndSite}";
        using (UnityWebRequest www = UnityWebRequest.Post($"http://{currentHost}", form))
        {
            www.SendWebRequest();
            while (!www.isDone)
                await Task.Delay(100);
            if (www.result == UnityWebRequest.Result.ConnectionError)
            {
                apiResponseTMP.text = www.error;
                RequestFailColor();
            }

            else
            {
                string text = www.downloadHandler.text;
                Label2 labelReceived = JsonUtility.FromJson<Label2>(text);
                site = labelReceived.site;
                array = labelReceived.label.ToArray();

                if (site == "PCY")
                {
                    room = "UC" + array[1];
                }
                room = array[1];
                if (room.Length > 3)
                    room = room.Substring(3);
                rack = array[2];

                apiResponseTMP.text = $"The label read is {site}{room}-{rack}";
                if (string.IsNullOrEmpty(room) && string.IsNullOrEmpty(rack))
                {
                    RequestFailColor();
                    apiResponseTMP.text = apiResponseTMP.text + "\nCould not read the room and the rack label";
                }
                else if (string.IsNullOrEmpty(room))
                {
                    RequestFailColor();
                    apiResponseTMP.text = apiResponseTMP.text + "\nCould not read the room label";
                }
                else if (string.IsNullOrEmpty(rack))
                {
                    RequestFailColor();
                    apiResponseTMP.text = apiResponseTMP.text + "\nCould not read the rack label";
                }

                else if (!string.IsNullOrEmpty(rack))
                {
                    await SetBuilding(room);
                    if (building == "error" || building == null)
                    {
                        RequestFailColor();
                        GameManager.gm.AppendLogLine(building);
                        apiResponseTMP.text = apiResponseTMP.text + "\nError while getting the parent building of the room";
                    }
                    else
                    {
                        RequestSuccessColor();
                        apiResponseTMP.text = apiResponseTMP.text + "\nLoading Rack please wait...";

                        ButtonPicture.SetActive(false);
                        apiResponseTMP.gameObject.SetActive(false);
                        Dialog myDialog = Dialog.Open(DialogPrefabLarge, DialogButtonType.Confirm | DialogButtonType.Cancel, "Found Rack !", $"Please click on 'Confirm' to place the rack {site}{room}-{rack}.\nClick on 'Cancel' if the label was misread or if you want to take another picture.", true);
                        myDialog.GetComponent<Follow>().MinDistance = 0.5f;
                        myDialog.GetComponent<Follow>().MaxDistance = 0.7f;
                        myDialog.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
                        myDialog.GetComponent<ConstantViewSize>().MinDistance = 0.5f;
                        myDialog.GetComponent<ConstantViewSize>().MinDistance = 0.7f;
                        myDialog.GetComponent<ConstantViewSize>().MinScale = 0.05f;
                        while (myDialog.State != DialogState.Closed)
                        {
                            await Task.Delay(100);
                        }

                        if (myDialog.Result.Result == DialogButtonType.Confirm)
                        {
                            await LoadSingleRack(customer, site, building, room, rack);
                            //apiResponseTMP.text = $"The label read is {site}{room}-{rack}" + "\nRack Loaded in Scene";        
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
                    RequestFailColor();
                }
            }
        }
    }
}
