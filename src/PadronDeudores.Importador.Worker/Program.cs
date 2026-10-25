using PadronDeudores.Importador.Worker;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<ConsumidorImportaciones>();

var host = builder.Build();
host.Run();
