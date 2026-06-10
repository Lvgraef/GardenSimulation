using gardensettings;

namespace calculation
{
    public class CalculationData
    {
        public readonly float NonPermeable; // Volledige volharding
        public readonly float SemiPermeable; // Doorlatend verhard of grind
        public readonly float Bare; // Niet verhard en zonder beplanting
        public readonly float Flowers; // Bloemen of moestuin of bodembedekker
        public readonly float Grass; // Gras
        public readonly float Shrubs; // Struiken, heg, haag, of kleine bomen
        public readonly float Tree; // Grote boom

        public readonly Fertilizer Fertilizer; // Type mest
        public readonly CompostCleanup CompostCleanup; // Groenresten laten liggen

        public readonly bool FlyingInsects; // Vlinders en bijen
        public readonly bool Birds; // Vogels
        public readonly bool Spiders; // Spinnen
        public readonly bool OtherAnimals; // Andere dieren

        public readonly PlantDiversity PlantDiversity; // Hoeveel soorten planten

        public CalculationData(float nonPermeable, float semiPermeable, float bare, float flowers, float grass,
            float shrubs, float tree, Fertilizer fertilizer, CompostCleanup compostCleanup, bool flyingInsects,
            bool birds, bool spiders, bool otherAnimals, PlantDiversity plantDiversity)
        {
            NonPermeable = nonPermeable;
            SemiPermeable = semiPermeable;
            Bare = bare;
            Flowers = flowers;
            Grass = grass;
            Tree = tree;
            Fertilizer = fertilizer;
            CompostCleanup = compostCleanup;
            FlyingInsects = flyingInsects;
            Birds = birds;
            Spiders = spiders;
            OtherAnimals = otherAnimals;
            PlantDiversity = plantDiversity;
            Shrubs = shrubs;
        }
    }
}