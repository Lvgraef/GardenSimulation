namespace calculation
{
    public class ReportData
    {
        public readonly CalculationResult CalculationResult;
        public readonly float nonPermeableArea = 0;
        public readonly float semiPermeableArea = 0;
        public readonly float bareArea = 0;
        public readonly float flowerArea = 0;
        public readonly float grassArea = 0;
        public readonly float shrubArea = 0;
        public readonly float treeArea = 0;

        public ReportData(CalculationResult calculationResult, float nonPermeableArea, float semiPermeableArea, float bareArea, float flowerArea, float grassArea, float shrubArea, float treeArea)
        {
            CalculationResult = calculationResult;
            this.nonPermeableArea = nonPermeableArea;
            this.semiPermeableArea = semiPermeableArea;
            this.bareArea = bareArea;
            this.flowerArea = flowerArea;
            this.grassArea = grassArea;
            this.shrubArea = shrubArea;
            this.treeArea = treeArea;
        }
    }
}