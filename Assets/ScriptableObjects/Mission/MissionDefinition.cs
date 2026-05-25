using UnityEngine;

[CreateAssetMenu(fileName = "Mission_", menuName = "Missions/Mission Definition")]
public class MissionDefinition : ScriptableObject
{
    [Header("Info")]
    public string missionName;
    [TextArea] public string description;

    [Header("Type")]
    public MissionType missionType;

    [Header("Main Timer")]
    [Min(1f)] public float missionTimeLimit = 300f;

    [Header("Demolition")]
    [Min(1f)] public float captureTime = 5f;
    [Min(1f)] public float holdTime = 20f;

    [Header("Assassination")]
    [Min(1f)] public float escapeTimeAfterKill = 60f;
}