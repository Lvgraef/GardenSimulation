using System;
using System.IO;
using System.Linq;
using GardenDataManagement;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Utils
{
    public class NamePromptPopup : MonoBehaviour
{
    [SerializeField] private TMP_InputField nameInput;
    [SerializeField] private TextMeshProUGUI feedbackLabel;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;

    private const int MinLength = 3;
    private const int MaxLength = 20;

    private Action<string> onConfirm;
    private Action onCancel;

    private void Awake()
    {
        nameInput.characterLimit = MaxLength;
        confirmButton.onClick.AddListener(HandleConfirm);
        cancelButton.onClick.AddListener(HandleCancel);
        nameInput.onValueChanged.AddListener(_ => feedbackLabel.text = "");
    }

    public void Show(Action<string> onConfirm, Action onCancel = null)
    {
        
        nameInput.text = "";
        feedbackLabel.text = "";
        this.onConfirm = onConfirm;
        this.onCancel = onCancel;
        gameObject.SetActive(true);
        Debug.Log(gameObject.activeSelf);
        nameInput.Select();
        nameInput.ActivateInputField();
    }

    private void HandleConfirm()
    {
        string name = nameInput.text.Trim();
        string feedback = ValidateGardenName(name);

        if (feedback != "OK")
        {
            feedbackLabel.text = feedback;
            return;
        }

        gameObject.SetActive(false);
        onConfirm?.Invoke(name);
    }

    private void HandleCancel()
    {
        gameObject.SetActive(false);
        onCancel?.Invoke();
    }

    private string ValidateGardenName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return "Naam mag niet leeg zijn.";

        if (name.Length < MinLength)
            return $"Naam moet minstens {MinLength} tekens lang zijn.";

        if (name.Length > MaxLength)
            return $"Naam mag maximaal {MaxLength} tekens lang zijn.";

        char[] invalidChars = Path.GetInvalidFileNameChars();
        if (name.Any(c => invalidChars.Contains(c)))
            return "Naam bevat ongeldige tekens.";

        if (Datainterface.GardenExists(name))
            return "Een tuin met deze naam bestaat al.";

        return "OK";
    }
}
}
