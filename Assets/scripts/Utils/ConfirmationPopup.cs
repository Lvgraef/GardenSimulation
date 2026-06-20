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

        private Action _onConfirm;
        private Action _onCancel;

        private void Awake()
        {
            confirmButton.onClick.AddListener(HandleConfirm);
            cancelButton.onClick.AddListener(HandleCancel);
        }

        public void Show(string message, Action onConfirm, Action onCancel = null)
        {
            messageLabel.text = message;
            this._onConfirm = onConfirm;
            this._onCancel = onCancel;
            gameObject.SetActive(true);
        }

        private void HandleConfirm()
        {
            gameObject.SetActive(false);
            _onConfirm?.Invoke();
        }

        private void HandleCancel()
        {
            gameObject.SetActive(false);
            _onCancel?.Invoke();
        }
    }
}