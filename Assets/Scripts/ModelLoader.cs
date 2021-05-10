using System.Collections;
using System.Collections.Generic;
using TriLibCore;
using UnityEngine;
using UnityEngine.Networking;

public class ModelLoader : MonoBehaviour
{
    public static ModelLoader instance;

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
    public void ReplaceBox(GameObject _object, string _modelPath)
    {
        Destroy(_object.transform.GetChild(0).gameObject);

        AssetLoaderOptions assetLoaderOptions = AssetLoader.CreateDefaultLoaderOptions();
        assetLoaderOptions.ScaleFactor = 0.1f;
        // BoxCollider added later
        assetLoaderOptions.GenerateColliders = false;
        assetLoaderOptions.ConvexColliders = false;

        UnityWebRequest webRequest = AssetDownloader.CreateWebRequest(_modelPath);
        AssetDownloader.LoadModelFromUri(webRequest, OnLoad, OnMaterialsLoad, OnProgress, OnError,
                                            _object, assetLoaderOptions, null, "fbx");

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
        Debug.Log("TriLib Error: " + contextualizedError);
    }

    // This event is called when all model GameObjects and Meshes have been loaded.
    // There may still Materials and Textures processing at this stage.
    private void OnLoad(AssetLoaderContext assetLoaderContext)
    {
        Transform triLibWrapper = assetLoaderContext.RootGameObject.transform;
        Transform triLibObj = triLibWrapper.GetChild(0);

        triLibWrapper.name = $"Wraper_{triLibObj.name}";
        triLibWrapper.localPosition = Vector3.zero;

        // switch trilibWrapper & triLibObj in hierarchy to fit with objects requirements 
        triLibObj.parent = triLibWrapper.parent;
        triLibWrapper.parent = triLibObj;
        triLibObj.SetAsFirstSibling();
        triLibObj.localPosition = Vector3.zero;
        triLibWrapper.localPosition = Vector3.zero;

        cakeslice.Outline ol = triLibObj.gameObject.AddComponent<cakeslice.Outline>();
        ol.enabled = false;
        triLibObj.gameObject.AddComponent<BoxCollider>();

        triLibObj.tag = "Selectable";

    }

    // This event is called after OnLoad when all Materials and Textures have been loaded.
    // This event is also called after a critical loading error, so you can clean up any resource you want to.
    private void OnMaterialsLoad(AssetLoaderContext assetLoaderContext)
    {

    }

}
