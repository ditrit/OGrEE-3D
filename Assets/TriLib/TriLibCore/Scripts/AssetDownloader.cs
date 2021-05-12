using System;
using System.Threading;
using TriLibCore.General;
using UnityEngine;
using UnityEngine.Networking;

namespace TriLibCore
{
    /// <summary>Represents a class to download and load Models.</summary>
    public class AssetDownloader
    {
        /// <summary>Represents an HTTP Request method.</summary>
        public enum HttpRequestMethod
        {
            /// <summary>The HTTP GET method.</summary>
            Get,
            /// <summary>The HTTP POST method.</summary>
            Post,
            /// <summary>The HTTP PUT method.</summary>
            Put,
            /// <summary>The HTTP DELETE method.</summary>
            Delete,
            /// <summary>The HTTP HEAD method.</summary>
            Head
        }

        /// <summary>Creates a Unity Web Request from the given parameters.</summary>
        /// <param name="uri">The Request URI (URL).</param>
        /// <param name="httpRequestMethod">The HTTP Request method to use.</param>
        /// <param name="data">The Custom Data that was sent along the Request.</param>
        /// <param name="timeout">The Request timeout in seconds).</param>
        /// <returns>The created unity web request.</returns>
        public static UnityWebRequest CreateWebRequest(string uri, HttpRequestMethod httpRequestMethod = HttpRequestMethod.Get, string data = null, int timeout = 2000)
        {
            UnityWebRequest unityWebRequest;
            switch (httpRequestMethod)
            {
                case HttpRequestMethod.Post:
                    unityWebRequest = UnityWebRequest.Post(uri, data);
                    break;
                case HttpRequestMethod.Put:
                    unityWebRequest = UnityWebRequest.Put(uri, data);
                    break;
                case HttpRequestMethod.Delete:
                    unityWebRequest = UnityWebRequest.Delete($"{uri}?{data}");
                    break;
                case HttpRequestMethod.Head:
                    unityWebRequest = UnityWebRequest.Head($"{uri}?{data}");
                    break;
                default:
                    unityWebRequest = UnityWebRequest.Get($"{uri}?{data}");
                    break;
            }
            unityWebRequest.timeout = timeout;
            return unityWebRequest;
        }

        /// <summary>Loads a Model from the given URI Asynchronously (Accepts zip files).</summary>
        /// <param name="unityWebRequest">The Unity Web Request used to load the Model. You can use the CreateWebRequest method to create a new Unity Web Request or pass your instance.</param>
        /// <param name="onLoad">The Method to call on the Main Thread when the Model is loaded but resources may still pending.</param>
        /// <param name="onMaterialsLoad">The Method to call on the Main Thread when the Model and resources are loaded.</param>
        /// <param name="onProgress">The Method to call when the Model loading progress changes.</param>
        /// <param name="onError">The Method to call on the Main Thread when any error occurs.</param>
        /// <param name="wrapperGameObject">The Game Object that will be the parent of the loaded Game Object. Can be null.</param>
        /// <param name="assetLoaderOptions">The options to use when loading the Model.</param>
        /// <param name="customContextData">The Custom Data that will be passed along the Context.</param>
        /// <param name="fileExtension">The extension of the URI Model or the Model inside the Zip file.</param>
        /// <param name="isZipFile">Pass <c>true</c> if your file is a Zip file.</param>
        /// <returns>The AssetLoaderContext used to load the model.</returns>
        public static Coroutine LoadModelFromUri(UnityWebRequest unityWebRequest, Action<AssetLoaderContext> onLoad, Action<AssetLoaderContext> onMaterialsLoad, Action<AssetLoaderContext, float> onProgress, Action<IContextualizedError> onError = null, GameObject wrapperGameObject = null, AssetLoaderOptions assetLoaderOptions = null, object customContextData = null, string fileExtension = null, bool? isZipFile = null)
        {
            var assetDownloader = new GameObject("Asset Downloader").AddComponent<AssetDownloaderBehaviour>();
            return assetDownloader.StartCoroutine(assetDownloader.DownloadAsset(unityWebRequest, onLoad, onMaterialsLoad, onProgress, wrapperGameObject, onError, assetLoaderOptions, customContextData, fileExtension, isZipFile));
        }
    }
}