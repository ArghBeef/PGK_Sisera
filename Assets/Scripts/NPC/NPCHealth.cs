using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

[RequireComponent(typeof(NPCController))]
public class NPCHealth : MonoBehaviour, IDamageable
{
    [Header("Health")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float damageReactionTime = 0.6f;

    [Header("Death")]
    [SerializeField] private string deadInteractableTag = "interactables";
    [SerializeField] private string deadBodyTag = "Body";
    [SerializeField] private bool addDragObjectOnDeath = true;

    [Header("Events")]
    [SerializeField] private UnityEvent onDamaged;
    [SerializeField] private UnityEvent onDeath;

    private float currentHealth;
    private bool dead;

    private NPCController npcController;
    private NPCDialogueController dialogueController;
    private NavMeshAgent agent;
    private Coroutine damageReactionRoutine;

    public bool IsDead => dead;

    private void Awake()
    {
        currentHealth = maxHealth;

        npcController = GetComponent<NPCController>();
        dialogueController = GetComponent<NPCDialogueController>();
        agent = GetComponent<NavMeshAgent>();
    }

    public void TakeDamage(float damage)
    {
        if (dead)
            return;

        if (damage <= 0f)
            return;

        currentHealth -= damage;
        onDamaged?.Invoke();

        if (currentHealth <= 0f)
        {
            Die();
            return;
        }

        ReactToDamage();
    }

    private void ReactToDamage()
    {
        if (damageReactionRoutine != null)
            StopCoroutine(damageReactionRoutine);

        damageReactionRoutine = StartCoroutine(DamageReactionRoutine());
    }

    private IEnumerator DamageReactionRoutine()
    {
        if (agent != null)
        {
            agent.isStopped = true;
            agent.ResetPath();
        }

        yield return new WaitForSeconds(damageReactionTime);

        if (!dead && agent != null)
            agent.isStopped = false;
    }

    private void Die()
    {
        dead = true;
        onDeath?.Invoke();

        if (damageReactionRoutine != null)
            StopCoroutine(damageReactionRoutine);

        if (dialogueController != null)
            dialogueController.ForceEndDialogue(false);

        if (npcController != null)
            npcController.enabled = false;

        if (agent != null)
            agent.enabled = false;

        NPCDetectionZone detectionZone = GetComponentInChildren<NPCDetectionZone>();
        if (detectionZone != null)
            detectionZone.gameObject.SetActive(false);

        NPCDetectionVisualizer visualizer = GetComponentInChildren<NPCDetectionVisualizer>();
        if (visualizer != null)
            visualizer.SetState(NPCDetectionVisualizer.SignState.None);

        gameObject.tag = deadInteractableTag;

        Collider[] colliders = GetComponentsInChildren<Collider>();
        foreach (Collider col in colliders)
        {
            col.isTrigger = false;
            col.gameObject.tag = deadBodyTag;
        }

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb == null)
            rb = gameObject.AddComponent<Rigidbody>();

        rb.isKinematic = true;
        rb.useGravity = false;

        if (addDragObjectOnDeath && GetComponent<DragObject>() == null)
            gameObject.AddComponent<DragObject>();
    }
}