using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HotbarSlotUI : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private GameObject selectedOutline;

    private int slotIndex;

    public void Initialize(int index)
    {
        slotIndex = index;


        SetSelected(false);
        SetItem(null);
    }

    public void SetItem(ItemDefinition item)
    {
        if (iconImage == null)
            return;

        if (item == null || item.icon == null)
        {
            iconImage.sprite = null;
            iconImage.enabled = false;
            return;
        }

        iconImage.sprite = item.icon;
        iconImage.enabled = true;
    }

    public void SetSelected(bool isSelected)
    {
        if (selectedOutline != null)
            selectedOutline.SetActive(isSelected);
    }
}