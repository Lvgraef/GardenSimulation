using System;

namespace calculation
{
    public class BasicCalculationModel : ICalculationModel
    {
        public static readonly BasicCalculationModel Instance = new();

        private float GetTotalArea(CalculationData data)
        {
            return data.NonPermeable + data.SemiPermeable + data.Bare + data.Flowers + data.Grass +
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

        private float CalculateWaterScore(CalculationData data, bool raw)
        {
            float nonPermeableScore = data.NonPermeable * BasicCalculationConstants.NonPermeableCoefficient;
            float semiPermeableScore = data.SemiPermeable * BasicCalculationConstants.SemiPermeableCoefficient;
            float bareScore = data.Bare * BasicCalculationConstants.BareCoefficient;
            float flowerScore = data.Flowers * BasicCalculationConstants.FlowerCoefficient;
            float grassScore = data.Grass * BasicCalculationConstants.GrassCoefficient;
            float shrubScore = data.Shrubs * BasicCalculationConstants.ShrubCoefficient;
            float treeScore = data.Tree * BasicCalculationConstants.TreeCoefficient;

            if (raw)
            {
                return (nonPermeableScore + semiPermeableScore + bareScore + flowerScore + grassScore + shrubScore +
                        treeScore) / GetTotalArea(data);
            }

            return Math.Min(10, (nonPermeableScore + semiPermeableScore + bareScore + flowerScore + grassScore +
                                 shrubScore +
                                 treeScore) / GetTotalArea(data));
        }

        private float CalculateSoilScore(CalculationData data, bool raw)
        {
            if (raw)
            {
                return BasicCalculationConstants.GetFertilizationCoefficient(data.Fertilizer, data.CompostCleanup) *
                       GetVegetationPercentage(data);
            }

            return Math.Min(10,
                BasicCalculationConstants.GetFertilizationCoefficient(data.Fertilizer, data.CompostCleanup) *
                GetVegetationPercentage(data));
        }

        private float CalculateAnimalScore(CalculationData data, bool raw)
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

            if (raw)
            {
                return flyingInsectScore + birdScore + spiderScore + otherAnimalScore;
            }
            
            return Math.Min(10, flyingInsectScore + birdScore + spiderScore + otherAnimalScore);
        }

        private float CalculatePlantScore(CalculationData data, bool raw)
        {
            float plantSpeciesDiversityCoefficient =
                BasicCalculationConstants.GetPlantSpeciesDiversityCoefficient(data.PlantDiversity);

            float flowerScore = BasicCalculationConstants.FlowerDiversityCoefficient *
                                (data.Flowers / GetTotalArea(data)) *
                                plantSpeciesDiversityCoefficient;
            float grassScore = BasicCalculationConstants.GrassDiversityCoefficient * (data.Grass / GetTotalArea(data)) *
                               plantSpeciesDiversityCoefficient;
            float shrubScore = BasicCalculationConstants.ShrubDiversityCoefficient *
                               (data.Shrubs / GetTotalArea(data)) *
                               plantSpeciesDiversityCoefficient;
            float treeScore = BasicCalculationConstants.TreeDiversityCoefficient * (data.Tree / GetTotalArea(data)) *
                              plantSpeciesDiversityCoefficient;

            if (raw)
            {
                return flowerScore + grassScore + shrubScore + treeScore;
            }
            
            return Math.Min(10, flowerScore + grassScore + shrubScore + treeScore);
        }

        public CalculationResult Calculate(CalculationData data, bool raw)
        {
            if (GetTotalArea(data) == 0)
            {
                return new CalculationResult(0, 0, 0, 0);
            }

            return new CalculationResult(CalculateWaterScore(data, raw), CalculateSoilScore(data, raw),
                CalculateAnimalScore(data, raw), CalculatePlantScore(data, raw));
        }

        private BasicCalculationModel()
        {
        }
    }
}