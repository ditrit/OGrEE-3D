using System;
using System.Collections;
using System.IO;
using TriLibCore.General;
using TriLibCore.Utils;
using UnityEngine;
using UnityEngine.Networking;

namespace TriLibCore
{
    /// <summary>Represents a class used to download Models with Coroutines used by the Asset Downloader.</summary>
    public class AssetDownloaderBehaviour : MonoBehaviour
    {
        /// <summary>
        /// Unity web request instance used on this script.
        /// </summary>
        private UnityWebRequest _unityWebRequest;

        /// <summary>
        /// Method to call when the model downloading progress changes.
        /// </summary>
        private Action<AssetLoaderContext, float> _onProgress;

        /// <summary>
        /// Context used to load the model.
        /// </summary>
        private AssetLoaderContext _assetLoaderContext;

        /// <summary>Downloads the Model using the given Request and options.</summary>
        /// <param name="unityWebRequest">The Unity Web Request used to load the Model. You can use the CreateWebRequest method to create a new Unity Web Request or pass your instance.</param>
        /// <param name="onLoad">The Method to call on the Main Thread when the Model is loaded but resources may still pending.</param>
        /// <param name="onMaterialsLoad">The Method to call on the Main Thread when the Model and resources are loaded.</param>
        /// <param name="onProgress">The Method to call when the Model loading progress changes.</param>
        /// <param name="wrapperGameObject">The Game Object that will be the parent of the loaded Game Object. Can be null.</param>
        /// <param name="onError">The Method to call on the Main Thread when any error occurs.</param>
        /// <param name="assetLoaderOptions">The options to use when loading the Model.</param>
        /// <param name="customContextData">The Custom Data that will be passed along the Context.</param>
        /// <param name="fileExtension">The extension of the URI Model.</param>
        /// <param name="isZipFile">Pass <c>true</c> if your file is a Zip file.</param>
        /// <returns>The download coroutine enumerator.</returns>
        public IEnumerator DownloadAsset(UnityWebRequest unityWebRequest, Action<AssetLoaderContext> onLoad, Action<AssetLoaderContext> onMaterialsLoad, Action<AssetLoaderContext, float> onProgress, GameObject wrapperGameObject, Action<IContextualizedError> onError, AssetLoaderOptions assetLoaderOptions, object customContextData, string fileExtension, bool? isZipFile = null)
        {
            _unityWebRequest = unityWebRequest;
            _onProgress = onProgress;
            yield return unityWebRequest.SendWebRequest();
            try
            {
                if (unityWebRequest.responseCode < 400)
                {
                    var memoryStream = new MemoryStream(_unityWebRequest.downloadHandler.data);
                    var uriLoadCustomContextData = new UriLoadCustomContextData
                    {
                        UnityWebRequest = _unityWebRequest,
                        CustomData = customContextData
                    };
                    var contentType = unityWebRequest.GetResponseHeader("Content-Type");
                    if (contentType != null && isZipFile == null)
                    {
                        isZipFile = contentType.Contains("application/zip") || contentType.Contains("application/x-zip-compressed") || contentType.Contains("multipart/x-zip");
                    }
                    if (!isZipFile.GetValueOrDefault() && string.IsNullOrWhiteSpace(fileExtension))
                    {
                        fileExtension = FileUtils.GetFileExtension(unityWebRequest.url);
                    }
                    if (isZipFile.GetValueOrDefault())
                    {
                        _assetLoaderContext = AssetLoaderZip.LoadModelFromZipStream(memoryStream, onLoad, onMaterialsLoad, delegate (AssetLoaderContext assetLoaderContext, float progress) { onProgress?.Invoke(assetLoaderContext, 0.5f + progress * 0.5f); }, onError, wrapperGameObject, assetLoaderOptions, uriLoadCustomContextData, fileExtension);
                    }
                    else
                    {
                        _assetLoaderContext = AssetLoader.LoadModelFromStream(memoryStream, null, fileExtension, onLoad, onMaterialsLoad, delegate (AssetLoaderContext assetLoaderContext, float progress) { onProgress?.Invoke(assetLoaderContext, 0.5f + progress * 0.5f); }, onError, wrapperGameObject, assetLoaderOptions, uriLoadCustomContextData);
                    }
                }
                else
                {
                    var exception = new Exception($"UnityWebRequest error:{unityWebRequest.error}, code:{unityWebRequest.responseCode}");
                    throw exception;
                }
            }
            catch (Exception exception)
            {
                if (onError != null)
                {
                    var contextualizedError = exception as IContextualizedError;
                    onError(contextualizedError ?? new ContextualizedError<AssetLoaderContext>(exception, null));
                }
                else
                {
                    throw;
                }
            }
            Destroy(gameObject);
        }

        /// <summary>Updates the download progress.</summary>
        private void Update()
        {
            _onProgress?.Invoke(_assetLoaderContext, _unityWebRequest.downloadProgress * 0.5F);
        }
    }
}