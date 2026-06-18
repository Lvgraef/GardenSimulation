using System;
using System.Globalization;
using calculation;
using gardensettings;
using GridSystem;
using TMPro;
using UnityEngine;
using XCharts.Runtime;

namespace report
{
    public class ReportManager : MonoBehaviour
    {
        [SerializeField] private GridManager gridManager;
        [SerializeField] private GardenSettings gardenSettings;
        
        [SerializeField] private TMP_Text nonPermeableAreaText;
        [SerializeField] private TMP_Text semiPermeableAreaText;
        [SerializeField] private TMP_Text bareAreaText;
        [SerializeField] private TMP_Text flowerAreaText;
        [SerializeField] private TMP_Text grassAreaText;
        [SerializeField] private TMP_Text shrubAreaText;
        [SerializeField] private TMP_Text treeAreaText;

        [SerializeField] private BarChart sustainabilityReportChart;
        [SerializeField] private BarChart miniReportChart;

        [SerializeField] private GameObject miniReport;
        [SerializeField] private GameObject report;
        
        private Calculator _calculator;

        private void Awake()
        {
            _calculator = new Calculator(BasicCalculationModel.Instance, gridManager, gardenSettings);
        }

        private void Start()
        {
            gridManager.GridChangeEvent += OnGridChange;
        }

        private void OnGridChange()
        {
            ReportData data = _calculator.Calculate(false);

            nonPermeableAreaText.text = "Volledige volharding: " + data.nonPermeableArea.ToString(CultureInfo.CurrentCulture);
            semiPermeableAreaText.text = "Doorlatend verhard of grind: " + data.semiPermeableArea.ToString(CultureInfo.CurrentCulture);
            bareAreaText.text = "Niet verhard en zonder beplanting: " + data.bareArea.ToString(CultureInfo.CurrentCulture);
            flowerAreaText.text = "Bloemen of moestuin of bodembedekker: " + data.flowerArea.ToString(CultureInfo.CurrentCulture);
            grassAreaText.text = "Gras (Gemaaid): " + data.grassArea.ToString(CultureInfo.CurrentCulture);
            shrubAreaText.text = "Struiken, heg, haag of kleine bomen: " + data.shrubArea.ToString(CultureInfo.CurrentCulture);
            treeAreaText.text = "Grote boom: " + data.treeArea.ToString(CultureInfo.CurrentCulture);
            
            sustainabilityReportChart.UpdateData(0, 0, data.CalculationResult.WaterScore);
            sustainabilityReportChart.UpdateData(0, 1, data.CalculationResult.SoilScore);
            sustainabilityReportChart.UpdateData(0, 2, data.CalculationResult.AnimalScore);
            sustainabilityReportChart.UpdateData(0, 3, data.CalculationResult.PlantScore);
            
            miniReportChart.UpdateData(0, 0, data.CalculationResult.WaterScore);
            miniReportChart.UpdateData(0, 1, data.CalculationResult.SoilScore);
            miniReportChart.UpdateData(0, 2, data.CalculationResult.AnimalScore);
            miniReportChart.UpdateData(0, 3, data.CalculationResult.PlantScore);
        }

        public void ToggleMiniReport()
        {
            miniReport.SetActive(!miniReport.activeSelf);
        }

        public void SetReportState(bool state)
        {
            report.SetActive(state);
        }
    }
}