using TMPro;
using UnityEngine;

public class InteractionUI : MonoBehaviour
{
    [SerializeField] private TMP_Text promptText;

    private void Awake()
    {
        Hide();
    }

    public void Show(string text)
    {
        promptText.gameObject.SetActive(true);
        promptText.text = text;
    }

    public void Hide()
    {
        promptText.gameObject.SetActive(false);
    }
}