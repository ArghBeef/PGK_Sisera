using UnityEngine;

[RequireComponent(typeof(Collider))]
public class NPCDetectionZone : MonoBehaviour
{
    private NPCController owner;

    public void Initialize(NPCController npcOwner)
    {
        owner = npcOwner;
    }

    private void Reset()
    {
        Collider col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (owner != null)
            owner.HandleTargetEnter(other);
    }

    private void OnTriggerStay(Collider other)
    {
        if (owner != null)
            owner.HandleTargetStay(other);
    }

    private void OnTriggerExit(Collider other)
    {
        if (owner != null)
            owner.HandleTargetExit(other);
    }
}