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
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

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
                    _logger.LogInformation($"--> methodInfo is {methodInfo.Name}");
                    var attribute = methodInfo.GetCustomAttribute<Conit>();
                    _logger.LogInformation($"--> attribute is {attribute}");

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
                        double _stalenessThreshold = attribute.stalenessThreshold;

                        var numericalError = CalculateNumericalError();
                        if (numericalError >= _numericalErrorThreshold)
                        {
                            // log error
                            _logger.LogError($"Numerical error threshold exceeded: {numericalError}");
                        }

                        // Check for order error
                        var orderError = CalculateOrderError();
                        if (orderError >= _orderErrorThreshold)
                        {
                            // log error
                            _logger.LogError($"Order error threshold exceeded: {orderError}");
                        }

                        // Check for staleness
                        var staleness = CalculateStaleness(_stalenessThreshold);
                        if (staleness >= _stalenessThreshold)
                        {
                            // log error
                            _logger.LogError($"Staleness threshold exceeded: {staleness}");
                        }
                    }

                    // Print something to the logger
                    _logger.LogInformation("--> DyconitMiddleware is running");

                    await _next(context);
                }
            }
            _logger.LogInformation("--> DyconitMiddleware is done");
        }

        private double CalculateStaleness(double _stalenessThreshold)
        {
            // get the last received message timestamp from the message bus
            DateTime lastMessageTimestamp = GetLastReceivedMessageTimestampFromBus();

            // calculate the time elapsed since the last message was received
            TimeSpan elapsed = DateTime.UtcNow - lastMessageTimestamp;

            // check if the elapsed time exceeds the staleness threshold
            if (elapsed.TotalMilliseconds > _stalenessThreshold)
            {
                // log error
                _logger.LogError($"Staleness threshold exceeded: {elapsed.TotalMilliseconds} ms");
                return elapsed.TotalMilliseconds;
            }

            return 0;
        }

        /*
         * This implementation assumes that you have a queue called "my_queue" in your RabbitMQ instance, and that messages are being published to that queue.
         * The method sets up a consumer for the queue, and listens for incoming messages. When a message is received,
         * the callback function updates the lastMessageTimestamp variable with the timestamp of the received message.
         * Once the method has finished consuming messages from the queue, it returns the timestamp of the last received message.
         */

        private DateTime GetLastReceivedMessageTimestampFromBus()
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                // set up the queue and consumer
                channel.QueueDeclare(queue: "my_queue", durable: false, exclusive: false, autoDelete: false, arguments: null);
                var consumer = new EventingBasicConsumer(channel);

                // set up a callback function to handle received messages
                long lastMessageTimestamp = 0;
                consumer.Received += (model, ea) =>
                {
                    lastMessageTimestamp = ea.BasicProperties.Timestamp.UnixTime;
                    // do something with the received message
                };

                // start consuming messages from the queue
                channel.BasicConsume(queue: "my_queue", autoAck: true, consumer: consumer);

                // return the Unix timestamp of the last received message
                return DateTime.FromBinary(lastMessageTimestamp);
            }
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
