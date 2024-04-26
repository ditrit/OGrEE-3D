using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

[System.Serializable]
public class ClearanceHandler
{
    [System.Serializable]
    private struct SClearance
    {
        public float length;
        public Vector3 direction;
    }

    [SerializeField] private SClearance front = new();
    [SerializeField] private SClearance rear = new();
    [SerializeField] private SClearance left = new();
    [SerializeField] private SClearance right = new();
    [SerializeField] private SClearance top = new();
    [SerializeField] private SClearance bottom = new();
    [SerializeField] private Transform clearedObject;

    private bool isCreated = false;
    public bool isToggled = false;
    public bool isInitialized = false;
    public GameObject clearanceWrapper;
    [SerializeField] private List<SClearance> clearances;

    /// <summary>
    /// Initialize the clearance of the object
    /// </summary>
    /// <param name="_front">front clearance length</param>
    /// <param name="_rear">rear clearance length</param>
    /// <param name="_left">left clearance length</param>
    /// <param name="_right">right clearance length</param>
    /// <param name="_top">top clearance length</param>
    /// <param name="_clearedObject">object which has the clearance</param>
    public void Initialize(float _front, float _rear, float _left, float _right, float _top, float _bottom, Transform _clearedObject)
    {
        if (_front == 0 && _rear == 0 && _left == 0 && _right == 0 && _top == 0 && _bottom == 0)
            return;
        clearances = new List<SClearance>();
        Object.Destroy(clearanceWrapper);
        front.length = _front / 1000;
        front.direction = Vector3.forward;
        rear.length = _rear / 1000;
        rear.direction = Vector3.back;
        left.length = _left / 1000;
        left.direction = Vector3.left;
        right.length = _right / 1000;
        right.direction = Vector3.right;
        top.length = _top / 1000;
        top.direction = Vector3.up;
        bottom.length = _bottom / 1000;
        bottom.direction = Vector3.down;
        clearedObject = _clearedObject;
        if (isToggled)
            BuildClearance();
        else
            isCreated = false;
        isInitialized = true;
    }

    /// <summary>
    /// Display the clearance according to the boolean, call <see cref="BuildClearance"/> if <see cref="isCreated"/> is false
    /// </summary>
    /// <param name="_toggle">if the clearance is displayed or not</param>
    public void ToggleClearance(bool _toggle)
    {
        isToggled = _toggle;
        if (!isCreated)
        {
            if (_toggle)
                BuildClearance();
            return;
        }
        clearanceWrapper.transform.GetChild(0).gameObject.SetActive(_toggle);
        GameManager.instance.AppendLogLine(new ExtendedLocalizedString("Logs", $"{(_toggle ? "Display" : "Hide")} clearance for object", clearedObject.name), ELogTarget.logger, ELogtype.success);
    }

    /// <summary>
    /// Toggle the clearance
    /// </summary>
    public void ToggleClearance()
    {
        ToggleClearance(!isToggled);
    }

    /// <summary>
    /// Build each side of the clearance which is greater than 0
    /// </summary>
    public void BuildClearance()
    {
        if (!clearedObject)
            return;
        if (front.length > 0)
            clearances.Add(front);
        if (rear.length > 0)
            clearances.Add(rear);
        if (left.length > 0)
            clearances.Add(left);
        if (right.length > 0)
            clearances.Add(right);
        if (top.length > 0)
            clearances.Add(top);
        if (bottom.length > 0)
            clearances.Add(bottom);
        clearanceWrapper = new GameObject("Clearance Wrapper");
        clearanceWrapper.transform.parent = clearedObject;
        Item item = clearedObject.GetComponent<Item>();



        // Apply scale and move all components to have the rack's pivot at the lower left corner
        Vector2 size = ((JArray)item.attributes["size"]).ToVector2();
        float height = (float)item.attributes["height"];
        height *= item.attributes["heightUnit"] switch
        {
            LengthUnit.U => UnitValue.U,
            LengthUnit.OU => UnitValue.OU,
            LengthUnit.Centimeter => 0.01f,
            LengthUnit.Millimeter => 0.001f,
            _ => height
        };
        if ((string)item.attributes["sizeUnit"] == LengthUnit.Millimeter)
            size /= 10;

        clearanceWrapper.transform.SetLocalPositionAndRotation(clearedObject.GetChild(0).localPosition, Quaternion.identity);
        Transform clearanceObject = Object.Instantiate(GameManager.instance.clearanceModel, clearanceWrapper.transform).transform;
        clearanceObject.transform.localScale = new Vector3(size.x / 100, height, size.y / 100);
        clearanceObject.GetChild(0).GetComponent<ClearanceCollisionHandler>().ownObject = clearedObject;
        foreach (SClearance clearance in clearances)
        {
            clearanceObject.localPosition += 0.5f * clearance.length * clearance.direction;
            clearanceObject.localScale += clearance.length * Vector3.Scale(clearance.direction, clearance.direction);
        }
        isCreated = true;
        GameManager.instance.AppendLogLine(new ExtendedLocalizedString("Logs", "Display clearance for object", clearedObject.name), ELogTarget.logger, ELogtype.success);
    }
}
