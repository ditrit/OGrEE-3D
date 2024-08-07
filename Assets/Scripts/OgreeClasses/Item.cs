using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Item : OgreeObject
{
    public Color color;

    /// <summary>
    /// The direct child of a room which is a parent of this object or which is this object
    /// </summary>
    public Item referent;
    public GameObject tempBar;
    public string temperatureUnit;
    public bool hasSlotColor = false;
    public Group group = null;
    public ClearanceHandler clearanceHandler = new();

    protected virtual void Start()
    {
        EventManager.instance.UpdateDomain.Add(OnDomainColorUpdate);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        EventManager.instance.UpdateDomain.Remove(OnDomainColorUpdate);
        if (GetComponent<ObjectDisplayController>() is ObjectDisplayController odc && odc.isHidden)
        {
            UiManager.instance.hiddenObjects.Remove(this);
            UiManager.instance.hiddenObjList.RebuildMenu(UiManager.instance.BuildHiddenObjButtons);
        }
    }

    ///<summary>
    /// On an UpdateDomainEvent, update the object's color if its the right domain
    ///</summary>
    ///<param name="_event">The event to catch</param>
    private void OnDomainColorUpdate(UpdateDomainEvent _event)
    {
        if (_event.name == domain && !hasSlotColor && !attributes.ContainsKey("color"))
            UpdateColorByDomain();
    }

    ///<summary>
    /// Update the OObject attributes with given SApiObject.
    ///</summary>
    ///<param name="_src">The SApiObject used to update attributes</param>
    public override void UpdateFromSApiObject(SApiObject _src)
    {
        foreach (string attribute in _src.attributes.Keys)
        {
            if (attribute.StartsWith("temperature_")
                && (!attributes.ContainsKey(attribute) || attributes[attribute] != _src.attributes[attribute]))
                SetTemperature((float)_src.attributes[attribute], attribute.Substring(12), _src.attributes["heightUnit"]);
            if (attribute == "clearance")
            {
                List<float> lengths = ((JArray)_src.attributes[attribute]).ToObject<List<float>>();
                if (lengths == null)
                {
                    GameManager.instance.AppendLogLine(new ExtendedLocalizedString("Logs", "Can't deserialize clearance attribute", _src.name), ELogTarget.both, ELogtype.error);
                    break;
                }
                if (lengths.Count != 6)
                {
                    GameManager.instance.AppendLogLine(new ExtendedLocalizedString("Logs", "Wrong vector cardinality for clearance", _src.name), ELogTarget.both, ELogtype.error);
                    break;
                }
                clearanceHandler.Initialize(lengths[0], lengths[1], lengths[2], lengths[3], lengths[4], lengths[5], transform);
            }
        }
        base.UpdateFromSApiObject(_src);

        if (!transform.parent || transform.parent.GetComponent<Room>())
            referent = this;
        else if (transform.parent?.GetComponent<Item>().referent != null)
            referent = transform.parent.GetComponent<Item>().referent;
        else
            referent = null;

        GetComponent<DisplayObjectData>()?.UpdateLabels();
    }

    ///<summary>
    /// Set a Color with an hexadecimal value
    ///</summary>
    ///<param name="_hex">The hexadecimal value, without '#'</param>
    public void SetColor(string _hex)
    {
        if (ColorUtility.TryParseHtmlString($"#{_hex}", out Color newColor))
        {
            color = newColor;
            GetComponent<ObjectDisplayController>().ChangeColor(color);
        }
        else
        {
            UpdateColorByDomain();
            GameManager.instance.AppendLogLine(new ExtendedLocalizedString("Logs", "Unknown color to display", id), ELogTarget.both, ELogtype.warning);
        }
    }

    ///<summary>
    /// Update object's color according to its domain.
    ///</summary>
    ///<param name="_domain">An optionnal domain to use</param>
    public void UpdateColorByDomain(string _domain = null)
    {
        if (attributes.ContainsKey("color") || _domain == "")
            return;

        string domainToUse = _domain == null ? domain : _domain;
        if (!GameManager.instance.allItems.Contains(domainToUse))
        {
            GameManager.instance.AppendLogLine(new ExtendedLocalizedString("Logs", "Domain doesn't exist", domainToUse), ELogTarget.both, ELogtype.error);
            return;
        }

        Domain domainObject = ((GameObject)GameManager.instance.allItems[domainToUse]).GetComponent<Domain>();

        color = Utils.ParseHtmlColor($"#{domainObject.attributes["color"]}");
        GetComponent<ObjectDisplayController>().ChangeColor(color);
    }

    ///<summary>
    /// Display or hide all unused slots of the object.
    ///</summary>
    ///<param name="_value">True or false value</param>
    public void ToggleSlots(bool _value)
    {
        foreach (Slot slot in GetComponentsInChildren<Slot>())
            if (slot.transform.parent == transform && slot.used == false)
                slot.GetComponent<ObjectDisplayController>().Display(_value, _value);
    }

    ///<summary>
    /// Set temperature attribute and create/update related sensor object.
    ///</summary>
    ///<param name="_value">The temperature value</param>
    ///<param name="_sensorName">The sensor to modify</param>
    public void SetTemperature(float _value, string _sensorName, string _srcHeightUnit)
    {
        if (transform.Find(_sensorName) is Transform t)
            t.GetComponent<Sensor>().SetTemperature(_value);
        else
        {
            GameManager.instance.AppendLogLine(new ExtendedLocalizedString("Logs", "Sensor doesn't exist", new List<string>() { id, _sensorName }), ELogTarget.both, ELogtype.warning);
            return;
        }

        Sensor sensor = transform.Find("sensor") is Transform meanSensor ? meanSensor.GetComponent<Sensor>() : CreateMeanSensor(_srcHeightUnit);
        sensor.SetTemperature(GetTemperatureInfos().mean);
    }

    /// <summary>
    /// Compute recursively temperature average, standard deviation, min, max and hottes child of the object
    /// </summary>
    /// <returns>a STemp instance containg all temperature infos of the object</returns>
    public STemp GetTemperatureInfos()
    {
        List<(float temp, float volume, string childName)> temps = new();
        List<(float temp, string sensorName)> sensorsTemps = new();
        foreach (Transform child in transform)
        {
            Item childItem = child.GetComponent<Item>();
            if (childItem)
                temps.Add((childItem.GetTemperatureInfos().mean, Utils.VolumeOfMesh(child.GetChild(0).GetComponent<MeshFilter>()), childItem.name));
            else if (child.GetComponent<Sensor>() is Sensor childSensor && childSensor.fromTemplate && !float.IsNaN(childSensor.temperature))
                sensorsTemps.Add((childSensor.temperature, childSensor.name));
        }

        float mean = float.NaN;
        float std = float.NaN;
        float min = float.NaN;
        float max = float.NaN;
        string hottestChild = "";

        List<(float temp, float volume, string childName)> tempsNoNaN = temps.Where(v => !float.IsNaN(v.temp)).ToList();
        float totalEffectiveVolume = tempsNoNaN.Sum(v => v.volume);
        if (sensorsTemps.Count() > 0)
        {
            totalEffectiveVolume = Utils.VolumeOfMesh(transform.GetChild(0).GetComponent<MeshFilter>()) - temps.Where(v => float.IsNaN(v.temp)).Sum(v => v.volume);
            float sensorVolume = (totalEffectiveVolume - tempsNoNaN.Sum(v => v.volume)) / sensorsTemps.Count();

            sensorsTemps.ForEach(sensor => tempsNoNaN.Add((sensor.temp, sensorVolume, sensor.sensorName)));
        }
        if (tempsNoNaN.Count() > 0)
        {
            mean = tempsNoNaN.Sum(v => v.temp * v.volume) / totalEffectiveVolume;
            std = Mathf.Sqrt(tempsNoNaN.Sum(v => Mathf.Pow(v.temp - mean, 2) * v.volume) / totalEffectiveVolume);
            min = tempsNoNaN.Min(v => v.temp);
            max = tempsNoNaN.Max(v => v.temp);
            hottestChild = tempsNoNaN.Where(v => v.temp >= max).First().childName;
        }
        return new(mean, std, min, max, hottestChild, temperatureUnit);
    }

    ///<summary>
    /// Generate a sensor (from GameManager.sensorExtModel) and setup its <see cref="DisplayObjectData"/>.
    ///</summary>
    ///<returns>The created sensor</returns>
    public Sensor CreateMeanSensor(string _srcHeightUnit)
    {
        Vector3 parentSize = transform.GetChild(0).localScale;

        GameObject newSensor = Instantiate(GameManager.instance.sensorExtModel, transform);
        newSensor.name = "sensor";

        Vector3 shapeSize = newSensor.transform.GetChild(0).localScale;
        newSensor.transform.localPosition = new(shapeSize.x / 2, parentSize.y - shapeSize.y / 2, parentSize.z);
        if (this is Rack)
        {
            float uXSize = _srcHeightUnit switch
            {
                LengthUnit.U => UnitValue.U,
                LengthUnit.OU => UnitValue.OU,
                LengthUnit.Millimeter => 0.001f,
                LengthUnit.Centimeter => 0.01f,
                LengthUnit.Meter => 1.0f,
                _ => 1
            };
            newSensor.transform.localPosition += uXSize * Vector3.right;
        }

        Sensor sensor = newSensor.GetComponent<Sensor>();
        sensor.fromTemplate = false;

        DisplayObjectData dod = newSensor.GetComponent<DisplayObjectData>();
        dod.PlaceTexts(LabelPos.Front);
        dod.SetLabel($"{sensor.temperature:0.##} {sensor.temperatureUnit}");
        dod.SwitchLabel((ELabelMode)UiManager.instance.labelsDropdown.value);

        return sensor;
    }

    ///<summary>
    /// Move this object to its position in a room according to the API data.
    ///</summary>
    ///<param name="_apiObj">The SApiObject containing relevant positionning data</param>
    public void PlaceInRoom(SApiObject _apiObj)
    {
        Room parentRoom = transform.parent.GetComponent<Room>();
        float posXYUnit = GetUnitFromAttributes(_apiObj);

        // technicalZone is null for a nonSquareRoom
        Transform zone = (posXYUnit != UnitValue.Tile && parentRoom.technicalZone) ? parentRoom.technicalZone : parentRoom.usableZone;
        transform.position = zone.position;
        Vector3 origin = zone.localScale / 0.2f;

        Vector2 orient = parentRoom.attributes["axisOrientation"] switch
        {
            AxisOrientation.XMinus => new(-1, 1),
            AxisOrientation.YMinus => new(1, -1),
            AxisOrientation.BothMinus => new(-1, -1),
            _ => new(1, 1)
        };

        Vector3 pos;
        if ((_apiObj.category == Category.Rack || _apiObj.category == Category.Corridor || _apiObj.category == Category.Generic) && _apiObj.attributes.ContainsKey("posXYZ"))
            pos = ((JArray)_apiObj.attributes["posXYZ"]).ToVector3().ZAxisUp();
        else
        {
            Vector2 tmp = ((JArray)_apiObj.attributes["posXY"]).ToVector2();
            pos = new(tmp.x, 0, tmp.y);
        }

        Transform floor = transform.parent.Find("Floor");
        if (!parentRoom.isSquare && posXYUnit == UnitValue.Tile && floor)
        {
            int trunkedX = (int)pos.x;
            int trunkedZ = (int)pos.z;
            foreach (Transform tileObj in floor)
            {
                Tile tile = tileObj.GetComponent<Tile>();
                if (tile.coord.x == trunkedX && tile.coord.y == trunkedZ)
                {
                    transform.localPosition += new Vector3(tileObj.localPosition.x - 5 * tileObj.localScale.x, pos.y / 100, tileObj.localPosition.z - 5 * tileObj.localScale.z);
                    transform.localPosition += UnitValue.Tile * new Vector3(orient.x * (pos.x - trunkedX), 0, orient.y * (pos.z - trunkedZ));
                    return;
                }
            }
        }

        // Go to the right corner of the room & apply pos
        if (parentRoom.isSquare)
            transform.localPosition += new Vector3(origin.x * -orient.x, 0, origin.z * -orient.y);

        transform.localPosition += new Vector3(pos.x * orient.x * posXYUnit, pos.y / 100, pos.z * orient.y * posXYUnit);
        transform.transform.localEulerAngles = ((JArray)_apiObj.attributes["rotation"]).ToVector3().ZAxisUp();
    }

}
