using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using PadronDeudores.Consulta.Api.Deudores;
using PadronDeudores.Consulta.Api.Entidades;
using PadronDeudores.Infraestructura;
using PadronDeudores.Infraestructura.Logs;
using PadronDeudores.Infraestructura.Persistencia;
using Scalar.AspNetCore;

// Los chequeos con esta etiqueta deciden si la API puede atender (/health/ready).
const string Listo = "ready";

var builder = WebApplication.CreateBuilder(args);

// No revelar en cada respuesta qué servidor web usamos (encabezado "Server: Kestrel").
builder.WebHost.ConfigureKestrel(kestrel => kestrel.AddServerHeader = false);
builder.AgregarLogsEstructurados();
builder.AgregarBaseDeDatos("Consulta");
builder.Services.AgregarOpciones<OpcionesConsulta>(OpcionesConsulta.Seccion);
builder.Services.AddSingleton<RepositorioDeudores>();
builder.Services.AddSingleton<RepositorioEntidades>();
builder.Services.AddProblemDetails();
builder.Services.AddOpenApi(opciones => opciones.AddDocumentTransformer((documento, _, _) =>
{
    documento.Info.Title = "Consulta API";
    documento.Info.Description = "Consulta los resúmenes de deudores y entidades. Los montos están en miles de pesos, como en el archivo del BCRA.";
    return Task.CompletedTask;
}));
builder.Services.AddHealthChecks().AddCheck<ChequeoBaseDeDatos>("base-de-datos", tags: [Listo], timeout: TimeSpan.FromSeconds(3));

var app = builder.Build();

// live: el proceso responde (no revisa dependencias). ready: además, la base responde.
app.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = _ => false });
app.MapHealthChecks("/health/ready", new HealthCheckOptions { Predicate = chequeo => chequeo.Tags.Contains(Listo) });
app.MapOpenApi();
app.MapScalarApiReference();
app.MapObtenerDeudor();
app.MapTopDeudores();
app.MapObtenerEntidad();

app.Run();
