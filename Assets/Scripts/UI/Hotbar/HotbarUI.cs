using UnityEngine;

public class HotbarUI : MonoBehaviour
{
    [SerializeField] private PlayerHotbar playerHotbar;
    [SerializeField] private HotbarSlotUI[] slotUIs;

    private void Start()
    {
        if (playerHotbar == null)
        {
            Debug.LogError("HotbarUI: PlayerHotbar reference is missing.");
            return;
        }

        if (slotUIs == null || slotUIs.Length == 0)
        {
            Debug.LogError("HotbarUI: Slot UI references are missing.");
            return;
        }

        for (int i = 0; i < slotUIs.Length; i++)
        {
            slotUIs[i].Initialize(i);
            slotUIs[i].SetItem(playerHotbar.GetSlotItem(i));
            slotUIs[i].SetSelected(i == playerHotbar.ActiveIndex);
        }

        playerHotbar.OnSlotUpdated += HandleSlotUpdated;
        playerHotbar.OnActiveSlotChanged += HandleActiveSlotChanged;
    }

    private void OnDestroy()
    {
        if (playerHotbar == null)
            return;

        playerHotbar.OnSlotUpdated -= HandleSlotUpdated;
        playerHotbar.OnActiveSlotChanged -= HandleActiveSlotChanged;
    }

    private void HandleSlotUpdated(int slotIndex, ItemDefinition item)
    {
        if (slotIndex < 0 || slotIndex >= slotUIs.Length)
            return;

        slotUIs[slotIndex].SetItem(item);
    }

    private void HandleActiveSlotChanged(int activeIndex, ItemDefinition item)
    {
        for (int i = 0; i < slotUIs.Length; i++)
        {
            slotUIs[i].SetSelected(i == activeIndex);
        }
    }
}