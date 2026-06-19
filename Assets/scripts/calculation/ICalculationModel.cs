namespace calculation
{
    public interface ICalculationModel
    {
        public CalculationResult Calculate(CalculationData data, bool raw);
    }
}