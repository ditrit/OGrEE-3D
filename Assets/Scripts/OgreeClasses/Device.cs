using System.Collections.Generic;

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
}
