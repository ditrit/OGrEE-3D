using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateServer : MonoBehaviour
{
    [SerializeField] private GameObject model = null;
    public int nbOfComponent;

    private void Start()
    {
        for (int i = 0; i < nbOfComponent; i++)
            Instantiate(model, Vector3.one * 0.01f, Quaternion.identity, transform);
    }

}
