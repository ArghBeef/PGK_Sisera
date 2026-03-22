using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerHotbar : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputActionReference cycleSlotAction;
    [SerializeField] private InputActionReference activeItemAction;

    [Header("Hotbar")]
    [SerializeField] private HotbarSlot[] slots;

    public ItemDefinition ActiveItem { get; private set; }
    public int ActiveIndex { get; private set; } = -1;

    public event Action<int, ItemDefinition> OnActiveSlotChanged;
    public event Action<int, ItemDefinition> OnSlotUpdated;

    private void Reset()
    {
        CreateDefaultSlots();
    }

    private void Awake()
    {
        if (slots == null || slots.Length == 0)
            CreateDefaultSlots();
    }

    private void OnEnable()
    {
        if (cycleSlotAction != null)
            cycleSlotAction.action.Enable();

        if (activeItemAction != null)
            activeItemAction.action.Enable();
    }

    private void OnDisable()
    {
        if (cycleSlotAction != null)
            cycleSlotAction.action.Disable();

        if (activeItemAction != null)
            activeItemAction.action.Disable();
    }

    private void Update()
    {
        HandleSlotCycleInput();
        HandleActiveItemActionInput();
    }

    private void CreateDefaultSlots()
    {
        slots = new HotbarSlot[7];
        slots[0] = new HotbarSlot(ItemCategory.Weapon);
        slots[1] = new HotbarSlot(ItemCategory.Weapon);
        slots[2] = new HotbarSlot(ItemCategory.Utility);
        slots[3] = new HotbarSlot(ItemCategory.Utility);
        slots[4] = new HotbarSlot(ItemCategory.UseItem);
        slots[5] = new HotbarSlot(ItemCategory.UseItem);
        slots[6] = new HotbarSlot(ItemCategory.UseItem);
    }

    private void HandleSlotCycleInput()
    {
        if (cycleSlotAction == null)
            return;

        if (cycleSlotAction.action.WasPressedThisFrame())
        {
            SelectNextSlot();
        }
    }

    private void HandleActiveItemActionInput()
    {
        if (activeItemAction == null || ActiveItem == null)
            return;

        if (activeItemAction.action.WasPressedThisFrame())
        {
            ActiveItem.OnActiveItemAction(gameObject);
        }
    }

    public void SelectNextSlot()
    {
        if (slots == null || slots.Length == 0)
            return;

        int startIndex = ActiveIndex;

        for (int i = 0; i < slots.Length; i++)
        {
            int nextIndex = (startIndex + 1 + i) % slots.Length;

            if (!slots[nextIndex].IsEmpty)
            {
                ActivateSlot(nextIndex);
                return;
            }
        }

        int fallback = (ActiveIndex + 1) % slots.Length;
        ActivateSlot(fallback);
    }

    public void ActivateSlot(int slotIndex)
    {
        if (!IsValidSlot(slotIndex))
            return;

        ItemDefinition nextItem = slots[slotIndex].Item;

        if (ActiveItem != null)
            ActiveItem.OnHotbarDeselected(gameObject);

        ActiveIndex = slotIndex;
        ActiveItem = nextItem;

        if (ActiveItem != null)
            ActiveItem.UseHotBar(gameObject);

        OnActiveSlotChanged?.Invoke(ActiveIndex, ActiveItem);

        Debug.Log($"Selected hotbar slot {ActiveIndex + 1}");
    }

    public bool AssignItemToSlot(ItemDefinition item, int slotIndex)
    {
        if (!IsValidSlot(slotIndex) || item == null)
            return false;

        if (!slots[slotIndex].TrySetItem(item))
        {
            Debug.LogWarning(
                $"Cannot place {item.displayName} ({item.category}) in slot {slotIndex + 1}. " +
                $"Slot accepts only {slots[slotIndex].AllowedCategory}.");
            return false;
        }

        OnSlotUpdated?.Invoke(slotIndex, item);
        return true;
    }

    public void ClearSlot(int slotIndex)
    {
        if (!IsValidSlot(slotIndex))
            return;

        bool wasActive = slotIndex == ActiveIndex;

        slots[slotIndex].Clear();
        OnSlotUpdated?.Invoke(slotIndex, null);

        if (wasActive)
        {
            ActiveItem = null;
            OnActiveSlotChanged?.Invoke(ActiveIndex, null);
        }
    }

    public ItemDefinition GetSlotItem(int slotIndex)
    {
        if (!IsValidSlot(slotIndex))
            return null;

        return slots[slotIndex].Item;
    }

    public ItemCategory GetSlotCategory(int slotIndex)
    {
        if (!IsValidSlot(slotIndex))
            return ItemCategory.None;

        return slots[slotIndex].AllowedCategory;
    }

    public int SlotCount => slots != null ? slots.Length : 0;

    private bool IsValidSlot(int index)
    {
        return slots != null && index >= 0 && index < slots.Length;
    }
}