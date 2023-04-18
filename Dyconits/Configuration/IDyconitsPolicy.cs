namespace Dyconits.Configuration
{
    public interface IDyconitsPolicy
    {
        public double Staleness { get; }

        public int NumericalError { get; }
    }
}