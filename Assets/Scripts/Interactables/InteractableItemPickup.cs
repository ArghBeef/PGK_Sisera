using UnityEngine;

public class InteractableItemPickup : MonoBehaviour, IInteractable
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

        PlayerEquipment equipment = interactor.GetComponent<PlayerEquipment>();

        if (equipment == null)
            equipment = interactor.GetComponentInChildren<PlayerEquipment>();

        if (equipment == null)
        {
            Debug.LogWarning("PlayerEquipment not found on interactor.");
            return;
        }

        bool pickedUp = equipment.TryPickup(itemDefinition);

        if (!pickedUp)
            return;

        Debug.Log($"Picked up {itemDefinition.displayName}");

        if (destroyOnPickup)
            Destroy(gameObject);
    }
}