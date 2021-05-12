using System;
using System.Collections;
using UnityEngine;

namespace TriLibCore
{
    /// <summary>
    /// Represents a class that forces GC collection using a fixed interval.
    /// </summary>
    public class GCHelper : MonoBehaviour
    {
        /// <summary>
        /// The interval to do the GC Collection.
        /// </summary>
        public float Interval = 1f;

        private int _loadingCount;

        private static GCHelper _instance;

        /// <summary>
        /// Gets the GCHelper instance and setup the given internal.
        /// </summary>
        /// <returns>The GCHelper singleton instance.</returns>
        public static GCHelper GetInstance()
        {
            if (!Application.isPlaying)
            {
                return null;
            }
            if (_instance == null)
            {
                _instance = new GameObject("TriLibGCHelper").AddComponent<GCHelper>();
            }
            return _instance;
        }

        /// <summary>
        /// Starts the CollectGC Coroutine.
        /// </summary>
        private void Start()
        {
            StartCoroutine(CollectGC());
        }

        /// <summary>
        /// If there is any model loading, does the GC collection.
        /// </summary>
        /// <returns>The Coroutine IEnumerator.</returns>
        private IEnumerator CollectGC()
        {
            if (!Application.isPlaying)
            {
                Destroy(_instance.gameObject);
                yield break;
            }
            while (true)
            {
                if (_loadingCount > 0)
                {
                    yield return new WaitForSeconds(Interval);
                    GC.Collect();
                }
                yield return null;
            }
        }

        /// <summary>
        /// Waits the interval and decreases the model loading count.
        /// </summary>
        /// <param name="interval">Interval used to decrease the model loading counter.</param>
        /// <returns>The Coroutine IEnumerator.</returns>
        private IEnumerator RemoveInstanceInternal(float interval)
        {
            yield return new WaitForSeconds(interval);
            _loadingCount = Mathf.Max(0, _loadingCount-1);
        }

        /// <summary>
        /// Indicates a new model is loading.
        /// </summary>
        public void RegisterLoading()
        {
            _loadingCount++;
        }

        /// <summary>
        /// Indicates a model has finished loading or an error occurred.
        /// </summary>
        /// <param name="interval">Interval used to decrease the model loading counter.</param>
        public void UnRegisterLoading(float interval)
        {
            StartCoroutine(RemoveInstanceInternal(interval));
        }
    }
}