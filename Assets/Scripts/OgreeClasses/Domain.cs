using System.Collections;

public class Domain : OgreeObject
{
    public override void UpdateFromSApiObject(SApiObject _src)
    {
        name = _src.name;
        id = _src.id;
        parentId = _src.parentId;
        category = _src.category;
        domain = _src.domain;
        description = _src.description;
        attributes = _src.attributes;
        // attributes.Clear();
        // foreach (DictionaryEntry de in _src.attributes)
        //     attributes.Add((string)de.Key, de.Value);
    }
}
