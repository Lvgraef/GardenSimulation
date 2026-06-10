namespace calculation
{
    public class CalculationResult
    {
        public readonly float WaterScore; // Water in de bodem
        public readonly float SoilScore; // Gezonde bodem
        public readonly float AnimalScore; // Gezonde leefomgeving voor dieren
        public readonly float PlantScore; // Plantdiversiteit

        public CalculationResult(float waterScore, float soilScore, float animalScore, float plantScore)
        {
            WaterScore = waterScore;
            SoilScore = soilScore;
            AnimalScore = animalScore;
            PlantScore = plantScore;
        }
    }
}