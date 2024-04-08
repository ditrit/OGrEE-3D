using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class Rescaler : MonoBehaviour
{
    public bool isSaving = false;
    public List<Vector3> initialPosition;
    public List<Quaternion> initialRotation;
    public static Rescaler instance;
    public bool snapping = false;
    public Transform realDisplacement;

    [SerializeField] private float scale;
    [SerializeField] private List<ObjectMover> objectMovers;

    private List<string> position;
    private Room parentRoom;
    private List<float> posXYUnit;
    private List<Item> items;
    private List<Vector3> positionOffsets;
    private List<Quaternion> rotationOffsets;

    private void Awake()
    {
        instance = this;
        EventManager.instance.PositionMode.Add(OnPositionMode);
        gameObject.SetActive(false);
    }

    public async Task TogglePositionMode()
    {
        GameManager.instance.positionMode ^= true;
        EventManager.instance.Raise(new PositionModeEvent(GameManager.instance.positionMode));
        if (GameManager.instance.positionMode)
        {
            items = GameManager.instance.GetSelected().Select(go => go.GetComponent<Item>()).ToList();
            positionOffsets = items.Select(i => i.transform.position - items[0].transform.position).ToList();
            rotationOffsets = items.Select(i => i.transform.rotation * Quaternion.Inverse(items[0].transform.rotation)).ToList();
            transform.parent = items[0].transform;
            transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            gameObject.SetActive(true);
        }
        else
            while (isSaving)
                await Task.Delay(10);
    }

    private void OnEnable()
    {
        realDisplacement.parent = items[0].transform.parent;
        initialPosition = items.Select(i => i.transform.localPosition).ToList();
        initialRotation = items.Select(i => i.transform.localRotation).ToList();
        realDisplacement.SetLocalPositionAndRotation(initialPosition[0], initialRotation[0]);
        position = items.Select(i => i.attributes["posXYZ"]).ToList();
        parentRoom = items[0].transform.parent.GetComponent<Room>();
        posXYUnit = items.Select(i => i.attributes["posXYUnit"] switch
        {
            LengthUnit.Meter => 1.0f,
            LengthUnit.Feet => UnitValue.Foot,
            _ => UnitValue.Tile,
        }).ToList();
    }
    void Update()
    {
        transform.GetChild(0).localScale = scale * Vector3.Distance(transform.position, Camera.main.transform.position) * Vector3.one;
        bool alreadyActive = false;
        foreach (ObjectMover objectMover in objectMovers)
            if (objectMover.active)
            {
                objectMover.Move();
                alreadyActive = true;
                break;
            }
        if (!alreadyActive)
            foreach (ObjectMover objectMover in objectMovers)
                objectMover.Move();
        if (snapping)
        {
            Vector3 move = (realDisplacement.localPosition - initialPosition[0]) / posXYUnit[0];
            move.y = Mathf.Round(move.y * posXYUnit[0] / 0.01f) * 0.01f;
            move.x = Mathf.Round(move.x) * posXYUnit[0];
            move.z = Mathf.Round(move.z) * posXYUnit[0];
            Vector3 snappedPos = initialPosition[0] + move;
            Quaternion quaternion = realDisplacement.localRotation;
            Vector3 snappedRot = new(Mathf.Round(quaternion.eulerAngles.x / 45) * 45, Mathf.Round(quaternion.eulerAngles.y / 45) * 45, Mathf.Round(quaternion.eulerAngles.z / 45) * 45);
            items[0].transform.localPosition = snappedPos;
            items[0].transform.localRotation = Quaternion.Euler(snappedRot);
        }
        else
            items[0].transform.SetPositionAndRotation(realDisplacement.position, realDisplacement.rotation);
        for (int i = 1; i < items.Count; i++)
            items[i].transform.SetPositionAndRotation(items[0].transform.position + positionOffsets[i], rotationOffsets[i] * items[0].transform.rotation);
    }

    public async void OnPositionMode(PositionModeEvent _e)
    {
        if (_e.toggled)
            return;
        isSaving = true;
        Prompt prompt = UiManager.instance.GeneratePrompt("Save Object Position", "Continue", "Cancel");
        while (prompt.state == EPromptStatus.wait)
            await Task.Delay(10);
        if (prompt.state == EPromptStatus.accept)
            SavePosition();
        else
            UiManager.instance.ResetTransform();
        UiManager.instance.DeletePrompt(prompt);
        transform.parent = null;
        realDisplacement.transform.parent = null;
        gameObject.SetActive(false);
        isSaving = false;
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
        for (int i = 0; i < items.Count; i++)
        {
            Vector3 displacement = (items[i].transform.localPosition - initialPosition[i]) / posXYUnit[i];
            displacement.y *= posXYUnit[i] * 100;
            Vector3 newPos = Utils.ParseVector3(position[i], true) + new Vector3(displacement.x * orient.x, displacement.y, displacement.z * orient.y);
            items[i].attributes["posXYZ"] = $"[{newPos.x:0.00},{newPos.z:0.00},{newPos.y:0.00}]";
            items[i].attributes["rotation"] = $"[{items[i].transform.localEulerAngles.x:0.00},{items[i].transform.localEulerAngles.z:0.00},{items[i].transform.localEulerAngles.y:0.00}]";
            items[i].SetBaseTransform();
            await ApiManager.instance.ModifyObject($"{items[i].category}s/{items[i].id}", new() { { "attributes", items[i].attributes } });
            UiManager.instance.UpdateGuiInfos();
            if (items[i].group)
                Utils.ShapeGroup(items[i].group.GetContent().Select(go => go.transform), items[i].group, Category.Room);
        }
    }
}
