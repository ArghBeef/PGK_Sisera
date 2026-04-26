using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "Ability_RamShield", menuName = "Classes/Ram/Active Ability 2 - Ram Shield")]
public class RamShield : AbilityDefinition
{
    public float grabRange = 2f;
    public float duration = 5f;
    public float endStunDuration = 5f;
    public Transform fallbackHoldPoint;

    public override void Activate(GameObject user, PlayerClassController classController)
    {
        RamHumanShieldRunner runner = user.GetComponent<RamHumanShieldRunner>();

        if (runner == null)
            runner = user.AddComponent<RamHumanShieldRunner>();

        runner.StartShield(this);
    }
}

public class RamHumanShieldRunner : MonoBehaviour
{
    private bool active;
    private GameObject grabbedEnemy;

    public void StartShield(RamShield ability)
    {
        if (!active)
            StartCoroutine(ShieldRoutine(ability));
    }

    private IEnumerator ShieldRoutine(RamShield ability)
    {
        active = true;

        Collider[] hits = Physics.OverlapSphere(transform.position + transform.forward, ability.grabRange);

        foreach (Collider hit in hits)
        {
            if (hit.GetComponentInParent<NPCController>() != null)
            {
                grabbedEnemy = hit.GetComponentInParent<NPCController>().gameObject;
                break;
            }
        }

        if (grabbedEnemy == null)
        {
            active = false;
            yield break;
        }

        NPCStatus status = grabbedEnemy.GetComponent<NPCStatus>();
        if (status != null)
            status.Stun(ability.duration);

        float timer = ability.duration;

        while (timer > 0f && grabbedEnemy != null)
        {
            Health health = grabbedEnemy.GetComponent<Health>();

            if (health != null && health.IsDead)
                break;

            grabbedEnemy.transform.position = transform.position + transform.forward * 1.2f;
            grabbedEnemy.transform.rotation = transform.rotation;

            timer -= Time.deltaTime;
            yield return null;
        }

        if (grabbedEnemy != null)
        {
            Health health = grabbedEnemy.GetComponent<Health>();

            if (health == null || !health.IsDead)
            {
                NPCStatus enemyStatus = grabbedEnemy.GetComponent<NPCStatus>();
                if (enemyStatus != null)
                    enemyStatus.Stun(ability.endStunDuration);
            }
        }

        grabbedEnemy = null;
        active = false;
    }
}