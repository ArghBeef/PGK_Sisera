using UnityEngine;

public enum MainMenuSelectableType
{
    Skull,
    Folder
}

public class MenuSelectable : MonoBehaviour
{
    [Header("Type")]
    public MainMenuSelectableType selectableType;

    [Header("Data")]
    public string displayName;
    [TextArea] public string description;
    public Sprite previewImage;

    [Header("Visual")]
    public GameObject outlineObject;
    public Transform focusPoint;
    public GameObject objectToActivateOnClick;

    private void Awake()
    {
        SetHighlight(false);

        if (objectToActivateOnClick != null)
            objectToActivateOnClick.SetActive(false);
    }

    public void SetHighlight(bool active)
    {
        if (outlineObject != null)
            outlineObject.SetActive(active);
    }

    public void Select()
    {
        if (objectToActivateOnClick != null)
            objectToActivateOnClick.SetActive(true);
    }

    public void Deselect()
    {
        if (objectToActivateOnClick != null)
            objectToActivateOnClick.SetActive(false);
    }
}