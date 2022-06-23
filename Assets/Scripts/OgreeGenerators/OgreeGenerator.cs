using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

[RequireComponent(typeof(CustomerGenerator))]
[RequireComponent(typeof(BuildingGenerator))]
[RequireComponent(typeof(ObjectGenerator))]
public class OgreeGenerator : MonoBehaviour
{
    public static OgreeGenerator instance;
    [SerializeField] private CustomerGenerator customerGenerator;
    [SerializeField] private BuildingGenerator buildingGenerator;
    [SerializeField] private ObjectGenerator objectGenerator;
    private Coroutine waitCoroutine = null;

    private void Awake()
    {
        if (!instance)
            instance = this;
        else
            Destroy(this);
    }

    private void Start()
    {
        customerGenerator = GetComponent<CustomerGenerator>();
        buildingGenerator = GetComponent<BuildingGenerator>();
        objectGenerator = GetComponent<ObjectGenerator>();
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

        // Call Create function
        switch (_obj.category)
        {
            case "tenant":
                newItem = customerGenerator.CreateTenant(_obj);
                break;
            case "site":
                newItem = customerGenerator.CreateSite(_obj, _parent);
                break;
            case "building":
                newItem = buildingGenerator.CreateBuilding(_obj, _parent);
                break;
            case "room":
                newItem = buildingGenerator.CreateRoom(_obj, _parent);
                break;
            case "rack":
                newItem = objectGenerator.CreateRack(_obj, _parent);
                break;
            case "device":
                newItem = objectGenerator.CreateDevice(_obj, _parent);
                break;
            case "corridor":
                newItem = objectGenerator.CreateCorridor(_obj, _parent);
                break;
            case "group":
                newItem = objectGenerator.CreateGroup(_obj, _parent);
                break;
            case "sensor":
                newItem = objectGenerator.CreateSensor(_obj, _parent);
                break;
            default:
                newItem = null;
                GameManager.gm.AppendLogLine($"Unknown object type ({_obj.category})", "yellow");
                break;
        }
        ResetCoroutine();
        if (newItem != null)
        {
            newItem.originalLocalPosition = newItem.gameObject.transform.localPosition;
            newItem.originalLocalRotation = newItem.gameObject.transform.localRotation;
            newItem.originalLocalScale = newItem.gameObject.transform.localScale;
        }
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
        Debug.Log("[] event raised !");
        EventManager.Instance.Raise(new ImportFinishedEvent());
        EventManager.Instance.Raise(new ChangeCursorEvent() { type = CursorChanger.CursorType.Idle });
    }
}
