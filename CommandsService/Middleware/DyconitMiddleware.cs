using System.Diagnostics;
using System.Reflection;
using System.Text.Json;
using AutoMapper;
using CommandsService.Attributes;
using CommandsService.Data;
using CommandsService.Dtos;
using CommandsService.EventProcessing;
using CommandsService.Models;
using Microsoft.AspNetCore.Http;

namespace CommandsService.Middleware
{
    public class DyconitMiddleware
    {
        private readonly ILogger<DyconitMiddleware> _logger;
        private readonly RequestDelegate _next;
        private readonly DyconitOptions _options;
        private readonly IMapper _mapper;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public DyconitMiddleware(
            ILogger<DyconitMiddleware> logger,
            RequestDelegate next,
            IMapper mapper,
            IServiceScopeFactory serviceScopeFactory,
            DyconitOptions options = new DyconitOptions())
        {
            _logger = logger;
            _next = next;
            _options = options;
            _mapper = mapper;
            _serviceScopeFactory = serviceScopeFactory;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var repo = scope.ServiceProvider.GetRequiredService<ICommandRepo>();

                MethodInfo methodInfo = typeof(EventProcessor).GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
                    .FirstOrDefault(m => m.GetCustomAttribute<Conit>() != null);

                if (methodInfo == null)
                {
                    await _next(context);
                    // Print something to the logger
                    _logger.LogInformation("--> DyconitMiddleware is running, but attribute is null");
                }
                else
                {
                    var attribute = methodInfo.GetCustomAttribute<Conit>();

                    if (attribute == null)
                    {
                        await _next(context);
                        // Print something to the logger
                        _logger.LogInformation("--> DyconitMiddleware is running, but attribute is null");
                    }
                    else
                    {
                        double _numericalErrorThreshold = attribute.numericalErrorThreshold;
                        int _orderErrorThreshold = attribute.orderErrorThreshold;
                        int _stalenessThreshold = attribute.stalenessThreshold;

                        var numericalError = CalculateNumericalError();
                        if (numericalError > _numericalErrorThreshold)
                        {
                            // log error
                            _logger.LogError($"Numerical error threshold exceeded: {numericalError}");
                        }

                        // Check for order error
                        var orderError = CalculateOrderError();
                        if (orderError > _orderErrorThreshold)
                        {
                            // log error
                            _logger.LogError($"Order error threshold exceeded: {orderError}");
                        }

                        // Check for staleness
                        var staleness = CalculateStaleness();
                        if (staleness > _stalenessThreshold)
                        {
                            // log error
                            _logger.LogError($"Staleness threshold exceeded: {staleness}");

                            // Issue GET request to get the most recent platforms from their database
                            using (var client = new HttpClient())
                            {
                                client.BaseAddress = new Uri("http://localhost:5210/");
                                var response = await client.GetAsync("api/platforms");
                                var content = await response.Content.ReadAsStringAsync();

                                // print the content to the console
                                Console.WriteLine($"--> {content}");

                                var toBeAddedPlatforms = JsonSerializer.Deserialize<List<PlatformCreateDto>>(content);

                                try
                                {
                                    foreach (var platformDto in toBeAddedPlatforms)
                                    {
                                        var platform = _mapper.Map<Platform>(platformDto);
                                        Console.WriteLine($"--> Adding Platform {platform.Name} to DB");
                                        if (!repo.ExternalPlatformExists(platform.ExternalID))
                                        {
                                            repo.CreatePlatform(platform);
                                            repo.SaveChanges();
                                        }
                                        else
                                        {
                                            Console.WriteLine("--> Platform already exists");
                                        }
                                    }

                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"--> Could not add platform to DB: {ex.Message}");
                                }
                            }
                        }
                    }

                    // Print something to the logger
                    _logger.LogInformation("--> DyconitMiddleware is running");

                    await _next(context);
                }
            }
        }

        private int CalculateStaleness()
        {
            return 1;
        }

        private int CalculateOrderError()
        {
            return 1;
        }

        private int CalculateNumericalError()
        {
            return 1;
        }
    }

    public static class DyconitMiddlewareExtensions
    {
        public static IApplicationBuilder UseDyconitsMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<DyconitMiddleware>();
        }
    }
}
