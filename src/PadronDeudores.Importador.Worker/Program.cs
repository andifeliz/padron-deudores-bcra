using PadronDeudores.Importador.Worker;
using PadronDeudores.Infraestructura.Logs;

var builder = Host.CreateApplicationBuilder(args);

builder.AgregarLogsEstructurados();
builder.Services.AddHostedService<ConsumidorImportaciones>();

var host = builder.Build();
host.Run();
