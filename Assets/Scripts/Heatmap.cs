// Alan Zucconi
// www.alanzucconi.com
using System;
using UnityEngine;

public class Heatmap : MonoBehaviour
{
    public Vector4[] positions;
    public Vector4[] properties;
    public Material material;
    public Vector4[] adjustedPositions;
    private int count;
    void Awake()
    {
        count = TempDiagram.instance.heatMapSensorsMaxNumber;
        positions = new Vector4[count];
        adjustedPositions = new Vector4[count];
        properties = new Vector4[count];// x = radius, y = intensity
        //for (int i = 0; i < 10; i++)
        //{
        //    for (int j = 0; j < 10; j++)
        //    {
        //        properties[i * 10 + j] = new Vector4(0.05f, 1, 0, 0); ;//new Vector4(Random.Range(0f, 0.25f), Random.Range(-0.25f, 1f), 0, 0); //
        //        positions[i * 10 + j] = new Vector4(((float)i / 10 - 0.45f), ((float)j / 10 - 0.45f), 0, 0);
        //    }
        //}
    }

    void Update()
    {
        if (!transform.parent.hasChanged && false)
            return;
        float minDimension = Mathf.Min(transform.parent.localScale.x, transform.parent.localScale.y, transform.parent.localScale.z);
        if (minDimension == transform.parent.localScale.y)
            transform.SetPositionAndRotation(transform.parent.position + (transform.parent.localScale.y / 2 + 0.01f) * (transform.parent.rotation * Vector3.up), transform.parent.rotation * Quaternion.Euler(90, 0, 0));
        else if (minDimension == transform.parent.localScale.z)
            transform.SetPositionAndRotation(transform.parent.position + (transform.parent.localScale.z / 2 + 0.01f) * (transform.parent.rotation * Vector3.back), transform.parent.rotation * Quaternion.Euler(0, 0, 0));
        else
            transform.SetPositionAndRotation(transform.parent.position + (transform.parent.localScale.x / 2 + 0.01f) * (transform.parent.rotation * Vector3.left), transform.parent.rotation * Quaternion.Euler(0, 90, 0));

        for (int i = 0; i < count; i++)
            adjustedPositions[i] = HandleTransform(positions[i]);

        material.SetInt("_Points_Length", count);
        material.SetVectorArray("_Points", adjustedPositions);
        material.SetVectorArray("_Properties", properties);
        transform.parent.hasChanged = false;
    }

    private Vector4 HandleTransform(Vector4 _point)
    {
        Vector3 pointTemp = new Vector3(_point.x, _point.y, _point.z);
        pointTemp.Scale(transform.lossyScale);
        pointTemp = transform.rotation * pointTemp;
        pointTemp += transform.position;

        return new Vector4(pointTemp.x, pointTemp.y, pointTemp.z, 0);
    }

    public void SetPositionsAndProperties(Vector4[] _positions, Vector4[] _properties)
    {
        try
        {
            Array.Copy(_positions, positions, _positions.Length);
            Array.Copy(_properties, properties, _properties.Length);
        }
        catch (ArgumentException e)
        {
            print(e);
            GameManager.gm.AppendLogLine($"Number of sensors should be equal or less than {count}, {_positions.Length - count} of them have been ignored", true, eLogtype.warning);
            Array.Copy(_positions, positions, count);
            Array.Copy(_properties, properties, count);
        }
    }
}
