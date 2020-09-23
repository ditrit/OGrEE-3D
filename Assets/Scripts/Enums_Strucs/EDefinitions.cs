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
    inch,
    feet,
    tile,
    U,
    OU
}

///<summary>
/// The role of a device: parent / child
///</summary>
public enum ERole
{
    parent,
    child
}

///<summary>
/// The family of object: Rack / Powerpanel / Airconditionner / Chassis / Device / Pdu / Container
///</summary>
public enum EObjFamily
{
    rack,
    powerpanel,
    airconditionner,
    chassis,
    device,
    pdu,
    container
}

///<summary>
/// Role: parent / child
///</summary>
public enum EDeviceRole
{
    parent,
    child
}

///<summary>
/// ComponentType: processor / psu / fan / adapter / enclosure / memory / disk
///</summary>
public enum ECompCategory
{
    processor,
    psu,
    fan,
    adapter,
    memory,
    disk

}
