///<summary>
/// World orientation: North / South / East / West
///</summary>
public enum EOrientation
{
    N,
    S,
    E,
    W
}

///<summary>
/// Orientation of an object, relative to its parent: Forward / Backward / Left / Right
///</summary>
public enum EObjOrient
{
    Frontward,
    Backward,
    Left,
    Right
}

///<summary>
/// Unit: m / mm / inch / feet / tile / U / OU
///</summary>
public enum EUnit
{
    m,
    cm,
    mm,
    inch,
    feet,
    tile,
    U,
    OU
}

///<summary>
/// The type of object: Datacenter / Room / Itroom / Rack / Powerpanel / Airconditionner / Chassis / Device / Pdu / Container
///</summary>
public enum EObjectType
{
    Datacenter,
    Room,
    Itroom,
    Rack,
    Powerpanel,
    Airconditionner,
    Chassis,
    Device,
    Pdu,
    Container
}

///<summary>
/// Role: parent / child
///</summary>
public enum ERole
{
    Parent,
    Child
}

///<summary>
/// ComponentType: processor / psu / fan / adapter / enclosure / memory / disk
///</summary>
public enum EComponentType
{
    NA,
    processor,
    psu,
    fan,
    adapter,
    enclosure,
    memory,
    disk
}
