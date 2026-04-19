using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerEquipment : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputActionReference useItemAction;
    [SerializeField] private InputActionReference dropItemAction;

    [Header("Equipment")]
    [SerializeField] private WeaponItemDefinition equippedWeapon;
    [SerializeField] private ItemDefinition equippedItem;

    public WeaponItemDefinition EquippedWeapon => equippedWeapon;
    public ItemDefinition EquippedItem => equippedItem;

    public bool HasWeapon => equippedWeapon != null;
    public bool HasItem => equippedItem != null;
    public bool CanUseWeapon => equippedWeapon != null && equippedItem == null;

    public event Action<WeaponItemDefinition> OnWeaponChanged;
    public event Action<ItemDefinition> OnItemChanged;

    private void OnEnable()
    {
        if (useItemAction != null)
            useItemAction.action.Enable();

        if (dropItemAction != null)
            dropItemAction.action.Enable();
    }

    private void OnDisable()
    {
        if (useItemAction != null)
            useItemAction.action.Disable();

        if (dropItemAction != null)
            dropItemAction.action.Disable();
    }

    private void Update()
    {
        if (HasItem && useItemAction != null && useItemAction.action.WasPressedThisFrame())
        {
            equippedItem.Use(gameObject, this);
        }

        if (HasItem && dropItemAction != null && dropItemAction.action.WasPressedThisFrame())
        {
            DropItem();
        }
    }

    public bool TryPickup(ItemDefinition item)
    {
        if (item == null)
            return false;

        if (item is WeaponItemDefinition weaponItem)
        {
            EquipWeapon(weaponItem);
            return true;
        }

        EquipItem(item);
        return true;
    }

    public void EquipWeapon(WeaponItemDefinition weapon)
    {
        equippedWeapon = weapon;
        OnWeaponChanged?.Invoke(equippedWeapon);

        Debug.Log($"Equipped weapon: {(equippedWeapon != null ? equippedWeapon.displayName : "None")}");
    }

    public void EquipItem(ItemDefinition item)
    {
        if (item == null)
            return;

        if (item is WeaponItemDefinition)
        {
            Debug.LogWarning("Use EquipWeapon for weapons.");
            return;
        }

        equippedItem = item;
        OnItemChanged?.Invoke(equippedItem);

        Debug.Log($"Equipped item: {equippedItem.displayName}. Weapon disabled until item is used or dropped.");
    }

    public void ClearItemSlot()
    {
        equippedItem = null;
        OnItemChanged?.Invoke(null);

        Debug.Log("Item cleared. Weapon active again.");
    }

    public void DropItem()
    {
        if (equippedItem == null)
            return;

        Debug.Log($"Dropped item: {equippedItem.displayName}");

        if (equippedItem.worldPrefab != null)
        {
            Vector3 dropPosition = transform.position + transform.forward * 1.2f;
            dropPosition.y = transform.position.y + 0.25f;

            Instantiate(equippedItem.worldPrefab, dropPosition, Quaternion.identity);
        }

        ClearItemSlot();
    }
}