using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GardenDataManagement
{
    public class GardenListItem : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI label;
        private Button _button;

        private void Awake()
        {
            _button = GetComponent<Button>();
        }

        public void Setup(string gardenName, Action<string> onClick)
        {
            label.text = gardenName;
            _button.onClick.RemoveAllListeners();
            _button.onClick.AddListener(() => onClick(gardenName));
        }
    }
}