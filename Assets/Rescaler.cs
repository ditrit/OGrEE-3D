using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;

public class Rescaler : MonoBehaviour
{
    public string position;
    public Vector3 initialPosition;
    private Quaternion initialRotation;
    [SerializeField]
    private float scale;
    private OgreeObject oObject;
    private void Start()
    {
        oObject = transform.parent.GetComponent<OgreeObject>();
        initialPosition = oObject.transform.localPosition;
        position = oObject.attributes["posXYZ"];
        EventManager.instance.PositionMode.Add(OnPositionMode);
    }
    void Update()
    {
        transform.localScale = scale * Vector3.Distance(transform.position, Camera.main.transform.position) * Vector3.one;
    }

    public void OnPositionMode(PositionModeEvent _e)
    {
        SavePosition();
        Destroy(gameObject);
    }

    public void OnDestroy()
    {
        EventManager.instance.PositionMode.Remove(OnPositionMode);
    }

    public async void SavePosition()
    {
        Room parentRoom = oObject.transform.parent.GetComponent<Room>();
        Vector2 orient = parentRoom.attributes["axisOrientation"] switch
        {
            AxisOrientation.XMinus => new(-1, 1),
            AxisOrientation.YMinus => new(1, -1),
            AxisOrientation.BothMinus => new(-1, -1),
            _ => new(1, 1)
        };
        float posXYUnit = oObject.attributes["posXYUnit"] switch
        {
            LengthUnit.Meter => 1.0f,
            LengthUnit.Feet => UnitValue.Foot,
            _ => UnitValue.Tile,
        };
        Vector3 displacement = (oObject.transform.localPosition - initialPosition) / posXYUnit;
        displacement.y *= posXYUnit * 100;
        Vector3 newPos = Utils.ParseVector3(position, true) + new Vector3(displacement.x * orient.x, displacement.y, displacement.z * orient.y);
        oObject.attributes["posXYZ"] = $"[{newPos.x:0.00},{newPos.z:0.00},{newPos.y:0.00}]";
        oObject.attributes["rotation"] = $"[{oObject.transform.localEulerAngles.x:0.00},{oObject.transform.localEulerAngles.z:0.00},{oObject.transform.localEulerAngles.y:0.00}]";
        oObject.SetBaseTransform();
        await ApiManager.instance.ModifyObject($"{oObject.category}s/{oObject.id}", new() { { "attributes", oObject.attributes } });
        UiManager.instance.UpdateGuiInfos();
    }
}
