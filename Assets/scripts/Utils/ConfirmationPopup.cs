using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Utils
{
    public class ConfirmationPopup : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI messageLabel;
        [SerializeField] private Button confirmButton;
        [SerializeField] private Button cancelButton;

        private Action onConfirm;
        private Action onCancel;

        private void Awake()
        {
            confirmButton.onClick.AddListener(HandleConfirm);
            cancelButton.onClick.AddListener(HandleCancel);
        }

        public void Show(string message, Action onConfirm, Action onCancel = null)
        {
            messageLabel.text = message;
            this.onConfirm = onConfirm;
            this.onCancel = onCancel;
            gameObject.SetActive(true);
        }

        private void HandleConfirm()
        {
            gameObject.SetActive(false);
            onConfirm?.Invoke();
        }

        private void HandleCancel()
        {
            gameObject.SetActive(false);
            onCancel?.Invoke();
        }
    }
}