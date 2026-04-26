using UnityEngine;

public abstract class AbilityDefinition : ScriptableObject
{
    public string abilityName;
    [TextArea] public string description;
    public Sprite icon;
    public float cooldown = 5f;

    public abstract void Activate(GameObject user, PlayerClassController classController);
}