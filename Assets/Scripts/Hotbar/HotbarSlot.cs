using System;
using UnityEngine;

[Serializable]
public class HotbarSlot
{
    [SerializeField] private ItemCategory allowedCategory;
    [SerializeField] private ItemDefinition item;

    public ItemCategory AllowedCategory => allowedCategory;
    public ItemDefinition Item => item;
    public bool IsEmpty => item == null;

    public HotbarSlot(ItemCategory allowedCategory)
    {
        this.allowedCategory = allowedCategory;
    }

    public void SetAllowedCategory(ItemCategory category)
    {
        allowedCategory = category;
    }

    public bool CanAccept(ItemDefinition incoming)
    {
        return incoming != null && incoming.category == allowedCategory;
    }

    public bool TrySetItem(ItemDefinition incoming)
    {
        if (incoming == null || !CanAccept(incoming))
            return false;

        item = incoming;
        return true;
    }

    public void ForceSetItem(ItemDefinition incoming)
    {
        item = incoming;
    }

    public void Clear()
    {
        item = null;
    }
}