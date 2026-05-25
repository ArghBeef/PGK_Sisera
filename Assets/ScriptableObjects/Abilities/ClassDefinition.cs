using UnityEngine;

[CreateAssetMenu(fileName = "ClassDefinition", menuName = "Classes/Class Definition")]
public class ClassDefinition : ScriptableObject
{
    [Header("Abilities")]
    public AbilityDefinition activeAbility1;
    public AbilityDefinition activeAbility2;
    public AbilityDefinition ultimate;

    [Header("Passives")]
    public PassiveAbilityDefinition[] passiveAbilities;
}