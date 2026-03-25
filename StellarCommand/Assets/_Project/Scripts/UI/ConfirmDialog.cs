namespace StellarCommand.Core
{
    using System;
    using UnityEngine;
    using UnityEngine.UI;
    using TMPro;

    public class ConfirmDialog : MonoBehaviour
    {
        [SerializeField] private GameObject _panel;
        [SerializeField] private TextMeshProUGUI _messageText;
        [SerializeField] private Button _confirmButton;
        [SerializeField] private Button _cancelButton;

        private Action _onConfirm;
        private Action _onCancel;

        private void Awake()
        {
            _panel.SetActive(false);
            _confirmButton.onClick.AddListener(OnConfirmClicked);
            _cancelButton.onClick.AddListener(OnCancelClicked);
        }

        public void Show(string message, Action onConfirm, Action onCancel = null)
        {
            _messageText.text = message;
            _onConfirm = onConfirm;
            _onCancel = onCancel;
            _panel.SetActive(true);
        }

        private void OnConfirmClicked()
        {
            _panel.SetActive(false);
            _onConfirm?.Invoke();
        }

        private void OnCancelClicked()
        {
            _panel.SetActive(false);
            _onCancel?.Invoke();
        }
    }
}
