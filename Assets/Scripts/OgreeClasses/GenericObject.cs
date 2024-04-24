public class GenericObject : Item
{
    public override void UpdateFromSApiObject(SApiObject _src)
    {
        if ((HasAttributeChanged(_src, "posXYZ")
            || HasAttributeChanged(_src, "posXYUnit")
            || HasAttributeChanged(_src, "rotation"))
            && transform.parent)
        {
            PlaceInRoom(_src);
            group?.ShapeGroup();
        }

        if (domain != _src.domain)
            UpdateColorByDomain(_src.domain);

        if (HasAttributeChanged(_src, "color"))
            SetColor((string)_src.attributes["color"]);

        base.UpdateFromSApiObject(_src);
    }
}
