namespace calculation
{
    public static class Calculator
    {
        private static float GetTotalArea(CalculationData data)
        {
            return data.NonPermeable + data.SemiPermeable + data.Bare + data.Bare + data.Flowers + data.Grass +
                   data.Shrubs + data.Tree;
        }

        private static float GetTotalVegetationArea(CalculationData data)
        {
            return data.Flowers + data.Grass + data.Shrubs + data.Tree;
        }

        private static float GetVegetationPercentage(CalculationData data)
        {
            return GetTotalVegetationArea(data) / GetTotalArea(data);
        }

        private static float CalculateWaterScore(CalculationData data)
        {
            float nonPermeableScore = data.NonPermeable * CalculationConstants.NonPermeableWaterCoefficient;
            float semiPermeableScore = data.SemiPermeable * CalculationConstants.SemiPermeableWaterCoefficient;
            float bareScore = data.Bare * CalculationConstants.BareWaterCoefficient;
            float flowerScore = data.Flowers * CalculationConstants.FlowerWaterCoefficient;
            float grassScore = data.Grass * CalculationConstants.GrassWaterCoefficient;
            float shrubScore = data.Shrubs * CalculationConstants.ShrubWaterCoefficient;
            float treeScore = data.Tree * CalculationConstants.TreeWaterCoefficient;

            return nonPermeableScore + semiPermeableScore + bareScore + flowerScore + grassScore + shrubScore +
                   treeScore;
        }

        private static float CalculateSoilScore(CalculationData data)
        {
            return CalculationConstants.GetFertilizationCoefficient(data.Fertilizer, data.CompostCleanup) *
                   GetVegetationPercentage(data);
        }

        private static float CalculateAnimalScore(CalculationData data)
        {
            float vegetationPercentage = GetVegetationPercentage(data);

            float flyingInsectScore = (data.FlyingInsects ? 1 : 0) * CalculationConstants.FlyingInsectCoefficient *
                                      vegetationPercentage;
            float birdScore = (data.Birds ? 1 : 0) * CalculationConstants.BirdCoefficient *
                              vegetationPercentage;
            float spiderScore = (data.Spiders ? 1 : 0) * CalculationConstants.SpiderCoefficient *
                                vegetationPercentage;
            float otherAnimalScore = (data.OtherAnimals ? 1 : 0) * CalculationConstants.OtherAnimalCoefficient *
                                     vegetationPercentage;

            return flyingInsectScore + birdScore + spiderScore + otherAnimalScore;
        }

        private static float CalculatePlantScore(CalculationData data)
        {
            float plantSpeciesDiversityCoefficient =
                CalculationConstants.GetPlantSpeciesDiversityCoefficient(data.PlantDiversity);

            float flowerScore = CalculationConstants.FlowerDiversityCoefficient * (data.Flowers / GetTotalArea(data)) *
                                plantSpeciesDiversityCoefficient;
            float grassScore = CalculationConstants.GrassDiversityCoefficient * (data.Grass / GetTotalArea(data)) *
                               plantSpeciesDiversityCoefficient;
            float shrubScore = CalculationConstants.ShrubDiversityCoefficient * (data.Shrubs / GetTotalArea(data)) *
                               plantSpeciesDiversityCoefficient;
            float treeScore = CalculationConstants.TreeDiversityCoefficient * (data.Shrubs / GetTotalArea(data)) *
                              plantSpeciesDiversityCoefficient;

            return flowerScore + grassScore + shrubScore + treeScore;
        }

        public static CalculationResult Calculate(CalculationData data)
        {
            return new CalculationResult(CalculateWaterScore(data), CalculateSoilScore(data),
                CalculateAnimalScore(data), CalculatePlantScore(data));
        }
    }
}