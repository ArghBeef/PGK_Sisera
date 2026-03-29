using UnityEngine;

public class InteractablePickup : MonoBehaviour, IInteractable
{
    [SerializeField] private ItemDefinition itemDefinition;
    [SerializeField] private bool destroyOnPickup = true;

    public void Interact(GameObject interactor)
    {
        if (itemDefinition == null)
        {
            Debug.LogWarning($"{name} has no ItemDefinition assigned.");
            return;
        }

        PlayerHotbar hotbar = interactor.GetComponent<PlayerHotbar>();

        if (hotbar == null)
            hotbar = interactor.GetComponentInChildren<PlayerHotbar>();

        if (hotbar == null)
        {
            Debug.LogWarning("PlayerHotbar not found on interactor.");
            return;
        }

        int targetSlot = FindFirstCompatibleEmptySlot(hotbar, itemDefinition);

        if (targetSlot < 0)
        {
            Debug.Log($"No compatible free hotbar slot for {itemDefinition.displayName}.");
            return;
        }

        bool added = hotbar.AssignItemToSlot(itemDefinition, targetSlot);

        if (!added)
            return;

        Debug.Log($"Picked up {itemDefinition.displayName} into slot {targetSlot + 1}");

        if (destroyOnPickup)
            Destroy(gameObject);
    }

    private int FindFirstCompatibleEmptySlot(PlayerHotbar hotbar, ItemDefinition item)
    {
        for (int i = 0; i < hotbar.SlotCount; i++)
        {
            if (hotbar.GetSlotItem(i) != null)
                continue;

            if (hotbar.GetSlotCategory(i) == item.category)
                return i;
        }

        return -1;
    }
}