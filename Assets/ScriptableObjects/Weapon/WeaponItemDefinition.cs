using UnityEngine;

[CreateAssetMenu(fileName = "WeaponItem_", menuName = "Items/Weapon Item Definition")]
public class WeaponItemDefinition : ItemDefinition
{
    public WeaponDefinition weaponData;

    public override void Use(GameObject user, PlayerEquipment equipment)
    {
        Debug.Log("Weapon is not used as a consumable item.");
    }
}