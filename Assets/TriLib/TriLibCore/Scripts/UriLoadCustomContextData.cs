using UnityEngine.Networking;

namespace TriLibCore
{
    /// <summary>Represents a class passed as the custom data to the Asset Loader Context when loading Models from URIs (Network).</summary>
    public class UriLoadCustomContextData
    {
        /// <summary>
        /// The unity web request used to load the models.
        /// </summary>
        public UnityWebRequest UnityWebRequest;

        /// <summary>
        /// The optional custom data used to load the models.
        /// </summary>
        public object CustomData;
    }
}