using Npgsql;
using PadronDeudores.Importador.Worker;
using PadronDeudores.Importador.Worker.Procesamiento;
using PadronDeudores.Infraestructura.Archivos;
using PadronDeudores.Infraestructura.Logs;
using PadronDeudores.Infraestructura.Persistencia;

var builder = Host.CreateApplicationBuilder(args);

builder.AgregarLogsEstructurados();

var cadenaConexion = builder.Configuration.GetConnectionString("Importador")
    ?? throw new InvalidOperationException("Falta la conexión ConnectionStrings:Importador.");

builder.Services.AddSingleton(NpgsqlDataSource.Create(cadenaConexion));
builder.Services.AddSingleton<IOrigenArchivo, OrigenDisco>();
builder.Services.AddSingleton<LectorDeudores>();
builder.Services.AddSingleton<CargadorStaging>();
builder.Services.AddSingleton<ConsolidadorResumenes>();
builder.Services.AddSingleton<RepositorioImportaciones>();
builder.Services.AddSingleton<ProcesadorImportacion>();
builder.Services.AddHostedService<ConsumidorImportaciones>();

var host = builder.Build();
host.Run();
