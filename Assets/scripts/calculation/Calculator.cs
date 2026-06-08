using System;
using gardensettings;
using GridSystem;

namespace calculation
{
    public class Calculator
    {
        private readonly ICalculationModel _calculationModel;
        private readonly GridManager _gridManager;

        public Calculator(ICalculationModel calculationModel, GridManager gridManager)
        {
            _calculationModel = calculationModel;
            _gridManager = gridManager;
        }

        public CalculationResult Calculate()
        {
            GardenSettings gardenSettings = _gridManager.GardenSettings;

            float nonPermeableArea = 0,
                semiPermeableArea = 0,
                bareArea = 0,
                flowerArea = 0,
                grassArea = 0,
                shrubArea = 0,
                treeArea = 0;

            _gridManager.ForEachTile(coords =>
            {
                switch (coords.GetMaterial().category)
                {
                    case Material.Category.NonPermeable:
                        nonPermeableArea++;
                        break;
                    case Material.Category.SemiPermeable:
                        semiPermeableArea++;
                        break;
                    case Material.Category.Bare:
                        bareArea++;
                        break;
                    case Material.Category.Flowers:
                        flowerArea++;
                        break;
                    case Material.Category.Grass:
                        grassArea++;
                        break;
                    case Material.Category.Shrubs:
                        shrubArea++;
                        break;
                    case Material.Category.Tree:
                        treeArea++;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            });
            
            CalculationData data = new CalculationData(nonPermeableArea, semiPermeableArea, bareArea, flowerArea,
                grassArea, shrubArea, treeArea, gardenSettings.Fertilizer, gardenSettings.CompostCleanup,
                gardenSettings.FlyingInsects, gardenSettings.Birds, gardenSettings.Spiders, gardenSettings.OtherAnimals,
                gardenSettings.PlantDiversity);
            
            return _calculationModel.Calculate(data);
        }
    }
}