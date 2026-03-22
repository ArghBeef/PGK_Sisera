using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public class PC_Movement : MonoBehaviour
{
    private NavMeshAgent agent;

    public float moveSpeed = 10f;

    [SerializeField] private float sampleDistance = 0.5f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private InputActionReference moveAction;

    public static event System.Action<Vector3> OnGroundTouch;

    public bool IsMovementLocked { get; private set; }

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = moveSpeed;
    }

    private void Update()
    {
        if (IsMovementLocked)
            return;

        if (moveAction != null && moveAction.action.triggered)
        {
            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());

            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, groundLayer))
            {
                if (NavMesh.SamplePosition(hit.point, out NavMeshHit navHit, sampleDistance, NavMesh.AllAreas))
                {
                    agent.isStopped = false;
                    agent.SetDestination(navHit.position);
                    OnGroundTouch?.Invoke(navHit.position);
                }
                else
                {
                    Debug.LogWarning("No valid NavMesh position found near the clicked point.");
                }
            }
        }
    }

    public void SetMovementLocked(bool locked)
    {
        IsMovementLocked = locked;

        if (agent == null)
            agent = GetComponent<NavMeshAgent>();

        if (locked)
        {
            agent.isStopped = true;
            agent.ResetPath();
        }
        else
        {
            agent.isStopped = false;
        }
    }
}