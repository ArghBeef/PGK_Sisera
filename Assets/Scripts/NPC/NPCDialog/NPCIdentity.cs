using UnityEngine;

public class NPCIdentity : MonoBehaviour
{
    [SerializeField] private string npcId;

    public string NpcId => npcId;

    private void Reset()
    {
        if (string.IsNullOrWhiteSpace(npcId))
            npcId = gameObject.name;
    }

    private void OnValidate()
    {
        if (string.IsNullOrWhiteSpace(npcId))
            npcId = gameObject.name;
    }
}