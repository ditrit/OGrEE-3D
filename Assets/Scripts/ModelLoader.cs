#if TRILIB
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using TriLibCore;
using UnityEngine;
using UnityEngine.Networking;

public class ModelLoader : MonoBehaviour
{
    public static ModelLoader instance;
    private bool isLocked = false;

    private void Awake()
    {
        if (!instance)
            instance = this;
        else
            Destroy(this);
    }

    ///<summary>
    /// Replace the box GameObject by the given fbx model 
    ///</summary>
    ///<param name="_object">The Object to update</param>
    ///<param name="_modelPath">The path of the 3D model to load with TriLib</param>
    public async Task ReplaceBox(GameObject _object, string _modelPath)
    {
        isLocked = true;

        Uri filePath = new($"{GameManager.instance.configHandler.GetCacheDir()}/{_object.name}.fbx");
        await DownloadFile(_modelPath, filePath.AbsolutePath);

        AssetLoaderOptions assetLoaderOptions = AssetLoader.CreateDefaultLoaderOptions();

        // BoxCollider added later
        assetLoaderOptions.GenerateColliders = false;
        assetLoaderOptions.ConvexColliders = false;
        assetLoaderOptions.AlphaMaterialMode = TriLibCore.General.AlphaMaterialMode.None;

        if (File.Exists(filePath.AbsolutePath))
        {
            Debug.Log($"From file: {filePath.AbsolutePath}");
            AssetLoader.LoadModelFromFile(filePath.AbsolutePath, OnLoad, OnMaterialsLoad, OnProgress, OnError,
                                            _object, assetLoaderOptions);
        }
        else
        {
            Debug.Log($"From url: {_modelPath}");
            UnityWebRequest webRequest = AssetDownloader.CreateWebRequest(_modelPath);
            AssetDownloader.LoadModelFromUri(webRequest, OnLoad, OnMaterialsLoad, OnProgress, OnError,
                                                _object, assetLoaderOptions, null, "fbx");
        }
        while (isLocked)
            await Task.Delay(100);
        _object.GetComponent<ObjectDisplayController>().Initialize();
    }

    ///<summary>
    /// Check cache directory size and download fbx file if it's not full.
    ///</summary>
    ///<param name="_url">The url to download the file</param>
    ///<param name="_filePath">The path to write the file</param>
    private async Task DownloadFile(string _url, string _filePath)
    {
        DirectoryInfo info = new(GameManager.instance.configHandler.GetCacheDir());
        float totalSize = 0;
        foreach (FileInfo file in info.EnumerateFiles())
            totalSize += file.Length;
        float sizeMo = totalSize / 1000000;

        if (sizeMo > GameManager.instance.configHandler.config.cacheLimitMo)
        {
            GameManager.instance.AppendLogLine($"Local cache limit reached ({sizeMo}Mo)", ELogTarget.both, ELogtype.warning);
            return;
        }

        if (!File.Exists(_filePath))
        {
            try
            {
                WebClient client = new();
                await client.DownloadFileTaskAsync(_url, _filePath);
            }
            catch (Exception _e)
            {
                GameManager.instance.AppendLogLine($"Error while downloading file at \"{_url}\": {_e.Message}", ELogTarget.both, ELogtype.error);
                File.Delete(_filePath);
            }
        }
        else
            Debug.Log("Template is already in cache");
    }

    // This event is called when the model loading progress changes.
    // You can use this event to update a loading progress-bar, for instance.
    // The "progress" value comes as a normalized float (goes from 0 to 1).
    // Platforms like UWP and WebGL don't call this method at this moment, since they don't use threads.
    private void OnProgress(AssetLoaderContext assetLoaderContext, float progress)
    {

    }

    // This event is called when there is any critical error loading your model.
    // You can use this to show a message to the user.
    private void OnError(IContextualizedError contextualizedError)
    {
        Debug.LogError("TriLib Error: " + contextualizedError);
        isLocked = false;
    }

    // This event is called when all model GameObjects and Meshes have been loaded.
    // There may still be Materials and Textures processing at this stage.
    private void OnLoad(AssetLoaderContext assetLoaderContext)
    {
        Transform triLibWrapper = assetLoaderContext.RootGameObject.transform;
        Transform triLibObj = triLibWrapper.GetChild(0);

        // Get the basic box' scale 
        Vector3 boxScale = triLibWrapper.parent.GetChild(0).localScale;

        triLibWrapper.name = $"Wraper_{triLibObj.name}";
        triLibWrapper.localPosition = Vector3.zero;

        // switch trilibWrapper & triLibObj in hierarchy to fit with objects requirements 
        triLibObj.parent = triLibWrapper.parent;
        triLibWrapper.parent = triLibObj;
        triLibObj.SetAsFirstSibling();
        triLibObj.localPosition = boxScale / 2;
        triLibWrapper.localPosition = Vector3.zero;

        triLibObj.gameObject.AddComponent<BoxCollider>();
        triLibObj.tag = "Selectable";
    }

    // This event is called after OnLoad when all Materials and Textures have been loaded.
    // This event is also called after a critical loading error, so you can clean up any resource you want to.
    private void OnMaterialsLoad(AssetLoaderContext assetLoaderContext)
    {
        // Destroy basic box
        Destroy(assetLoaderContext.WrapperGameObject.transform.GetChild(1).gameObject);
        isLocked = false;
    }
}
#endif
