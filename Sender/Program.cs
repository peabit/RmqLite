using RabbitMQ.Client;
using Rmq.Interfaces;
using Rmq.Services;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<IConnectionFactory>(new ConnectionFactory() { HostName = "localHost" });
builder.Services.AddSingleton<IPersistentConnection, PersistentConnection>();
builder.Services.AddTransient<IPublisher, Publisher>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();
app.Run();