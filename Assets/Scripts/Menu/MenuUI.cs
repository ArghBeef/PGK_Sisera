using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [Header("Entrance")]
    [SerializeField] private GameObject entranceText;

    [Header("Info Panel")]
    [SerializeField] private GameObject infoPanel;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private Image previewImage;

    [Header("Start")]
    [SerializeField] private Button startButton;

    private void Awake()
    {
        ShowEntrance(true);
        HideInfo();
        SetStartVisible(false);
    }

    public void ShowEntrance(bool active)
    {
        if (entranceText != null)
            entranceText.SetActive(active);
    }

    public void ShowInfo(MenuSelectable selectable)
    {
        if (selectable == null)
        {
            HideInfo();
            return;
        }

        if (infoPanel != null)
            infoPanel.SetActive(true);

        if (nameText != null)
            nameText.text = selectable.displayName;

        if (descriptionText != null)
            descriptionText.text = selectable.description;

        if (previewImage != null)
        {
            previewImage.sprite = selectable.previewImage;
            previewImage.enabled = selectable.previewImage != null;
        }
    }

    public void HideInfo()
    {
        if (infoPanel != null)
            infoPanel.SetActive(false);
    }

    public void SetStartVisible(bool visible)
    {
        if (startButton != null)
            startButton.gameObject.SetActive(visible);
    }
}