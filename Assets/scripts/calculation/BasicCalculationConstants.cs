using System;

using gardensettings;

using GridSystem;

namespace calculation
{
    public static class BasicCalculationConstants
    {
        // Pijler 1 - Water in de bodem
        public const float NonPermeableWaterCoefficient = 0f; // Volledige volharding
        public const float SemiPermeableWaterCoefficient = 2f; // Doorlatend verhard of grind
        public const float BareWaterCoefficient = 3f; // niet verhard en zonder beplanting
        public const float FlowerWaterCoefficient = 3.5f; // bloemen of moestuin of bodembedekker
        public const float GrassWaterCoefficient = 5f; // gras
        public const float ShrubWaterCoefficient = 10f; // Struiken, heg, haag of kleine bomen
        public const float TreeWaterCoefficient = 15f; // Grote boom
        
        // Pijler 2 - Gezonde bodem
        public static float GetFertilizationCoefficient(Fertilizer fertilizer, CompostCleanup compostCleanup)
        {
            return (fertilizer, compostCleanup) switch
            {
                (Fertilizer.None, CompostCleanup.None) => 7f, // Geen besmetting - Niets
                (Fertilizer.None, CompostCleanup.Half) => 2f, // Geen besmetting - De helft
                (Fertilizer.None, CompostCleanup.All) => 0f, // Geen besmetting - Alles
                (Fertilizer.Artificial, CompostCleanup.None) => 5f, // Kunstmest (Blauwe korrels) - Niets
                (Fertilizer.Artificial, CompostCleanup.Half) => 4f, // Kunstmest (Blauwe korrels) - De helft
                (Fertilizer.Artificial, CompostCleanup.All) => 3f, // Kunstmest (Blauw korrels) - Alles
                (Fertilizer.Organic, CompostCleanup.None) => 10f, // Organische mest (Bruin) - Niets
                (Fertilizer.Organic, CompostCleanup.Half) => 9f, // Organische mest (Bruin) - De helft 
                (Fertilizer.Organic, CompostCleanup.All) => 8f, // Organische mest (Bruin) - Alles
                _ => 0f
            };
        }
        
        // Pijler 3 - Gezonde leefomgeving voor dieren
        public const float FlyingInsectCoefficient = 2.5f; // Vlinders en bijen
        public const float BirdCoefficient = 2.5f; // Vogels
        public const float SpiderCoefficient = 2.5f; // Spinnen
        public const float OtherAnimalCoefficient = 2.5f; // Andere dieren
        
        // Pijler 4 - Plantdiversiteit
        public static float GetPlantSpeciesDiversityCoefficient(PlantDiversity plantDiversity)
        {
            return plantDiversity switch
            {
                PlantDiversity.None => 0f,
                PlantDiversity.Little => 2f,
                PlantDiversity.Medium => 5f,
                PlantDiversity.High => 8f,
                PlantDiversity.Highest => 12f,
                _ => 0f
            };
        }

        // Verdeelcoefficient
        public const float FlowerDiversityCoefficient = 1f;
        public const float GrassDiversityCoefficient = 0.25f;
        public const float ShrubDiversityCoefficient = 2f;
        public const float TreeDiversityCoefficient = 3f;
    }
}