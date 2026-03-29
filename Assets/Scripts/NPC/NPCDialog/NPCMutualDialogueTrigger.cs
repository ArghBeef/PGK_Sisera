using UnityEngine;

[RequireComponent(typeof(NPCDialogueController))]
public class NPCMutualDialogueTrigger : MonoBehaviour
{
    [SerializeField] private NPCDialogueController dialogueController;

    private void Awake()
    {
        if (dialogueController == null)
            dialogueController = GetComponent<NPCDialogueController>();
    }

    public void TryDialogueWith(GameObject otherObject)
    {
        if (otherObject == null)
            return;

        NPCDialogueController otherDialogue = otherObject.GetComponent<NPCDialogueController>();
        if (otherDialogue == null)
            return;

        if (dialogueController == null)
            return;

        if (!dialogueController.CanTalkTo(otherDialogue))
            return;

        if (!otherDialogue.CanTalkTo(dialogueController))
            return;

        dialogueController.TryStartDialogueWith(otherDialogue);
    }
}