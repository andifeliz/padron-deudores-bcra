using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using PadronDeudores.Importador.Api.Importaciones;
using PadronDeudores.Infraestructura;
using PadronDeudores.Infraestructura.Archivos;
using PadronDeudores.Infraestructura.Logs;
using PadronDeudores.Infraestructura.Persistencia;
using Scalar.AspNetCore;

// Los chequeos con esta etiqueta deciden si la API puede atender (/health/ready).
const string Listo = "ready";

var builder = WebApplication.CreateBuilder(args);

// No revelar en cada respuesta qué servidor web usamos (encabezado "Server: Kestrel").
builder.WebHost.ConfigureKestrel(kestrel => kestrel.AddServerHeader = false);
builder.AgregarLogsEstructurados();
builder.AgregarBaseDeDatos("Importador");
builder.AgregarColaImportaciones();
builder.AgregarArchivosS3();
builder.Services.AgregarOpciones<OpcionesArchivos>(OpcionesArchivos.Seccion);
builder.Services.AgregarOpciones<OpcionesImportacion>(OpcionesImportacion.Seccion);
builder.Services.AddSingleton<RepositorioImportaciones>();
builder.Services.AddSingleton<EnvioImportaciones>();
builder.Services.AddSingleton<RegistroImportacion>();
builder.Services.AddHostedService<RepartidorImportaciones>();
builder.Services.AddProblemDetails();
builder.Services.AddOpenApi(opciones => opciones.AddDocumentTransformer((documento, _, _) =>
{
    documento.Info.Title = "Importador API";
    documento.Info.Description = "Recibe el archivo de deudores del BCRA (por ruta local o subida a S3) y encola su importación.";
    return Task.CompletedTask;
}));
builder.Services.AddHealthChecks().AddCheck<ChequeoBaseDeDatos>("base-de-datos", tags: [Listo], timeout: TimeSpan.FromSeconds(3));

var app = builder.Build();

// live: el proceso responde (no revisa dependencias). ready: además, la base responde.
app.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = _ => false });
app.MapHealthChecks("/health/ready", new HealthCheckOptions { Predicate = chequeo => chequeo.Tags.Contains(Listo) });
app.MapOpenApi();
app.MapScalarApiReference();
app.MapImportarDesdeRuta();
app.MapImportarDesdeUpload();
app.MapObtenerImportacion();

app.Run();
