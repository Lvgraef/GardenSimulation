using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class MaterialMenu : MonoBehaviour
    {
        [SerializeField]
        private Material outlineMaterial;
        public GridSystem.Material[] Materials;
        public Transform ButtonContainer;
        public GridSystem.Material SelectedMaterial { get; private set; } 
        public Button ButtonPrefab;
        public Button EraserButton;
        public bool Eraser;
        public GameObject BackGround;
        private List<Button> _buttons = new ();
        
        private void Start()
        {
            BackGround.SetActive(true);
            EraserButton.onClick.AddListener(() => EraserClicked());
            foreach (var material in Materials)
            {
                Button button = Instantiate(ButtonPrefab, ButtonContainer);
                button.image.sprite = material.sprite;
                Outline outline = button.gameObject.AddComponent<Outline>();
                outline.effectColor = new Color(0.9f, 0.9f, 0.9f, 1f);
                button.onClick.AddListener(() => ButtonClicked(material, button));
                _buttons.Add(button);
            }
        }
        private void ButtonClicked(GridSystem.Material material, Button button)
        {
            foreach (var b in _buttons)
            {
                b.image.material = null;
            }
            EraserButton.image.material = null;
            button.image.material = outlineMaterial;
            if (Eraser)
            {
                Eraser = false;
            }
            SelectedMaterial = material;
        }
        private void EraserClicked()
        {
            foreach (var b in _buttons)
            {
                b.image.material = null;
            }
            EraserButton.image.material = outlineMaterial;
            Eraser = true;
            SelectedMaterial = null;
        }
    }
}