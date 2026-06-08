namespace calculation
{
    public class BasicCalculationModel : ICalculationModel
    {
        public static readonly BasicCalculationModel Instance = new(); 
        
        private float GetTotalArea(CalculationData data)
        {
            return data.NonPermeable + data.SemiPermeable + data.Bare + data.Bare + data.Flowers + data.Grass +
                   data.Shrubs + data.Tree;
        }

        private float GetTotalVegetationArea(CalculationData data)
        {
            return data.Flowers + data.Grass + data.Shrubs + data.Tree;
        }

        private float GetVegetationPercentage(CalculationData data)
        {
            return GetTotalVegetationArea(data) / GetTotalArea(data);
        }

        private float CalculateWaterScore(CalculationData data)
        {
            float nonPermeableScore = data.NonPermeable * BasicCalculationConstants.NonPermeableWaterCoefficient;
            float semiPermeableScore = data.SemiPermeable * BasicCalculationConstants.SemiPermeableWaterCoefficient;
            float bareScore = data.Bare * BasicCalculationConstants.BareWaterCoefficient;
            float flowerScore = data.Flowers * BasicCalculationConstants.FlowerWaterCoefficient;
            float grassScore = data.Grass * BasicCalculationConstants.GrassWaterCoefficient;
            float shrubScore = data.Shrubs * BasicCalculationConstants.ShrubWaterCoefficient;
            float treeScore = data.Tree * BasicCalculationConstants.TreeWaterCoefficient;

            return nonPermeableScore + semiPermeableScore + bareScore + flowerScore + grassScore + shrubScore +
                   treeScore;
        }

        private float CalculateSoilScore(CalculationData data)
        {
            return BasicCalculationConstants.GetFertilizationCoefficient(data.Fertilizer, data.CompostCleanup) *
                   GetVegetationPercentage(data);
        }

        private float CalculateAnimalScore(CalculationData data)
        {
            float vegetationPercentage = GetVegetationPercentage(data);

            float flyingInsectScore = (data.FlyingInsects ? 1 : 0) * BasicCalculationConstants.FlyingInsectCoefficient *
                                      vegetationPercentage;
            float birdScore = (data.Birds ? 1 : 0) * BasicCalculationConstants.BirdCoefficient *
                              vegetationPercentage;
            float spiderScore = (data.Spiders ? 1 : 0) * BasicCalculationConstants.SpiderCoefficient *
                                vegetationPercentage;
            float otherAnimalScore = (data.OtherAnimals ? 1 : 0) * BasicCalculationConstants.OtherAnimalCoefficient *
                                     vegetationPercentage;

            return flyingInsectScore + birdScore + spiderScore + otherAnimalScore;
        }

        private float CalculatePlantScore(CalculationData data)
        {
            float plantSpeciesDiversityCoefficient =
                BasicCalculationConstants.GetPlantSpeciesDiversityCoefficient(data.PlantDiversity);

            float flowerScore = BasicCalculationConstants.FlowerDiversityCoefficient * (data.Flowers / GetTotalArea(data)) *
                                plantSpeciesDiversityCoefficient;
            float grassScore = BasicCalculationConstants.GrassDiversityCoefficient * (data.Grass / GetTotalArea(data)) *
                               plantSpeciesDiversityCoefficient;
            float shrubScore = BasicCalculationConstants.ShrubDiversityCoefficient * (data.Shrubs / GetTotalArea(data)) *
                               plantSpeciesDiversityCoefficient;
            float treeScore = BasicCalculationConstants.TreeDiversityCoefficient * (data.Tree / GetTotalArea(data)) *
                              plantSpeciesDiversityCoefficient;

            return flowerScore + grassScore + shrubScore + treeScore;
        }

        public CalculationResult Calculate(CalculationData data)
        {
            return new CalculationResult(CalculateWaterScore(data), CalculateSoilScore(data),
                CalculateAnimalScore(data), CalculatePlantScore(data));
        }
        
        private BasicCalculationModel()
        {
            
        }
    }
}