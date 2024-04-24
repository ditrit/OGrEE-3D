using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class Positionner : MonoBehaviour
{
    public static Positionner instance;
    public bool isSaving = false;
    public List<Vector3> initialPositions;
    public List<Quaternion> initialRotations;
    public bool snapping = false;
    public Transform realDisplacement;

    [SerializeField] private float scale;
    [SerializeField] private List<AxisMover> axisMovers;

    private List<Vector3> originalPositions;
    private Vector2 orient;
    private List<float> posXYUnits;
    private List<Item> items;
    private List<Vector3> positionOffsets;
    private List<Quaternion> rotationOffsets;

    private void Awake()
    {
        instance = this;
        EventManager.instance.PositionMode.Add(OnPositionMode);
        gameObject.SetActive(false);
    }

    public void OnDestroy()
    {
        EventManager.instance.PositionMode.Remove(OnPositionMode);
    }

    private void OnEnable()
    {
        foreach (Transform child in transform.GetChild(0))
            child.gameObject.layer = LayerMask.NameToLayer("UI");
        realDisplacement.parent = items[0].transform.parent;
        initialPositions = items.Select(i => i.transform.localPosition).ToList();
        initialRotations = items.Select(i => i.transform.localRotation).ToList();
        realDisplacement.SetLocalPositionAndRotation(initialPositions[0], initialRotations[0]);
        originalPositions = items.Select(i => ((JArray)i.attributes["posXYZ"]).ToVector3()).ToList();
        orient = items[0].transform.parent.GetComponent<Room>().attributes["axisOrientation"] switch
        {
            AxisOrientation.XMinus => new(-1, 1),
            AxisOrientation.YMinus => new(1, -1),
            AxisOrientation.BothMinus => new(-1, -1),
            _ => new(1, 1)
        };
        posXYUnits = items.Select(i => i.attributes["posXYUnit"] switch
        {
            LengthUnit.Meter => 1.0f,
            LengthUnit.Feet => UnitValue.Foot,
            _ => UnitValue.Tile,
        }).ToList();
    }

    private void Update()
    {
        transform.GetChild(0).localScale = scale * Vector3.Distance(transform.position, Camera.main.transform.position) * Vector3.one;
        bool alreadyActive = false;
        foreach (AxisMover axisMover in axisMovers)
            if (axisMover.active)
            {
                axisMover.Move();
                alreadyActive = true;
                break;
            }
        if (!alreadyActive)
            foreach (AxisMover objectMover in axisMovers)
                objectMover.Move();
        if (snapping)
        {
            Vector3 move = (realDisplacement.localPosition - initialPositions[0]) / posXYUnits[0];
            move.y = Mathf.Round(move.y * posXYUnits[0] / 0.01f) * 0.01f;
            move.x = Mathf.Round(move.x) * posXYUnits[0];
            move.z = Mathf.Round(move.z) * posXYUnits[0];
            Vector3 snappedPos = initialPositions[0] + move;
            Quaternion quaternion = realDisplacement.localRotation;
            Vector3 snappedRot = new(Mathf.Round(quaternion.eulerAngles.x / 45) * 45, Mathf.Round(quaternion.eulerAngles.y / 45) * 45, Mathf.Round(quaternion.eulerAngles.z / 45) * 45);
            items[0].transform.localPosition = snappedPos;
            items[0].transform.localRotation = Quaternion.Euler(snappedRot);
        }
        else
            items[0].transform.SetPositionAndRotation(realDisplacement.position, realDisplacement.rotation);
        for (int i = 1; i < items.Count; i++)
            items[i].transform.SetPositionAndRotation(items[0].transform.position + positionOffsets[i], rotationOffsets[i] * items[0].transform.rotation);
        if (items.Count == 1)
        {
            ComputePosition(0);
            UiManager.instance.UpdateGuiInfos();
        }
    }

    /// <summary>
    /// Whenn called, if position mode is exiting, launch a <see cref="Prompt"/> and save or not the current position of the selection
    /// </summary>
    /// <param name="_e">The event's instance</param>
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

    /// <summary>
    /// Handle position mode on/off
    ///<br/> On exiting position mode, lock the application while <see cref="SavePosition"/> is running
    /// </summary>
    public async Task TogglePositionMode()
    {
        GameManager.instance.positionMode ^= true;
        EventManager.instance.Raise(new PositionModeEvent(GameManager.instance.positionMode));
        if (GameManager.instance.positionMode)
        {
            items = GameManager.instance.GetSelected().Select(go => go.GetComponent<Item>()).ToList();
            GameManager.instance.AppendLogLine(new ExtendedLocalizedString("Logs", "Enable Position Mode", string.Join(", ", items.Select(i => i.name))), ELogTarget.logger, ELogtype.success);
            positionOffsets = items.Select(i => i.transform.position - items[0].transform.position).ToList();
            rotationOffsets = items.Select(i => i.transform.rotation * Quaternion.Inverse(items[0].transform.rotation)).ToList();
            transform.parent = items[0].transform;
            transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            gameObject.SetActive(true);
        }
        else
        {
            while (isSaving)
                await Task.Delay(10);
            GameManager.instance.AppendLogLine(new ExtendedLocalizedString("Logs", "Disable Position Mode", string.Join(", ", items.Select(i => i.name))), ELogTarget.logger, ELogtype.success);
        }
    }

    /// <summary>
    /// Compute the position of the ith object in the selection and change its orientation and posXYZ attributes
    /// </summary>
    /// <param name="_i">index of the object in the selection</param>
    private void ComputePosition(int _i)
    {
        Vector3 displacement = (items[_i].transform.localPosition - initialPositions[_i]) / posXYUnits[_i];
        displacement.y *= posXYUnits[_i] * 100;
        Vector3 newPos = originalPositions[_i].ZAxisUp() + new Vector3(displacement.x * orient.x, displacement.y, displacement.z * orient.y);
        items[_i].attributes["posXYZ"] = $"[{newPos.x:0.##},{newPos.z:0.##},{newPos.y:0.##}]";
        items[_i].attributes["rotation"] = $"[{items[_i].transform.localEulerAngles.x:0.##},{items[_i].transform.localEulerAngles.z:0.##},{items[_i].transform.localEulerAngles.y:0.##}]";
    }

    /// <summary>
    /// Save the position of the selection and send it to the API with <see cref="ApiManager.ModifyObject(string, Dictionary{string, object})"/>
    ///<br/> Also update the GUI
    /// </summary>
    private async void SavePosition()
    {
        for (int i = 0; i < items.Count; i++)
        {
            ComputePosition(i);
            items[i].SetBaseTransform();
            await ApiManager.instance.ModifyObject($"{items[i].category}s/{items[i].id}", new() { { "attributes", items[i].attributes } });
            UiManager.instance.UpdateGuiInfos();
            if (items[i].group)
                items[i].group.ShapeGroup();
        }
    }
}
