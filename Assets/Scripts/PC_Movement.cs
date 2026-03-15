using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public class PC_Movement : MonoBehaviour
{

    private NavMeshAgent agent;

    public float moveSpeed = 10f;

    [SerializeField] float sampleDistance = 0.5f;
    [SerializeField] LayerMask groundLayer;
    [SerializeField] InputActionReference moveAction;

    public static event System.Action<Vector3> OnGroundTouch;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = moveSpeed;
    }

    void Update()
    {
        if(moveAction != null && moveAction.action.triggered)
        {
            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());

            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, groundLayer))
            {
                if(NavMesh.SamplePosition(hit.point, out NavMeshHit navHit, sampleDistance, NavMesh.AllAreas))
                {
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
}
