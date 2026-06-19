using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GardenDataManagement
{
    public class GardenListItem : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI label;
        private Button button;

        private void Awake()
        {
            button = GetComponent<Button>();
        }

        public void Setup(string gardenName, Action<string> onClick)
        {
            label.text = gardenName;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => onClick(gardenName));
        }
    }
}