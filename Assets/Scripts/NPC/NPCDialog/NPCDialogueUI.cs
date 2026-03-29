using TMPro;
using UnityEngine;

public class NPCDialogueUI : MonoBehaviour
{
    [SerializeField] private Canvas canvas;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private bool billboardToCamera = true;

    private Camera cachedCamera;

    private void Awake()
    {
        Hide();
    }

    private void LateUpdate()
    {
        if (!billboardToCamera)
            return;

        if (cachedCamera == null)
            cachedCamera = Camera.main;

        if (cachedCamera == null)
            return;

        Vector3 direction = transform.position - cachedCamera.transform.position;
        transform.rotation = Quaternion.LookRotation(direction);
    }

    public void ShowText(string text)
    {
        if (dialogueText == null)
            return;

        if (canvas != null)
            canvas.enabled = true;

        dialogueText.text = text;
        dialogueText.gameObject.SetActive(true);
    }

    public void Hide()
    {
        if (dialogueText != null)
        {
            dialogueText.text = string.Empty;
            dialogueText.gameObject.SetActive(false);
        }
    }
}