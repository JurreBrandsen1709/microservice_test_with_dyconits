namespace Dyconits.Configuration;

public interface IDyconitsOptions
{
    public bool Demo { get; set; }

    public double Staleness { get; set; }

    public int NumericalError { get; set; }
}