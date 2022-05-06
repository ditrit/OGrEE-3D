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
    public string telephoneHost = "192.168.228.231";
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
                UploadByteAsync(imageByteArray);
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
        await ApiManager.instance.GetObject($"sites/"+ _site);
        await ApiManager.instance.GetObject($"tenants/" + _customer + "/sites/" + _site + "/buildings/" +_building);
        await ApiManager.instance.GetObject($"tenants/" + _customer + "/sites/" + _site + "/buildings/" + _building + "/rooms/" + _room);
        await ApiManager.instance.GetObject($"tenants/" + _customer + "/sites/" + _site + "/buildings/" + _building + "/rooms/" + _room + "/racks/" + _rack);
        GameObject rack = GameManager.gm.FindByAbsPath(_customer + "." + _site + "." + _building + "." + _room + "." + _rack);
        if (rack != null)
            GameManager.gm.AppendLogLine("Rack Found in the scene after loading from API", "green");
        else 
            GameManager.gm.AppendLogLine("Rack NOT Found in the scene after loading from API", "red");
        Utils.MoveObjectToCamera(rack, GameManager.gm.m_camera);
        await rack.GetComponent<OgreeObject>().LoadChildren("3");
        EventManager.Instance.Raise(new ImportFinishedEvent());
    }

    ///<summary>
    /// Send picture to API and receive json
    ///</summary>
    ///<param name="_byteArray"> image in the format of a byte array</param>
    public async void UploadByteAsync(byte[] byteArray)
    {
        WWWForm form = new WWWForm();
        form.AddBinaryData("Label_Rack", byteArray);
        form.AddField("Tenant_Name", customerAndSite);
        RequestSentColor();
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
                        Dialog myDialog = Dialog.Open(DialogPrefabLarge, DialogButtonType.Confirm | DialogButtonType.Cancel, "Rack Trouvé !", $"Cliquez sur 'Confirm' pour placer le rack {site}{room}-{rack} devant vous.'\n'Cliquez sur 'Cancel' si l'étiquette a mal été lue et que vous souhaitez reprendre une photo.", true);
                        myDialog.GetComponent<ConstantViewSize>().enabled = false;
                        while (myDialog.State != DialogState.Closed)
                        {
                            await Task.Delay(100);
                        }

                        if (myDialog.Result.Result == DialogButtonType.Confirm)
                        {
                            await LoadSingleRack(customer, site, building, room, rack);
                            apiResponseTMP.text = $"The label read is {site}{room}-{rack}" + "\nRack Loaded in Scene";        
                        }

                        if (myDialog.Result.Result == DialogButtonType.Cancel)
                        {
                            ButtonPicture.SetActive(true);    
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
