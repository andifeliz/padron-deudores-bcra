using PadronDeudores.Importador.Worker;
using PadronDeudores.Importador.Worker.Notificaciones;
using PadronDeudores.Importador.Worker.Procesamiento;
using PadronDeudores.Infraestructura;
using PadronDeudores.Infraestructura.Archivos;
using PadronDeudores.Infraestructura.Logs;
using PadronDeudores.Infraestructura.Persistencia;

var builder = Host.CreateApplicationBuilder(args);

builder.AgregarLogsEstructurados();
builder.AgregarBaseDeDatos("Importador");
builder.AgregarColaImportaciones();
builder.AgregarArchivosS3();
builder.Services.AddSingleton<OrigenDisco>();
builder.Services.AddSingleton<IOrigenArchivo, OrigenSegunArchivo>();
builder.Services.AddSingleton<LectorDeudores>();
builder.Services.AddSingleton<CargadorStaging>();
builder.Services.AddSingleton<ConsolidadorResumenes>();
builder.Services.AddSingleton<RepositorioImportaciones>();
builder.Services.AgregarOpciones<OpcionesWebhook>(OpcionesWebhook.Seccion);
// Reintentos con espera creciente y timeout por intento (valores estándar de Microsoft).
builder.Services.AddHttpClient(NotificadorWebhook.Cliente).AddStandardResilienceHandler();
builder.Services.AddSingleton<INotificador, NotificadorWebhook>();
builder.Services.AddSingleton<ProcesadorImportacion>();
builder.Services.AddHostedService<ConsumidorImportaciones>();

var host = builder.Build();
host.Run();
