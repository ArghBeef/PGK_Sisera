using UnityEngine;

public abstract class ItemDefinition : ScriptableObject
{
    [Header("Info")]
    public string itemId;
    public string displayName;
    [TextArea] public string description;
    public Sprite icon;

    [Header("Prefab")]
    public GameObject worldPrefab;

    public abstract void Use(GameObject user, PlayerEquipment equipment);
}