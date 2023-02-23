using RmqLiteExample;
using RmqLite;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSwaggerGen();

builder.Services.AddRmqLite(builder.Configuration, c =>
{
    c.Subscribe<Consumer, Message>();
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();
app.Run();