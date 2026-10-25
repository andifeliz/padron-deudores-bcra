using PadronDeudores.Importador.Worker;
using PadronDeudores.Importador.Worker.Procesamiento;
using PadronDeudores.Infraestructura;
using PadronDeudores.Infraestructura.Archivos;
using PadronDeudores.Infraestructura.Logs;
using PadronDeudores.Infraestructura.Persistencia;

var builder = Host.CreateApplicationBuilder(args);

builder.AgregarLogsEstructurados();
builder.AgregarBaseDeDatos("Importador");
builder.Services.AddSingleton<IOrigenArchivo, OrigenDisco>();
builder.Services.AddSingleton<LectorDeudores>();
builder.Services.AddSingleton<CargadorStaging>();
builder.Services.AddSingleton<ConsolidadorResumenes>();
builder.Services.AddSingleton<RepositorioImportaciones>();
builder.Services.AddSingleton<ProcesadorImportacion>();
builder.Services.AddHostedService<ConsumidorImportaciones>();

var host = builder.Build();
host.Run();
