using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Utils
{
    public class InfoPopup : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI messageLabel;
        [SerializeField] private Button confirmButton;

        private Action _onConfirm;

        private void Awake()
        {
            confirmButton.onClick.AddListener(HandleConfirm);
        }

        public void Show(string message, Action onConfirm, Action onCancel = null)
        {
            messageLabel.text = message;
            this._onConfirm = onConfirm;
            gameObject.SetActive(true);
        }

        private void HandleConfirm()
        {
            gameObject.SetActive(false);
            _onConfirm?.Invoke();
        }
        
    }
}