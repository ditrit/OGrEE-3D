using UnityEngine;

public class GenericObject : Item
{
    public override void UpdateFromSApiObject(SApiObject _src)
    {
        if ((HasAttributeChanged(_src,  "posXYZ")
            || HasAttributeChanged(_src,  "posXYUnit")
            || HasAttributeChanged(_src,  "rotation"))
            && transform.parent)
        {
            PlaceInRoom(_src);
            if (group)
                group.ShapeGroup();
        }

        if (string.IsNullOrEmpty(domain) || (domain != _src.domain && color.Equals(((GameObject)GameManager.instance.allItems[domain]).GetComponent<Domain>().GetColor())))
        {
            domain = _src.domain;
            UpdateColorByDomain();
        }

        if (HasAttributeChanged(_src,  "color"))
            SetColor(_src.attributes["color"]);

        base.UpdateFromSApiObject(_src);
    }
}
