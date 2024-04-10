using System.Collections.Generic;
using UnityEngine;

public class Device : Item
{
    public bool isComponent = false;
    public List<Slot> takenSlots = new();

    protected override void OnDestroy()
    {
        base.OnDestroy();
        foreach (Slot slot in takenSlots)
            slot.SlotTaken(false);
    }

    public override void UpdateFromSApiObject(SApiObject _src)
    {
        if ((HasAttributeChanged(_src, "posU")
            || HasAttributeChanged(_src, "slot")
            || HasAttributeChanged(_src, "orientation")
            || HasAttributeChanged(_src, "invertOffset"))
            && transform.parent)
        {
            PlaceDevice(_src);
            group?.ShapeGroup();
        }

        if (domain != _src.domain)
            UpdateColorByDomain(_src.domain);

        if (HasAttributeChanged(_src, "color"))
            SetColor(_src.attributes["color"]);

        base.UpdateFromSApiObject(_src);
    }

    /// <summary>
    /// Move the given device to its position in a rack according to the API data.
    /// </summary>
    /// <param name="_apiObj">The SApiObject containing relevant positionning data</param>
    public void PlaceDevice(SApiObject _apiObj)
    {
        foreach (Slot slot in takenSlots)
            slot.SlotTaken(false);
        takenSlots.Clear();

        // Check slot
        Transform primarySlot = null;
        Vector3 slotsScale = new();
        if (transform.parent && _apiObj.attributes.HasKeyAndValue("slot"))
        {
            string slots = _apiObj.attributes["slot"].Trim('[', ']');
            string[] slotsArray = slots.Split(",");

            foreach (Transform child in transform.parent)
            {
                if (child.TryGetComponent(out Slot slot))
                {
                    foreach (string slotName in slotsArray)
                    {
                        if (child.name == slotName)
                        {
                            takenSlots.Add(slot);
                            slot.SlotTaken(true);
                        }
                    }
                }
            }
            if (takenSlots.Count > 0)
            {
                SlotsShape(out Vector3 slotsPivot, out slotsScale);
                primarySlot = takenSlots[0].transform;
                foreach (Slot slot in takenSlots)
                {
                    if (Vector3.Distance(slotsPivot, slot.transform.position) < Vector3.Distance(slotsPivot, primarySlot.position))
                        primarySlot = slot.transform;
                }
            }
            else
            {
                GameManager.instance.AppendLogLine($"One or more slots from {_apiObj.attributes["slot"]} not found in {transform.parent.name}", ELogTarget.both, ELogtype.error);
                return;
            }
        }
        foreach (Slot s in takenSlots)
            if (!takenSlots.Contains(s))
                s.SlotTaken(false);

        Vector2 size;
        float height;
        if (!_apiObj.attributes.HasKeyAndValue("template"))//Rescale according to slot or parent if basic object
        {
            Vector3 scale;
            if (takenSlots.Count > 0)
                scale = new(takenSlots[0].transform.GetChild(0).localScale.x, Utils.ParseDecFrac(_apiObj.attributes["height"]) / 1000, takenSlots[0].transform.GetChild(0).localScale.z);
            else
                scale = new(transform.parent.GetChild(0).localScale.x, Utils.ParseDecFrac(_apiObj.attributes["height"]) / 1000, transform.parent.GetChild(0).localScale.z);
            transform.GetChild(0).localScale = scale;
            transform.GetChild(0).GetComponent<Collider>().enabled = true;

            foreach (Transform child in transform)
                child.localPosition = scale / 2;
            size = new(scale.x, scale.z);
            height = scale.y;
        }
        else
        {
            size = Utils.ParseVector2(_apiObj.attributes["size"]) / 1000;
            height = Utils.ParseDecFrac(_apiObj.attributes["height"]) / 1000;
        }

        // Place the device
        if (_apiObj.attributes.HasKeyAndValue("slot"))
        {
            // parent to slot for applying orientation
            Transform savedParent = transform.parent;
            transform.parent = primarySlot;
            transform.localEulerAngles = Vector3.zero;
            transform.localPosition = Vector3.zero;

            float deltaZ = slotsScale.z - size.y;
            switch (_apiObj.attributes["orientation"])
            {
                case Orientation.Front:
                    transform.localPosition += new Vector3(0, 0, deltaZ);
                    break;
                case Orientation.Rear:
                    transform.localEulerAngles += new Vector3(0, 180, 0);
                    transform.localPosition += new Vector3(size.x, 0, size.y);
                    break;
                case Orientation.FrontFlipped:
                    transform.localEulerAngles += new Vector3(0, 0, 180);
                    transform.localPosition += new Vector3(size.x, height, deltaZ);
                    break;
                case Orientation.RearFlipped:
                    transform.localEulerAngles += new Vector3(180, 0, 0);
                    transform.localPosition += new Vector3(0, height, size.y);
                    break;
            }
            // align device to right side of the slot if invertOffset == true
            if (_apiObj.attributes.ContainsKey("invertOffset") && _apiObj.attributes["invertOffset"] == "true")
                transform.localPosition += new Vector3(slotsScale.x - size.x, 0, 0);
            // parent back to _parent for good hierarchy 
            transform.parent = savedParent;

            if (!_apiObj.attributes.ContainsKey("color"))
            {
                // if slot, color
                Color slotColor = primarySlot.GetChild(0).GetComponent<Renderer>().material.color;
                color = new(slotColor.r, slotColor.g, slotColor.b);
                GetComponent<ObjectDisplayController>().ChangeColor(slotColor);
                hasSlotColor = true;
            }
        }
        else
        {
            Vector3 parentShape = transform.parent.GetChild(0).localScale;
            transform.localEulerAngles = Vector3.zero;
            transform.localPosition = Vector3.zero;
            if (_apiObj.attributes.ContainsKey("posU"))
                transform.localPosition += new Vector3(0, (Utils.ParseDecFrac(_apiObj.attributes["posU"]) - 1) * UnitValue.U, 0);

            float deltaX = parentShape.x - size.x;
            float deltaZ = parentShape.z - size.y;
            transform.localPosition += new Vector3(deltaX / 2, 0, deltaZ);
        }
        // Set labels
        DisplayObjectData dod = GetComponent<DisplayObjectData>();
        if (primarySlot)
            dod.PlaceTexts(primarySlot.GetComponent<Slot>().labelPos);
        else
            dod.PlaceTexts(LabelPos.FrontRear);
    }

