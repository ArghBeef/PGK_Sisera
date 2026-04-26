using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Ability_RamCharge", menuName = "Classes/Ram/Active Ability 1 - Charge")]
public class RamChargeAbility : AbilityDefinition
{
    [Header("Charge")]
    public float chargeDistance = 6f;
    public float chargeSpeed = 16f;
    public float radius = 1.2f;

    [Header("Hit")]
    public float damage = 20f;
    public float stunDuration = 2f;
    public float pushForce = 8f;
    public LayerMask hitLayers;

    [Header("Collision")]
    public LayerMask obstacleLayers = ~0;
    public float wallCheckHeight = 0.4f;
    public float wallCheckExtraDistance = 0.2f;

    public override void Activate(GameObject user, PlayerClassController classController)
    {
        RamChargeRunner runner = user.GetComponent<RamChargeRunner>();

        if (runner == null)
            runner = user.AddComponent<RamChargeRunner>();

        runner.StartCharge(this, classController);
    }
}

public class RamChargeRunner : MonoBehaviour
{
    private bool charging;
    private Rigidbody rb;

    private RamChargeAbility currentAbility;
    private PlayerClassController currentClassController;
    private Vector3 currentDirection;

    private readonly HashSet<GameObject> hitObjects = new();

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void StartCharge(RamChargeAbility ability, PlayerClassController classController)
    {
        if (charging)
            return;

        currentAbility = ability;
        currentClassController = classController;

        StartCoroutine(ChargeRoutine());
    }

    private IEnumerator ChargeRoutine()
    {
        charging = true;
        hitObjects.Clear();

        if (rb == null)
            rb = GetComponent<Rigidbody>();

        PC_Movement movement = GetComponent<PC_Movement>();

        currentDirection = movement != null
            ? movement.FacingDirection
            : transform.forward;

        currentDirection.y = 0f;

        if (currentDirection.sqrMagnitude < 0.01f)
            currentDirection = transform.forward;

        currentDirection.Normalize();

        float travelled = 0f;

        while (travelled < currentAbility.chargeDistance)
        {
            float step = currentAbility.chargeSpeed * Time.fixedDeltaTime;

            if (Physics.Raycast(
                rb.position + Vector3.up * currentAbility.wallCheckHeight,
                currentDirection,
                out RaycastHit wallHit,
                step + currentAbility.wallCheckExtraDistance,
                currentAbility.obstacleLayers
            ))
            {
                break;
            }

            Vector3 targetPosition = rb.position + currentDirection * step;
            rb.MovePosition(targetPosition);

            CheckOverlapHits();

            travelled += step;

            yield return new WaitForFixedUpdate();
        }

        rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);

        charging = false;
        currentAbility = null;
        currentClassController = null;
    }

    private void CheckOverlapHits()
    {
        Collider[] hits = Physics.OverlapSphere(
            transform.position,
            currentAbility.radius,
            currentAbility.hitLayers
        );

        foreach (Collider hit in hits)
        {
            if (hit == null)
                continue;

            HandleHit(hit);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!charging)
            return;

        if (collision == null || collision.collider == null)
            return;

        HandleHit(collision.collider);
    }

    private void OnCollisionStay(Collision collision)
    {
        if (!charging)
            return;

        if (collision == null || collision.collider == null)
            return;

        HandleHit(collision.collider);
    }

    private void HandleHit(Collider hit)
    {
        if (hit.transform.root == transform.root)
            return;

        GameObject hitRoot = hit.attachedRigidbody != null
            ? hit.attachedRigidbody.gameObject
            : hit.transform.root.gameObject;

        bool firstHit = !hitObjects.Contains(hitRoot);

        if (firstHit)
            hitObjects.Add(hitRoot);

        Rigidbody targetRb = hit.GetComponentInParent<Rigidbody>();

        if (targetRb != null && targetRb != rb)
        {
            Vector3 pushDirection = currentDirection;
            pushDirection.y = 0.25f;
            pushDirection.Normalize();

            targetRb.AddForce(
                pushDirection * currentAbility.pushForce,
                ForceMode.Impulse
            );
        }

        if (!firstHit)
            return;

        IDamageable damageable = hit.GetComponentInParent<IDamageable>();

        if (damageable != null)
        {
            float finalDamage = currentAbility.damage;

            if (currentClassController != null)
                finalDamage *= currentClassController.GetDamageMultiplierAgainst(hit.gameObject);

            damageable.TakeDamage(finalDamage);
        }

        NPCStatus status = hit.GetComponentInParent<NPCStatus>();

        if (status != null)
            status.Stun(currentAbility.stunDuration);

        BreakableObject breakable = hit.GetComponentInParent<BreakableObject>();

        if (breakable != null)
            breakable.Break();
    }
}