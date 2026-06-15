using TMPro;
using UnityEngine;

public class PlayerPointTextUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerPoints playerPoints;
    [SerializeField] private TMP_Text pointsPopupText;

    [Header("Settings")]
    [SerializeField] private float showTime = 1.5f;
    [SerializeField] private string prefix = "+";

    private float hideTimer;

    private void Awake()
    {
        if (playerPoints == null)
            playerPoints = GetComponent<PlayerPoints>();

        if (pointsPopupText != null)
            pointsPopupText.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        if (playerPoints != null)
            playerPoints.onPointsAdded.AddListener(ShowPoints);
    }

    private void OnDisable()
    {
        if (playerPoints != null)
            playerPoints.onPointsAdded.RemoveListener(ShowPoints);
    }

    private void Update()
    {
        if (pointsPopupText == null)
            return;

        if (!pointsPopupText.gameObject.activeSelf)
            return;

        hideTimer -= Time.deltaTime;

        if (hideTimer <= 0f)
            pointsPopupText.gameObject.SetActive(false);
    }

    private void ShowPoints(int amount)
    {
        if (pointsPopupText == null)
            return;

        pointsPopupText.text = prefix + amount;
        pointsPopupText.gameObject.SetActive(true);

        hideTimer = showTime;
    }
}