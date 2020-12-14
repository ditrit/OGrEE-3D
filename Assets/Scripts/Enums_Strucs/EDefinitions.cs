///<summary>
/// Cardinal points (+x,+y): EN, NW, WS, SE
///</summary>
public enum ECardinalOrient
{
    EN,
    WS,
    SE,
    NW
}

///<summary>
/// Orientation of an object, relative to its parent: Forward / Backward / Left / Right
///</summary>
public enum EObjOrient
{
    Front,
    Rear,
    Left,
    Right,
    FrontFlipped,
    RearFlipped
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
