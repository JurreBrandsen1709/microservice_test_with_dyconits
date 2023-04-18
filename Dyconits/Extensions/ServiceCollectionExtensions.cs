using Microsoft.Extensions.DependencyInjection;
using Dyconits.Configuration;
using Dyconits.Services;
using Dyconits.Exceptions;
using Dyconits.Event;

namespace Dyconits.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection ConfigureDyconits(this IServiceCollection collection)
    {
        var options = new DyconitsOptions
        {
            Demo = true ,
            Staleness = 0.0,
            NumericalError = 0,
        };

        return ConfigureDyconits(collection, options);
    }

    public static IServiceCollection ConfigureDyconits(this IServiceCollection collection, DyconitsOptions options)
    {
        collection.AddSingleton(_ => options);
        collection.AddSingleton<IDyconitsPolicy, DyconitsPolicy>();
        collection.AddTransient<IDyconitsEvent, DyconitsEvent>();
        collection.AddHostedService<DyconitHostedService>();

        return collection;
    }


    private static DyconitsException MissingEnvVariable(string variable)
    {
        return new DyconitsException($"Environment variable '{variable}' not set.");
    }
}