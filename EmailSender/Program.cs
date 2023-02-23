var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSwaggerGen();

//builder.Services.AddSingleton<IConnectionFactory>(new ConnectionFactory() { HostName = "localHost", DispatchConsumersAsync = true});
//builder.Services.AddSingleton<IPersistentConnection, PersistentConnection>();
//builder.Services.AddHostedService<ConsumingService>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();
app.Run();