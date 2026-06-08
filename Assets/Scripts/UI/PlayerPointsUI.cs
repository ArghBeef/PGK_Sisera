using TMPro;
using UnityEngine;

public class PlayerPointsUI : MonoBehaviour
{
    [SerializeField] private PlayerPoints playerPoints;
    [SerializeField] private TMP_Text pointsText;
    [SerializeField] private string prefix = "Points: ";

    private void Awake()
    {
        if (playerPoints == null)
            playerPoints = FindFirstObjectByType<PlayerPoints>();
    }

    private void OnEnable()
    {
        if (playerPoints != null)
            playerPoints.OnPointsChanged += UpdateText;
    }

    private void OnDisable()
    {
        if (playerPoints != null)
            playerPoints.OnPointsChanged -= UpdateText;
    }

    private void Start()
    {
        if (playerPoints != null)
            UpdateText(playerPoints.CurrentPoints);
    }

    private void UpdateText(int points)
    {
        if (pointsText != null)
            pointsText.text = prefix + points;
    }
}