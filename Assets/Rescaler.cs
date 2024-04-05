using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class Rescaler : MonoBehaviour
{
    private string position;
    public Vector3 initialPosition;
    public Quaternion initialRotation;
    [SerializeField] private float scale;
    public static Rescaler instance;
    public bool snapping = false;
    public Room parentRoom;
    public Transform realDisplacement;
    public float posXYUnit;
    public Item item;
    [SerializeField] private List<ObjectMover> objectMovers;
    private void Awake()
    {
        instance = this;
        EventManager.instance.PositionMode.Add(OnPositionMode);
        gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        item = transform.parent.GetComponent<Item>();
        realDisplacement.parent = item.transform.parent;
        initialPosition = item.transform.localPosition;
        initialRotation = item.transform.localRotation;
        realDisplacement.SetLocalPositionAndRotation(initialPosition, initialRotation);
        position = item.attributes["posXYZ"];
        parentRoom = item.transform.parent.GetComponent<Room>();
        posXYUnit = item.attributes["posXYUnit"] switch
        {
            LengthUnit.Meter => 1.0f,
            LengthUnit.Feet => UnitValue.Foot,
            _ => UnitValue.Tile,
        };
    }
    void Update()
    {
        transform.GetChild(0).localScale = scale * Vector3.Distance(transform.position, Camera.main.transform.position) * Vector3.one;
        foreach (ObjectMover objectMover in objectMovers)
            objectMover.Move();
        if (snapping)
        {
            Vector3 move = (realDisplacement.localPosition - initialPosition) / posXYUnit;
            move.y = Mathf.Round(move.y * posXYUnit / 0.01f) * 0.01f;
            move.x = Mathf.Round(move.x) * posXYUnit;
            move.z = Mathf.Round(move.z) * posXYUnit;
            Vector3 snappedPos = initialPosition + move;
            Quaternion quaternion = realDisplacement.localRotation;
            Vector3 snappedRot = new(Mathf.Round(quaternion.eulerAngles.x / 45) * 45, Mathf.Round(quaternion.eulerAngles.y / 45) * 45, Mathf.Round(quaternion.eulerAngles.z / 45) * 45);
            item.transform.localPosition = snappedPos;
            item.transform.localRotation = Quaternion.Euler(snappedRot);
        }
        else
            item.transform.SetPositionAndRotation(realDisplacement.position, realDisplacement.rotation);
    }

    public async void OnPositionMode(PositionModeEvent _e)
    {
        if (_e.toggled)
            return;
        Prompt prompt = UiManager.instance.GeneratePrompt("Save Object Position", "Continue", "Cancel");
        while (prompt.state == EPromptStatus.wait)
            await Task.Delay(10);
        if (prompt.state == EPromptStatus.accept)
            SavePosition();
        else
            UiManager.instance.ResetTransform();
        UiManager.instance.DeletePrompt(prompt);
        SavePosition();
        transform.parent = null;
        realDisplacement.transform.parent = null;
        gameObject.SetActive(false);
    }

    public void OnDestroy()
    {
        EventManager.instance.PositionMode.Remove(OnPositionMode);
    }

    public async void SavePosition()
    {
        Vector2 orient = parentRoom.attributes["axisOrientation"] switch
        {
            AxisOrientation.XMinus => new(-1, 1),
            AxisOrientation.YMinus => new(1, -1),
            AxisOrientation.BothMinus => new(-1, -1),
            _ => new(1, 1)
        };
        Vector3 displacement = (item.transform.localPosition - initialPosition) / posXYUnit;
        displacement.y *= posXYUnit * 100;
        Vector3 newPos = Utils.ParseVector3(position, true) + new Vector3(displacement.x * orient.x, displacement.y, displacement.z * orient.y);
        item.attributes["posXYZ"] = $"[{newPos.x:0.00},{newPos.z:0.00},{newPos.y:0.00}]";
        item.attributes["rotation"] = $"[{item.transform.localEulerAngles.x:0.00},{item.transform.localEulerAngles.z:0.00},{item.transform.localEulerAngles.y:0.00}]";
        item.SetBaseTransform();
        await ApiManager.instance.ModifyObject($"{item.category}s/{item.id}", new() { { "attributes", item.attributes } });
        UiManager.instance.UpdateGuiInfos();
        if (item.group)
            Utils.ShapeGroup(item.group.GetContent().Select(go => go.transform), item.group, Category.Room);
    }
}
