using System;
using UnityEngine;

// from Alan Zucconi : www.alanzucconi.com
public class Heatmap : MonoBehaviour
{
    public Vector4[] positions;
    public Vector4[] properties;
    public Material material;
    public Vector4[] adjustedPositions;
    private int count;

    private void Awake()
    {
        count = TempDiagram.instance.heatMapSensorsMaxNumber;
        positions = new Vector4[count];
        adjustedPositions = new Vector4[count];
        properties = new Vector4[count];        
    }

    private void Update()
    {
        if (transform.parent.hasChanged)
        {
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
    }

    /// <summary>
    /// Apply the object's scale, position and rotation to a heatmap point
    /// </summary>
    /// <param name="_point">the heatmap point to modify</param>
    /// <returns>the modified point</returns>
    private Vector4 HandleTransform(Vector4 _point)
    {
        Vector3 pointTemp = new Vector3(_point.x, _point.y, _point.z);
        pointTemp.Scale(transform.lossyScale);
        pointTemp = transform.rotation * pointTemp;
        pointTemp += transform.position;

        return new(pointTemp.x, pointTemp.y, pointTemp.z, 0);
    }

    /// <summary>
    /// Set the positions and properties(radius,intensity) of the heatmap points <br></br> Arguments needs to be Vector4 to be passed to a shader
    /// </summary>
    /// <param name="_positions">position of the point (last digits is not used)</param>
    /// <param name="_properties">(x = radius, y = intensity, z not used, w not used)</param>
    public void SetPositionsAndProperties(Vector4[] _positions, Vector4[] _properties)
    {
        try
        {
            Array.Copy(_positions, positions, _positions.Length);
            Array.Copy(_properties, properties, _properties.Length);
        }
        catch (ArgumentException)
        {
            GameManager.instance.AppendLogLine($"Number of sensors should be equal or less than {count}, {_positions.Length - count} of them have been ignored", ELogTarget.both, ELogtype.warning);
            Array.Copy(_positions, positions, count);
            Array.Copy(_properties, properties, count);
        }
    }
}
