using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    ///<summary>
    /// Load a scene of given name.
    ///</summary>
    public void LoadScene(string _name)
    {
        SceneManager.LoadScene(_name);
    }
}
