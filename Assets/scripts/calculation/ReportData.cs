namespace calculation
{
    public class ReportData
    {
        public readonly CalculationResult CalculationResult;
        public readonly float NonPermeableArea = 0;
        public readonly float SemiPermeableArea = 0;
        public readonly float BareArea = 0;
        public readonly float FlowerArea = 0;
        public readonly float GrassArea = 0;
        public readonly float ShrubArea = 0;
        public readonly float TreeArea = 0;

        public ReportData(CalculationResult calculationResult, float nonPermeableArea, float semiPermeableArea, float bareArea, float flowerArea, float grassArea, float shrubArea, float treeArea)
        {
            CalculationResult = calculationResult;
            NonPermeableArea = nonPermeableArea;
            SemiPermeableArea = semiPermeableArea;
            BareArea = bareArea;
            FlowerArea = flowerArea;
            GrassArea = grassArea;
            ShrubArea = shrubArea;
            TreeArea = treeArea;
        }
    }
}