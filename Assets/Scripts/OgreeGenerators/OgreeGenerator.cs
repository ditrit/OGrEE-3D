using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

public class OgreeGenerator : MonoBehaviour
{
    public static OgreeGenerator instance;
    private readonly CustomerGenerator customerGenerator = new();
    private readonly BuildingGenerator buildingGenerator = new();
    private readonly ObjectGenerator objectGenerator = new();
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

        OgreeObject newObject;
        // Get dependencies from API:
        // Domains
        if (_obj.category != Category.Domain && !string.IsNullOrEmpty(_obj.domain)
            && !GameManager.instance.allItems.Contains(_obj.domain))
            await ApiManager.instance.GetObject($"domains/{_obj.domain}", ApiManager.instance.DrawObject);

        // Templates
        if (_obj.category == Category.Building && Utils.IsInDict(_obj.attributes, "template")
            && !GameManager.instance.buildingTemplates.ContainsKey(_obj.attributes["template"]))
        {
            Debug.Log($"Get template \"{_obj.attributes["template"]}\" from API");
            await ApiManager.instance.GetObject($"bldg-templates/{_obj.attributes["template"]}", ApiManager.instance.DrawObject);
        }

        if (_obj.category == Category.Room && Utils.IsInDict(_obj.attributes, "template")
            && !GameManager.instance.roomTemplates.ContainsKey(_obj.attributes["template"]))
        {
            Debug.Log($"Get template \"{_obj.attributes["template"]}\" from API");
            await ApiManager.instance.GetObject($"room-templates/{_obj.attributes["template"]}", ApiManager.instance.DrawObject);
        }

        if ((_obj.category == Category.Rack || _obj.category == Category.Device || _obj.category == Category.Generic) && Utils.IsInDict(_obj.attributes, "template")
            && !GameManager.instance.objectTemplates.ContainsKey(_obj.attributes["template"]))
        {
            Debug.Log($"Get template \"{_obj.attributes["template"]}\" from API");
            await ApiManager.instance.GetObject($"obj-templates/{_obj.attributes["template"]}", ApiManager.instance.DrawObject);
        }

        // Tags
        if (_obj.category != Category.Domain)
        {
            foreach (string tagName in _obj.tags)
            {
                if (GameManager.instance.GetTag(tagName) == null)
                    await ApiManager.instance.GetObject($"tags/{tagName}", ApiManager.instance.CreateTag);
            }
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
                    if (GameManager.instance.getCoordsMode)
                        UiManager.instance.ToggleGetCoordsMode();
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
                newObject = customerGenerator.CreateDomain(_obj);
                break;
            case Category.Site:
                newObject = customerGenerator.CreateSite(_obj);
                break;
            case Category.Building:
                newObject = buildingGenerator.CreateBuilding(_obj, parent);
                break;
            case Category.Room:
                newObject = buildingGenerator.CreateRoom(_obj, parent);
                break;
            case Category.Rack:
                newObject = objectGenerator.CreateRack(_obj, parent);
                break;
            case Category.Device:
                newObject = objectGenerator.CreateDevice(_obj, parent);
                break;
            case Category.Corridor:
                newObject = objectGenerator.CreateCorridor(_obj, parent);
                break;
            case Category.Group:
                newObject = objectGenerator.CreateGroup(_obj, parent);
                break;
            case Category.Generic:
                newObject = objectGenerator.CreateGeneric(_obj, parent);
                break;
            default:
                newObject = null;
                GameManager.instance.AppendLogLine($"Unknown object type ({_obj.category})", ELogTarget.both, ELogtype.error);
                break;
        }
        if (newObject)
        {
            newObject.SetBaseTransform();
            if (newObject is not Domain && (!GameManager.instance.objectRoot || GameManager.instance.objectRoot.GetComponent<OgreeObject>().isDoomed)
                && !(parent == GameManager.instance.templatePlaceholder || parent == GameManager.instance.templatePlaceholder.GetChild(0)))
            {
                GameManager.instance.objectRoot = newObject.gameObject;
                FindObjectOfType<CameraControl>().MoveToObject(newObject.transform);
            }
            if (newObject is Item item)
            {
                if (parent)
                {
                    item.temperatureUnit = parent.GetComponent<Room>()?.temperatureUnit;
                    if (string.IsNullOrEmpty(item.temperatureUnit))
                        item.temperatureUnit = parent.GetComponent<Item>()?.temperatureUnit;
                }
                else
                    item.temperatureUnit = await ApiManager.instance.GetObject($"tempunits/{newObject.id}", ApiManager.instance.TempUnitFromAPI);
            }
            if (newObject is Room newItemRoom)
                newItemRoom.temperatureUnit = await ApiManager.instance.GetObject($"tempunits/{newObject.id}", ApiManager.instance.TempUnitFromAPI);
        }
        ResetCoroutine();
        return newObject;
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
