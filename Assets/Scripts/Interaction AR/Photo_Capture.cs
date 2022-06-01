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

public class Label
{
    public string site;
    public string room;
    public string rack;
}

public class Photo_Capture : MonoBehaviour
{
    public static Photo_Capture instance;
    [SerializeField]
    [Tooltip("Assign DialogLarge_192x192.prefab")]
    private GameObject dialogPrefabLarge;

    /// <summary>
    /// Large Dialog example prefab to display
    /// </summary>
    public GameObject DialogPrefabLarge
    {
        get => dialogPrefabLarge;
        set => dialogPrefabLarge = value;
    }
    private string currentHost = "";
    public string maisonHost = "192.168.1.38";
    public string telephoneHost = "192.168.229.231";
    public string customer = "EDF";
    public string site = "NOE";
    private string building;
    private string room;
    private string rack;
    private string customerAndSite;
    public string port = "5000";
    public GameObject ButtonPicture;
    public GameObject quadButtonNoe;
    public GameObject quadButtonPcy;
    public GameObject quadButtonMaison;
    public GameObject quadButtonTelephone;
    public GameObject quadButtonPhoto;
    public Material red;
    public Material green;
    public Material yellow;
    private PhotoCapture photoCaptureObject = null;
    public TextMeshPro apiResponseTMP = null;

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
        SetHostTelephone();
        if (currentHost == maisonHost)
        {
            SetHostMaison();
        }

        if (currentHost == telephoneHost)
        {
            SetHostTelephone();
        }

        if (customerAndSite == "EDF.PCY")
        {
            SetSitePcy();
        }

