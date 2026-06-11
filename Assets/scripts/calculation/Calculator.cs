using System;
using gardensettings;
using GridSystem;

namespace calculation
{
    public class Calculator
    {
        private readonly ICalculationModel _calculationModel;
        private readonly GridManager _gridManager;
        private readonly GardenSettings _gardenSettings;

        public Calculator(ICalculationModel calculationModel, GridManager gridManager, GardenSettings gardenSettings)
        {
            _calculationModel = calculationModel;
            _gridManager = gridManager;
            _gardenSettings = gardenSettings;
        }

        public ReportData Calculate()
        {
            float nonPermeableArea = 0,
                semiPermeableArea = 0,
                bareArea = 0,
                flowerArea = 0,
                grassArea = 0,
                shrubArea = 0,
                treeArea = 0;
            
            _gridManager.ForEachTile((tile, _, _) =>
            {
                if (tile.GetMaterial() is not null)
                {
                    switch (tile.GetMaterial()?.category)
                    {
                        case Material.Category.NonPermeable:
                            nonPermeableArea += _gridManager.tileArea;
                            break;
                        case Material.Category.SemiPermeable:
                            semiPermeableArea += _gridManager.tileArea;
                            break;
                        case Material.Category.Bare:
                            bareArea += _gridManager.tileArea;
                            break;
                        case Material.Category.Flowers:
                            flowerArea += _gridManager.tileArea;
                            break;
                        case Material.Category.Grass:
                            grassArea += _gridManager.tileArea;
                            break;
                        case Material.Category.Shrubs:
                            shrubArea += _gridManager.tileArea;
                            break;
                        case Material.Category.Tree:
                            treeArea += _gridManager.tileArea;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            });
            
            CalculationData data = new CalculationData(nonPermeableArea, semiPermeableArea, bareArea, flowerArea,
                grassArea, shrubArea, treeArea, _gardenSettings.Fertilizer, _gardenSettings.CompostCleanup,
                _gardenSettings.FlyingInsects, _gardenSettings.Birds, _gardenSettings.Spiders, _gardenSettings.OtherAnimals,
                _gardenSettings.PlantDiversity);

            CalculationResult result = _calculationModel.Calculate(data);
            
            return new ReportData(result, nonPermeableArea, semiPermeableArea, bareArea, flowerArea, grassArea, shrubArea, treeArea);
        }
    }
}