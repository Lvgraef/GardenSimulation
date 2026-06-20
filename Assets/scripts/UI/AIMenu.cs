using ai;
using GridSystem;
using TMPro;
using UnityEngine;

namespace UI
{
    public class AIMenu : MonoBehaviour
    {
        public GridManager gridManager;
        public (int, int)? SelectedSubGrid { get; private set; }
        public int BushArea { get; private set; } = int.MinValue;
        public int WaterArea { get; private set; } = int.MinValue;
        public int TileArea { get; private set; } = int.MinValue;
        public int TreeArea { get; private set; } = int.MinValue;
        public int GrassArea { get; private set; } = int.MinValue;
        public int FlowerArea { get; private set; } = int.MinValue;

        [SerializeField] private GridInferenceAgent inferenceAgent;

        [SerializeField] private TMP_InputField xInput;
        [SerializeField] private TMP_InputField yInput;
        [SerializeField] private TMP_InputField stepsInput;
        [SerializeField] private TMP_InputField bushesInput;
        [SerializeField] private TMP_InputField waterInput;
        [SerializeField] private TMP_InputField treesInput;
        [SerializeField] private TMP_InputField grassInput;
        [SerializeField] private TMP_InputField flowerInput;
        [SerializeField] private TMP_InputField tilesInput;


        public int GetSteps()
        {
            if (int.TryParse(stepsInput.text, out var steps))
            {
                return steps;
            }

            return gridManager.height *  gridManager.width;
        }

        public void SetLocal(bool local)
        {
            if (local)
            {
                SelectedSubGrid = null;
                xInput.interactable = true;
                yInput.interactable = true;
                return;
            }

            xInput.interactable = false;
            yInput.interactable = false;
            SelectedSubGrid = null;
        }

        public void OnUpdate()
        {
            BushArea = int.MinValue;
            WaterArea = int.MinValue;
            TreeArea = int.MinValue;
            GrassArea = int.MinValue;
            FlowerArea = int.MinValue;
            TileArea = int.MinValue;

            bool changed = false;
            
            int xParsed = 0;
            int yParsed = 0;

            if (int.TryParse(xInput.text, out var x))
            {
                xParsed = x;
                changed = true;
            }

            if (int.TryParse(yInput.text, out var y))
            {
                yParsed = y;
                changed = true;
            }

            SelectedSubGrid = changed ? (xParsed, yParsed) : null;

            if (int.TryParse(bushesInput.text, out var bushes))
            {
                BushArea = bushes;
            }

            if (int.TryParse(waterInput.text, out var water))
            {
                WaterArea = water;
            }

            if (int.TryParse(treesInput.text, out var trees))
            {
                TreeArea = trees;
            }

            if (int.TryParse(grassInput.text, out var grass))
            {
                GrassArea = grass;
            }

            if (int.TryParse(flowerInput.text, out var flower))
            {
                FlowerArea = flower;
            }
            
            if (int.TryParse(tilesInput.text, out var tile))
            {
                TileArea = tile;
            }
        }
    }
}