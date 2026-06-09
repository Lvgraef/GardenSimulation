using UnityEngine;
using System.Collections.Generic;
using GardenSimulation.Services.Api;
using UnityEngine.UI;
namespace GardenSimulation
{
    public class MaterialMenu : MonoBehaviour 
    {
        public MaterialData[] Materials;

        public Transform ButtonContainer;
        public MaterialData SelectedMaterial; 
        public APIClient api;
        public Button ButtonPrefab;
        public Button EraserButton;
        public Button CallAPIButton;
        public bool Eraser = false;
        public GameObject BackGround;
        private void Start()
        {
            api = new APIClient();
            CallAPIButton.onClick.AddListener(() => StartCoroutine(api.GetAddressSpatialData("Elzenzoom 63")));
            BackGround.SetActive(true);
            EraserButton.onClick.AddListener(() => EraserClicked());
            foreach (var material in Materials)
            {
                Button button = Instantiate(ButtonPrefab, ButtonContainer);
                button.image.sprite = material.sprite;
                Outline outline = button.gameObject.AddComponent<Outline>();
                outline.effectColor = new Color(0.9f, 0.9f, 0.9f, 1f);
                button.onClick.AddListener(() => ButtonClicked(material));
            }
        }
        private void ButtonClicked(MaterialData material)
        {
            if (Eraser == true)
            {
                Eraser = false;
            }
            SelectedMaterial = material;
        }
        private void EraserClicked()
        {
            Eraser = true;
            SelectedMaterial = null;
        }
    }
}