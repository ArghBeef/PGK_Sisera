using UnityEngine;

[CreateAssetMenu(fileName = "Class_", menuName = "Classes/Player Class")]
public class ClassDefinition : ScriptableObject
{
    public string classId;
    public string displayName;

    [Header("Active Abilities")]
    public AbilityDefinition activeAbility1;
    public AbilityDefinition activeAbility2;
    public AbilityDefinition ultimate;

    [Header("Passives")]
    public bool damageHealsPlayer;
    [Range(0f, 1f)] public float damageHealPercent = 0.2f;

    public bool stunnedEnemiesTakeMoreDamage;
    public float stunnedDamageMultiplier = 1.5f;

    public bool stunnedKillsAreLessVisible = true;

    public bool louderPlayerSounds = true;
    public float soundMultiplier = 2f;

    public bool longerNpcSuspicion = true;
    public float suspicionDurationMultiplier = 1.5f;
}