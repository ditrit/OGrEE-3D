using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class OgreeGenerator : MonoBehaviour
{
    public static OgreeGenerator instance;
    private readonly CustomerGenerator customerGenerator = new CustomerGenerator();
    private readonly BuildingGenerator buildingGenerator = new BuildingGenerator();
    private readonly ObjectGenerator objectGenerator = new ObjectGenerator();
    private Coroutine waitCoroutine = null;

    private void Awake()
    {
        if (!instance)
            instance = this;
        else
            Destroy(this);
    }

    ///<summary>
    /// Call the good CreateX function according to the item's category.
    ///</summary>
    ///<param name="_obj">The item to generate</param>
    public async Task<OgreeObject> CreateItemFromSApiObject(SApiObject _obj, Transform _parent = null)
    {
        if (Utils.GetObjectById(_obj.id))
        {
            GameManager.instance.AppendLogLine($"{_obj.name} already exists.", ELogTarget.none, ELogtype.info);
            ResetCoroutine();
            return null;
        }

        OgreeObject newItem;
        // Get dependencies from API
        if (_obj.category != Category.Domain && !string.IsNullOrEmpty(_obj.domain)
            && !GameManager.instance.allItems.Contains(_obj.domain))
            await ApiManager.instance.GetObject($"domains/{_obj.domain}", ApiManager.instance.DrawObject);

        if (_obj.category == Category.Building && !string.IsNullOrEmpty(_obj.attributes["template"])
            && !GameManager.instance.buildingTemplates.ContainsKey(_obj.attributes["template"]))
        {
            Debug.Log($"Get template \"{_obj.attributes["template"]}\" from API");
            await ApiManager.instance.GetObject($"bldg-templates/{_obj.attributes["template"]}", ApiManager.instance.DrawObject);
        }

        if (_obj.category == Category.Room && !string.IsNullOrEmpty(_obj.attributes["template"])
            && !GameManager.instance.roomTemplates.ContainsKey(_obj.attributes["template"]))
        {
            Debug.Log($"Get template \"{_obj.attributes["template"]}\" from API");
            await ApiManager.instance.GetObject($"room-templates/{_obj.attributes["template"]}", ApiManager.instance.DrawObject);
        }

        if ((_obj.category == Category.Rack || _obj.category == Category.Device) && !string.IsNullOrEmpty(_obj.attributes["template"])
            && !GameManager.instance.objectTemplates.ContainsKey(_obj.attributes["template"]))
        {
            Debug.Log($"Get template \"{_obj.attributes["template"]}\" from API");
            await ApiManager.instance.GetObject($"obj-templates/{_obj.attributes["template"]}", ApiManager.instance.DrawObject);
        }

        // Find parent
        Transform parent = Utils.FindParent(_parent, _obj.parentId);
        if (!parent)
        {
            if (_obj.category == Category.Device && string.IsNullOrEmpty(_obj.attributes["template"]))
            {
                GameManager.instance.AppendLogLine("Unable to draw a basic device without its parent.", ELogTarget.both, ELogtype.errorCli);
                return null;
            }
            if (_obj.category == Category.Corridor || _obj.category == Category.Group)
            {
                GameManager.instance.AppendLogLine($"Unable to draw a {_obj.category} without its parent.", ELogTarget.both, ELogtype.errorCli);
                return null;
            }

            if (_obj.category != Category.Domain && GameManager.instance.objectRoot)
            {
                Prompt prompt = UiManager.instance.GeneratePrompt($"Drawing {_obj.name} will erase current scene.", "Ok", "Cancel");
                while (prompt.state == EPromptStatus.wait)
                    await Task.Delay(10);
                if (prompt.state == EPromptStatus.accept)
                {
                    if (GameManager.instance.editMode)
                        UiManager.instance.EditFocused();
                    await GameManager.instance.UnfocusAll();
                    Destroy(GameManager.instance.objectRoot);
                    await GameManager.instance.PurgeDomains(_obj.domain);
                    UiManager.instance.DeletePrompt(prompt);
                }
                else //if (prompt.state == EPromptResp.refuse)
                {
                    EventManager.instance.Raise(new CancelGenerateEvent());
                    UiManager.instance.DeletePrompt(prompt);
                    return null;
                }
            }
        }
        // Call Create function
        switch (_obj.category)
        {
            case Category.Domain:
                newItem = customerGenerator.CreateDomain(_obj);
                break;
            case Category.Site:
                newItem = customerGenerator.CreateSite(_obj);
                break;
            case Category.Building:
                newItem = buildingGenerator.CreateBuilding(_obj, parent);
                break;
            case Category.Room:
                newItem = buildingGenerator.CreateRoom(_obj, parent);
                break;
            case Category.Rack:
                newItem = objectGenerator.CreateRack(_obj, parent);
                break;
            case Category.Device:
                newItem = objectGenerator.CreateDevice(_obj, parent);
                break;
            case Category.Corridor:
                newItem = objectGenerator.CreateCorridor(_obj, parent);
                break;
            case Category.Group:
                newItem = objectGenerator.CreateGroup(_obj, parent);
                break;
            default:
                newItem = null;
                GameManager.instance.AppendLogLine($"Unknown object type ({_obj.category})", ELogTarget.both, ELogtype.error);
                break;
        }
        if (newItem)
        {
            newItem.SetBaseTransform();
            if (newItem.category != Category.Domain && (!GameManager.instance.objectRoot || GameManager.instance.objectRoot.GetComponent<OgreeObject>().isDoomed)
                && !(parent == GameManager.instance.templatePlaceholder || parent == GameManager.instance.templatePlaceholder.GetChild(0)))
            {
                GameManager.instance.objectRoot = newItem.gameObject;
                FindObjectOfType<CameraControl>().MoveToObject(newItem.transform);
            }
            if (newItem is OObject newItemOObject)
            {
                if (parent)
                {
                    newItemOObject.temperatureUnit = parent.GetComponent<Room>()?.temperatureUnit;
                    if (string.IsNullOrEmpty(newItemOObject.temperatureUnit))
                        newItemOObject.temperatureUnit = parent.GetComponent<OObject>()?.temperatureUnit;
                }
                else
                    newItemOObject.temperatureUnit = await ApiManager.instance.GetObject($"tempunits/{newItem.id}", ApiManager.instance.TempUnitFromAPI);
            }
            if (newItem is Room newItemRoom)
            {
                newItemRoom.temperatureUnit = await ApiManager.instance.GetObject($"tempunits/{newItem.id}", ApiManager.instance.TempUnitFromAPI);
            }
        }
        ResetCoroutine();
        return newItem;
    }

    ///<summary>
    /// Create a sensor in given object.
    ///</summary>
    ///<param name="_obj">The base object</param>
    ///<param name="_parent"></param>
    ///<returns>The created sensor</returns>
    public Sensor CreateSensorFromSApiObject(SApiObject _obj, Transform _parent = null)
    {
        ResetCoroutine();
        return objectGenerator.CreateSensor(_obj, _parent);
    }

    ///<summary>
    /// If a waitCoroutine is running, stop it. Then, start a new one.
    ///</summary>
    private void ResetCoroutine()
    {
        if (waitCoroutine != null)
            StopCoroutine(waitCoroutine);
        waitCoroutine = StartCoroutine(WaitAndRaiseEvent());
    }

    ///<summary>
    /// Wait 1 second and raise ImportFinished et ChangeCursor envents
    ///</summary>
    private IEnumerator WaitAndRaiseEvent()
    {
        yield return new WaitForSeconds(1f);
        EventManager.instance.Raise(new ImportFinishedEvent());
        EventManager.instance.Raise(new ChangeCursorEvent(CursorChanger.CursorType.Idle));
        // Debug.Log("[] event raised !");
    }
}
