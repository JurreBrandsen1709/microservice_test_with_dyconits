using CommandsService.EventProcessing;
using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace CommandsService.AsyncDataService;

public class MessageBusSubscriber : BackgroundService
{
    private readonly IConfiguration _configuration;
    private readonly IEventProcessor _eventprocessor;
    private string _queueName;
    private IConnection _connection;
    private IModel _channel;

    public MessageBusSubscriber(IConfiguration configuration, IEventProcessor eventProcessor)
    {
        _configuration = configuration;
        _eventprocessor = eventProcessor;

        InitiazeRabbitMQ();
    }

    private void InitiazeRabbitMQ()
    {
        var factory = new ConnectionFactory() { HostName = _configuration["RabbitMQHost"], Port = int.Parse(_configuration["RabbitMQPort"]) };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
        _channel.ExchangeDeclare(exchange: "trigger", type: ExchangeType.Fanout);
        _queueName = _channel.QueueDeclare().QueueName;
        _channel.QueueBind(queue: _queueName, exchange: "trigger", routingKey: "");

        Console.WriteLine("--> Listening on the Message Bus...");

        _connection.ConnectionShutdown += RabbitMQ_ConnectionShutdown;

    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        stoppingToken.ThrowIfCancellationRequested();

        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += (ModuleHandle, ea) =>
        {
            Console.WriteLine("--> Event Received!");

            var body = ea.Body.ToArray();
            var notificationMessage = Encoding.UTF8.GetString(body);

            Console.WriteLine($"--> Message: {notificationMessage}");

            _eventprocessor.ProcessEvent(notificationMessage);
        };

        _channel.BasicConsume(queue: _queueName, autoAck: true, consumer: consumer);

        return Task.CompletedTask;
    }

    private void RabbitMQ_ConnectionShutdown(object sender, ShutdownEventArgs e)
    {
        Console.WriteLine("--> Connection Shutdown");
    }

    public override void Dispose()
    {
        if (_channel.IsOpen)
        {
            _channel.Close();
            _connection.Close();
        }

        base.Dispose();
    }
}