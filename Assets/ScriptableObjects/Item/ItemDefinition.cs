using UnityEngine;

[CreateAssetMenu(fileName = "Item_", menuName = "Items/Item Definition")]
public class ItemDefinition : ScriptableObject
{
    [Header("Info")]
    public string itemId;
    public string displayName;
    [TextArea] public string description;
    public Sprite icon;

    [Header("Type")]
    public ItemCategory category;

    [Header("Stacking")]
    public bool stackable = false;
    [Min(1)] public int maxStack = 1;

    [Header("Prefab")]
    public GameObject worldPrefab;

    public virtual void UseHotBar(GameObject user)
    {
        Debug.Log($"{displayName} SELECTED");
    }

    public virtual void OnHotbarDeselected(GameObject user)
    {
        Debug.Log($"{displayName} DESELECTED");
    }

    public virtual void OnActiveItemAction(GameObject user)
    {
        Debug.Log($"{displayName} ACTION");
    }
}