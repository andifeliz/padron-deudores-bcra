using PadronDeudores.Infraestructura.Logs;

var builder = WebApplication.CreateBuilder(args);

builder.AgregarLogsEstructurados();
builder.Services.AddHealthChecks();

var app = builder.Build();

app.MapHealthChecks("/health");

app.Run();
