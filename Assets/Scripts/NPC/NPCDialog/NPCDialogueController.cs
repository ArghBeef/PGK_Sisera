using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NPCIdentity))]
public class NPCDialogueController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private NPCIdentity identity;
    [SerializeField] private NPCDialogueUI dialogueUI;
    [SerializeField] private NavMeshAgent agent;

    [Header("Dialogue Data")]
    [SerializeField] private List<NPCDialogueEntry> dialogues = new List<NPCDialogueEntry>();

    [Header("Timing")]
    [SerializeField] private float delayBeforeReply = 0.75f;
    [SerializeField] private float distanceToStopDialogue = 5f;

    [Header("Optional")]
    [SerializeField] private bool stopMovementDuringDialogue = true;
    [SerializeField] private bool rotateTowardSpeaker = true;
    [SerializeField] private float rotationSpeed = 7f;

    private NPCDialogueController currentPartner;
    private NPCDialogueEntry currentOwnEntry;
    private NPCDialogueEntry currentPartnerEntry;

    private Coroutine dialogueRoutine;
    private bool isInDialogue;
    private bool isDialogueLocked;

    public bool IsInDialogue => isInDialogue;
    public bool IsDialogueLocked => isDialogueLocked;

    private void Awake()
    {
        if (identity == null)
            identity = GetComponent<NPCIdentity>();

        if (agent == null)
            agent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        if (!isInDialogue || currentPartner == null)
            return;

        float distance = Vector3.Distance(transform.position, currentPartner.transform.position);
        if (distance > distanceToStopDialogue && !isDialogueLocked)
        {
            ForceEndDialogue(false);
            return;
        }

        if (rotateTowardSpeaker)
        {
            Vector3 direction = currentPartner.transform.position - transform.position;
            direction.y = 0f;

            if (direction.sqrMagnitude > 0.001f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction.normalized);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
        }
    }

    public bool TryStartDialogueWith(NPCDialogueController other)
    {
        if (other == null || other == this)
            return false;

        if (identity == null || other.identity == null)
            return false;

        if (isInDialogue || other.isInDialogue)
            return false;

        NPCDialogueEntry myEntry = GetDialogueFor(other.identity.NpcId);
        NPCDialogueEntry otherEntry = other.GetDialogueFor(identity.NpcId);

        if (myEntry == null || otherEntry == null)
            return false;

        if (myEntry.lines == null || myEntry.lines.Count == 0)
            return false;

        if (otherEntry.lines == null || otherEntry.lines.Count == 0)
            return false;

        bool oneTime = myEntry.oneTimeOnly || otherEntry.oneTimeOnly;
        if (oneTime && NPCDialogueMemory.HasCompleted(identity.NpcId, other.identity.NpcId))
            return false;

        BeginDialogue(other, myEntry, otherEntry);
        other.BeginDialogue(this, otherEntry, myEntry);

        dialogueRoutine = StartCoroutine(RunDialogueConversation());
        return true;
    }

    private void BeginDialogue(NPCDialogueController partner, NPCDialogueEntry ownEntry, NPCDialogueEntry partnerEntry)
    {
        currentPartner = partner;
        currentOwnEntry = ownEntry;
        currentPartnerEntry = partnerEntry;
        isInDialogue = true;
        isDialogueLocked = ownEntry.priority == NPCDialoguePriority.High || partnerEntry.priority == NPCDialoguePriority.High;

        if (stopMovementDuringDialogue && agent != null)
        {
            agent.isStopped = true;
            agent.ResetPath();
        }
    }

    private IEnumerator RunDialogueConversation()
    {
        int maxCount = Mathf.Max(currentOwnEntry.lines.Count, currentPartnerEntry.lines.Count);

        for (int i = 0; i < maxCount; i++)
        {
            if (currentOwnEntry != null && i < currentOwnEntry.lines.Count)
            {
                NPCDialogueLine myLine = currentOwnEntry.lines[i];
                ShowLine(myLine.text);
                yield return new WaitForSeconds(myLine.duration);
                HideLine();
            }

            yield return new WaitForSeconds(delayBeforeReply);

            if (currentPartner != null && currentPartner.currentOwnEntry != null && i < currentPartner.currentOwnEntry.lines.Count)
            {
                NPCDialogueLine partnerLine = currentPartner.currentOwnEntry.lines[i];
                currentPartner.ShowLine(partnerLine.text);
                yield return new WaitForSeconds(partnerLine.duration);
                currentPartner.HideLine();
            }

            yield return new WaitForSeconds(delayBeforeReply);

            if (currentPartner == null || !currentPartner.isInDialogue)
                yield break;
        }

        CompleteDialogue();
    }

    private void CompleteDialogue()
    {
        if (currentPartner != null && identity != null && currentPartner.identity != null)
        {
            bool oneTime = currentOwnEntry.oneTimeOnly || currentPartnerEntry.oneTimeOnly;
            if (oneTime)
                NPCDialogueMemory.MarkCompleted(identity.NpcId, currentPartner.identity.NpcId);

            NPCDialogueController partner = currentPartner;
            CleanupDialogueState();
            partner.CleanupDialogueState();
        }
        else
        {
            CleanupDialogueState();
        }
    }

    public bool InterruptIfAllowed()
    {
        if (!isInDialogue)
            return true;

        if (isDialogueLocked)
            return false;

        ForceEndDialogue(false);
        return true;
    }

    public void ForceEndDialogue(bool markCompleted)
    {
        if (!isInDialogue)
            return;

        if (markCompleted && currentPartner != null && identity != null && currentPartner.identity != null)
        {
            NPCDialogueMemory.MarkCompleted(identity.NpcId, currentPartner.identity.NpcId);
        }

        NPCDialogueController partner = currentPartner;

        CleanupDialogueState();

        if (partner != null)
            partner.CleanupDialogueState();
    }

    private void CleanupDialogueState()
    {
        if (dialogueRoutine != null)
        {
            StopCoroutine(dialogueRoutine);
            dialogueRoutine = null;
        }

        HideLine();

        isInDialogue = false;
        isDialogueLocked = false;
        currentPartner = null;
        currentOwnEntry = null;
        currentPartnerEntry = null;

        if (stopMovementDuringDialogue && agent != null)
            agent.isStopped = false;
    }

    private NPCDialogueEntry GetDialogueFor(string otherNpcId)
    {
        for (int i = 0; i < dialogues.Count; i++)
        {
            if (dialogues[i] != null && dialogues[i].targetNpcId == otherNpcId)
                return dialogues[i];
        }

        return null;
    }

    public bool CanTalkTo(NPCDialogueController other)
    {
        if (other == null || identity == null || other.identity == null)
            return false;

        NPCDialogueEntry myEntry = GetDialogueFor(other.identity.NpcId);
        NPCDialogueEntry otherEntry = other.GetDialogueFor(identity.NpcId);

        if (myEntry == null || otherEntry == null)
            return false;

        bool oneTime = myEntry.oneTimeOnly || otherEntry.oneTimeOnly;
        if (oneTime && NPCDialogueMemory.HasCompleted(identity.NpcId, other.identity.NpcId))
            return false;

        return true;
    }

    public NPCDialoguePriority GetPriorityFor(NPCDialogueController other)
    {
        if (other == null || other.identity == null)
            return NPCDialoguePriority.Low;

        NPCDialogueEntry entry = GetDialogueFor(other.identity.NpcId);
        if (entry == null)
            return NPCDialoguePriority.Low;

        return entry.priority;
    }

    public void ShowLine(string text)
    {
        dialogueUI?.ShowText(text);
    }

    public void HideLine()
    {
        dialogueUI?.Hide();
    }
}