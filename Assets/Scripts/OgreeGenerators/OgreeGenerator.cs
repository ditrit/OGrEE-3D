using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class OgreeGenerator : MonoBehaviour
{
    public static OgreeGenerator instance;
    private CustomerGenerator customerGenerator = new CustomerGenerator();
    private BuildingGenerator buildingGenerator = new BuildingGenerator();
    private ObjectGenerator objectGenerator = new ObjectGenerator();
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
        OgreeObject newItem;
        // Get dependencies from API
        if (_obj.category != "tenant" && !string.IsNullOrEmpty(_obj.domain)
            && !GameManager.gm.allItems.Contains(_obj.domain))
            await ApiManager.instance.GetObject($"tenants?name={_obj.domain}", ApiManager.instance.DrawObject);

        if (_obj.category == "room" && !string.IsNullOrEmpty(_obj.attributes["template"])
            && !GameManager.gm.roomTemplates.ContainsKey(_obj.attributes["template"]))
        {
            Debug.Log($"Get template \"{_obj.attributes["template"]}\" from API");
            await ApiManager.instance.GetObject($"room-templates/{_obj.attributes["template"]}", ApiManager.instance.DrawObject);
        }

        if ((_obj.category == "rack" || _obj.category == "device") && !string.IsNullOrEmpty(_obj.attributes["template"])
            && !GameManager.gm.objectTemplates.ContainsKey(_obj.attributes["template"]))
        {
            Debug.Log($"Get template \"{_obj.attributes["template"]}\" from API");
            await ApiManager.instance.GetObject($"obj-templates/{_obj.attributes["template"]}", ApiManager.instance.DrawObject);
        }

        // Find parent
        Transform parent = Utils.FindParent(_parent, _obj.parentId);
        if (_obj.category != "tenant" && !parent)
        {
            GameManager.gm.AppendLogLine($"Parent of {_obj.name} not found", true, eLogtype.error);
            return null;
        }

        // Call Create function
        switch (_obj.category)
        {
            case "tenant":
                newItem = customerGenerator.CreateTenant(_obj);
                break;
            case "site":
                newItem = customerGenerator.CreateSite(_obj, parent);
                break;
            case "building":
                newItem = buildingGenerator.CreateBuilding(_obj, parent);
                break;
            case "room":
                newItem = buildingGenerator.CreateRoom(_obj, parent);
                break;
            case "rack":
                newItem = objectGenerator.CreateRack(_obj, parent);
                break;
            case "device":
                newItem = objectGenerator.CreateDevice(_obj, parent);
                break;
            case "corridor":
                newItem = objectGenerator.CreateCorridor(_obj, parent);
                break;
            case "group":
                newItem = objectGenerator.CreateGroup(_obj, parent);
                break;
            case "sensor":
                newItem = objectGenerator.CreateSensor(_obj, parent);
                break;
            default:
                newItem = null;
                GameManager.gm.AppendLogLine($"Unknown object type ({_obj.category})", true, eLogtype.error);
                break;
        }
        newItem?.SetBaseTransform();
        ResetCoroutine();
        return newItem;
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
        EventManager.Instance.Raise(new ImportFinishedEvent());
        EventManager.Instance.Raise(new ChangeCursorEvent() { type = CursorChanger.CursorType.Idle });
        // Debug.Log("[] event raised !");
    }
}