    /// <summary>
    /// Get <paramref name="_pivot"/> and <paramref name="_scale"/> of all slots combined
    /// </summary>
    /// <param name="_pivot">The pivot of the combined slots</param>
    /// <param name="_scale">The scale of the combined slots</param>
    private void SlotsShape(out Vector3 _pivot, out Vector3 _scale)
    {
        Quaternion parentRot = transform.parent.rotation;
        transform.parent.rotation = Quaternion.identity;

        // x axis
        float left = float.PositiveInfinity;
        float right = float.NegativeInfinity;
        // y axis
        float bottom = float.PositiveInfinity;
        float top = float.NegativeInfinity;
        // z axis
        float rear = float.PositiveInfinity;
        float front = float.NegativeInfinity;

        foreach (Slot slot in takenSlots)
        {
            Bounds bounds = slot.transform.GetChild(0).GetComponent<Renderer>().bounds;
            left = Mathf.Min(bounds.min.x, left);
            right = Mathf.Max(bounds.max.x, right);
            bottom = Mathf.Min(bounds.min.y, bottom);
            top = Mathf.Max(bounds.max.y, top);
            rear = Mathf.Min(bounds.min.z, rear);
            front = Mathf.Max(bounds.max.z, front);
        }

        _scale = new(right - left, top - bottom, front - rear);
        _pivot = new(left, bottom, rear);

        transform.parent.rotation = parentRot;
    }
}
