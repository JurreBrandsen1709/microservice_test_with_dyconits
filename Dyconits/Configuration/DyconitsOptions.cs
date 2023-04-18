using Dyconits.Configuration;

namespace Dyconits.Configuration
{
    public class DyconitsOptions : IDyconitsOptions
    {
        public bool Demo { get; set; } = true;

        public double Staleness { get; set; } = 0.0;

        public int NumericalError { get; set; } = 0;
    }
}