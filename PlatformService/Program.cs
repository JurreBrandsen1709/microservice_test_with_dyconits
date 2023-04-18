using Dyconits.Configuration;
using Dyconits.Extensions;
using Microsoft.EntityFrameworkCore;
using PlatformService.AsyncDataServices;
using PlatformService.Data;
using PlatformService.SyncDataServices.Http;

var builder = WebApplication.CreateBuilder(args);

// Disable SSL verification
System.Net.ServicePointManager.ServerCertificateValidationCallback +=
    (sender, certificate, chain, sslPolicyErrors) => true;


if (builder.Environment.IsProduction())
{
    Console.WriteLine("--> @@@@@@@@@@@@@@@@@@@@@@@@@@@@@ Using SQL Server");
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("PlatformsConn")));
}
else
{
    Console.WriteLine("--> Using In-Memory Db");
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseInMemoryDatabase("InMemoryDb"));
}

builder.Services.AddScoped<IPlatformRepo, PlatformRepo>();

builder.Services.AddControllers();
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddHttpClient<ICommandDataClient, HttpCommandDataClient>();
builder.Services.AddSingleton<IMessageBusClient, MessageBusClient>();


// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.ConfigureDyconits(new DyconitsOptions { });
builder.Services.AddSingleton<IDyconitsOptions, DyconitsOptions>();
builder.Services.AddSingleton<Dictionary<string, object>>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

Console.WriteLine($"--> CommandService: {builder.Configuration["CommandService"]}");

app.UseAuthorization();

app.MapControllers();

PrepDb.PrepPopulation(app, builder.Environment.IsProduction());

app.Run();