        if (customerAndSite == "EDF.NOE")
        {
            SetSiteNoe();
        }
    }


    public void SetHostMaison()
    {
        currentHost = maisonHost;
        quadButtonMaison.GetComponent<Renderer>().material = green;
        quadButtonTelephone.GetComponent<Renderer>().material = red;
    }

    public void SetHostTelephone()
    {
        currentHost = telephoneHost;
        quadButtonMaison.GetComponent<Renderer>().material = red;
        quadButtonTelephone.GetComponent<Renderer>().material = green;
    }

    public void SetSiteNoe()
    {
        customerAndSite = "EDF.NOE";
        quadButtonNoe.GetComponent<Renderer>().material = green;
        quadButtonPcy.GetComponent<Renderer>().material = red;
    }

    public void SetSitePcy()
    {
        customerAndSite = "EDF.PCY";
        quadButtonNoe.GetComponent<Renderer>().material = red;
        quadButtonPcy.GetComponent<Renderer>().material = green;
    }

    public void RequestSentColor()
    {
        quadButtonPhoto.GetComponent<Renderer>().material = yellow;
    }

    public void RequestFailColor()
    {
        quadButtonPhoto.GetComponent<Renderer>().material = red;
    }

    public void RequestSuccessColor()
    {
        quadButtonPhoto.GetComponent<Renderer>().material = green;
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
        string data = await ApiManager.instance.GetObjectParentId($"rooms?name=" + _room);
        building = await ApiManager.instance.GetObjectName($"buildings/" + data);
    }

    private void OnClosedDialogEvent(DialogResult _obj)
    {

        if (_obj.Result == DialogButtonType.Confirm)
        {
            
        }
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
    /// Load the 3D model of an OObject
    ///</summary>
    ///<param name="_fullname">full hierarchy name of the object</param>
    public async Task LoadOObject(string _fullname)
    {

        string[] splittedName = Utils.SplitHierarchyName(_fullname);
        GameObject oObject;
        switch (splittedName.Length)
        {
            case 0:
                throw new System.Exception("fullname is empty or not formatted : " + _fullname);
            case 1:
                await ApiManager.instance.GetObject($"tenants/" + splittedName[0]);
                oObject = GameManager.gm.FindByAbsPath(splittedName[0]);
                break;
            case 2:
                await ApiManager.instance.GetObject($"tenants/" + splittedName[0]);
                await ApiManager.instance.GetObject($"tenants/" + splittedName[0] + "/sites/" + splittedName[1]);
                oObject = GameManager.gm.FindByAbsPath(splittedName[0] + "." + splittedName[1]);
                break;
            case 3:
                await ApiManager.instance.GetObject($"tenants/" + splittedName[0]);
                await ApiManager.instance.GetObject($"tenants/" + splittedName[0] + "/sites/" + splittedName[1]);
                await ApiManager.instance.GetObject($"tenants/" + splittedName[0] + "/sites/" + splittedName[1] + "/buildings/" + splittedName[2]);
                oObject = GameManager.gm.FindByAbsPath(splittedName[0] + "." + splittedName[1] + "." + splittedName[2]);
                break;

            case 4:
                await ApiManager.instance.GetObject($"tenants/" + splittedName[0]);
                await ApiManager.instance.GetObject($"tenants/" + splittedName[0] + "/sites/" + splittedName[1]);
                await ApiManager.instance.GetObject($"tenants/" + splittedName[0] + "/sites/" + splittedName[1] + "/buildings/" + splittedName[2]);
                await ApiManager.instance.GetObject($"tenants/" + splittedName[0] + "/sites/" + splittedName[1] + "/buildings/" + splittedName[2] + "/rooms/" + splittedName[3]);
                oObject = GameManager.gm.FindByAbsPath(splittedName[0] + "." + splittedName[1] + "." + splittedName[2] + "." + splittedName[3]);
                break;
            case 5:
                await ApiManager.instance.GetObject($"tenants/" + splittedName[0]);
                await ApiManager.instance.GetObject($"tenants/" + splittedName[0] + "/sites/" + splittedName[1]);
                await ApiManager.instance.GetObject($"tenants/" + splittedName[0] + "/sites/" + splittedName[1] + "/buildings/" + splittedName[2]);
                await ApiManager.instance.GetObject($"tenants/" + splittedName[0] + "/sites/" + splittedName[1] + "/buildings/" + splittedName[2] + "/rooms/" + splittedName[3]);
                await ApiManager.instance.GetObject($"tenants/" + splittedName[0] + "/sites/" + splittedName[1] + "/buildings/" + splittedName[2] + "/rooms/" + splittedName[3] + "/racks/" + splittedName[4]);
                oObject = GameManager.gm.FindByAbsPath(splittedName[0] + "." + splittedName[1] + "." + splittedName[2] + "." + splittedName[3] + "." + splittedName[4]);
                break;
            default:
                throw new System.Exception("fullname is empty or not formatted : " + _fullname);

        }

        if (oObject != null)
            GameManager.gm.AppendLogLine("OObject Found in the scene after loading from API", "green");
        else 
            GameManager.gm.AppendLogLine("OObject NOT Found in the scene after loading from API", "red");
        //Utils.MoveObjectToCamera(rack, GameManager.gm.m_camera);
        /*rack.AddComponent<TapToPlace>();
        rack.GetComponent<TapToPlace>().AutoStart = true;
        rack.GetComponent<TapToPlace>().KeepOrientationVertical = true;
        rack.GetComponent<TapToPlace>().MagneticSurfaces[0] = LayerMask.GetMask("Nothing");
        rack.GetComponent<SolverHandler>().AdditionalOffset = new Vector3(0, -0.2f, 0);*/
        OgreeObject ogree = oObject.GetComponent<OgreeObject>();
        ogree.originalLocalRotation = oObject.transform.localRotation;
        ogree.originalLocalPosition = oObject.transform.localPosition;

        await ogree.LoadChildren((5 - splittedName.Length).ToString());
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
        RequestSentColor();
        apiResponseTMP.gameObject.SetActive(true);
        apiResponseTMP.text = $"Start POST request with url = {currentHost} and site = {customerAndSite}";
        using (UnityWebRequest www = UnityWebRequest.Post($"http://{currentHost}:{port}", form))
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
                            await LoadOObject($"{customer}.{site}.{building}.{room}.{rack}");
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
