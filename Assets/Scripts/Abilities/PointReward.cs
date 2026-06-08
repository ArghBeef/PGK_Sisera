using UnityEngine;

public class PointReward : MonoBehaviour
{
    [SerializeField] private int points = 10;

    public void GiveTo(GameObject target)
    {
        if (target == null)
            return;

        PlayerPoints playerPoints = target.GetComponent<PlayerPoints>();

        if (playerPoints == null)
            playerPoints = target.GetComponentInParent<PlayerPoints>();

        if (playerPoints == null)
            return;

        playerPoints.AddPoints(points);
    }

    public void GiveToPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player == null)
            return;

        GiveTo(player);
    }
}