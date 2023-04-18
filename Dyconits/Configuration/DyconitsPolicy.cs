using Dyconits.Configuration;
using Microsoft.Extensions.Logging;

namespace Dyconits.Configuration;

internal class DyconitsPolicy : IDyconitsPolicy
{
    private readonly IDyconitsOptions _options;

    private readonly ILogger<DyconitsPolicy> _logger;

    public double Staleness => _options.Staleness;

    public int NumericalError => _options.NumericalError;

    public DyconitsPolicy(IDyconitsOptions options, ILogger<DyconitsPolicy> logger)
    {
        _options = options;
        _logger = logger;
    }
}
