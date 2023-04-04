using CommandsService.AsyncDataService;
using CommandsService.Data;
using CommandsService.EventProcessing;
using Microsoft.EntityFrameworkCore;
using CommandsService.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
if (builder.Environment.IsProduction())
{
    Console.WriteLine("--> Using SQL Server");
    builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("CommandServiceConn")));
}
else
{
    Console.WriteLine("--> Using In-Memory Db");
    builder.Services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase("InMem"));
}


builder.Services.AddScoped<ICommandRepo, CommandRepo>();

builder.Services.AddSingleton<IEventProcessor, EventProcessor>();

builder.Services.AddHostedService<MessageBusSubscriber>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();

PrepDb.PrepPopulation(app, builder.Environment.IsProduction());

app.UseAuthorization();

app.MapControllers();

app.UseDyconitsMiddleware();

app.Run();
