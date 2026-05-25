using UnityEngine;

public abstract class PassiveAbilityDefinition : ScriptableObject
{
    public string passiveName;

    public abstract void Apply(GameObject user, PlayerClassController controller);
    public abstract void Remove(GameObject user, PlayerClassController controller);
}