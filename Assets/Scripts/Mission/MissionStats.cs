using System.Collections.Generic;
using UnityEngine;

public class MissionStatsTracker : MonoBehaviour
{
    public static MissionStatsTracker Instance { get; private set; }

    [SerializeField] private PlayerPoints playerPoints;
    [SerializeField] private Health playerHealth;

    private float missionStartTime;
    private int enemiesKilled;
    private float damageTaken;
    private readonly Dictionary<string, int> abilityUses = new();

    public int EnemiesKilled => enemiesKilled;
    public float MissionTimePlayed => Time.time - missionStartTime;
    public int Points => playerPoints != null ? playerPoints.CurrentPoints : 0;
    public float DamageTaken => damageTaken;

    private void Awake()
    {
        Instance = this;

        if (playerPoints == null)
            playerPoints = FindFirstObjectByType<PlayerPoints>();

        if (playerHealth == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                playerHealth = player.GetComponentInChildren<Health>();
        }
    }

    private void Start()
    {
        missionStartTime = Time.time;

        if (playerHealth != null)
            playerHealth.OnHealthChanged += TrackDamageTaken;
    }

    private void OnDestroy()
    {
        if (playerHealth != null)
            playerHealth.OnHealthChanged -= TrackDamageTaken;
    }

    private void TrackDamageTaken(float currentHealth, float maxHealth, float change)
    {
        if (change < 0f)
            damageTaken += Mathf.Abs(change);
    }

    public void AddEnemyKill()
    {
        enemiesKilled++;
    }

    public void AddAbilityUse(string abilityName)
    {
        if (string.IsNullOrWhiteSpace(abilityName))
            abilityName = "Unknown Ability";

        if (!abilityUses.ContainsKey(abilityName))
            abilityUses.Add(abilityName, 0);

        abilityUses[abilityName]++;
    }

    public string GetMostUsedAbility()
    {
        if (abilityUses.Count == 0)
            return "None";

        string bestAbility = "None";
        int bestCount = 0;

        foreach (var pair in abilityUses)
        {
            if (pair.Value > bestCount)
            {
                bestAbility = pair.Key;
                bestCount = pair.Value;
            }
        }

        return bestAbility + " (" + bestCount + " uses)";
    }
}