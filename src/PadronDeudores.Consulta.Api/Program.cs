using PadronDeudores.Consulta.Api.Deudores;
using PadronDeudores.Consulta.Api.Entidades;
using PadronDeudores.Infraestructura;
using PadronDeudores.Infraestructura.Logs;
using PadronDeudores.Infraestructura.Persistencia;

var builder = WebApplication.CreateBuilder(args);

builder.AgregarLogsEstructurados();
builder.AgregarBaseDeDatos("Consulta");
builder.Services.AgregarOpciones<OpcionesConsulta>(OpcionesConsulta.Seccion);
builder.Services.AddSingleton<RepositorioDeudores>();
builder.Services.AddSingleton<RepositorioEntidades>();
builder.Services.AddProblemDetails();
builder.Services.AddHealthChecks();

var app = builder.Build();

app.MapHealthChecks("/health");
app.MapObtenerDeudor();
app.MapTopDeudores();
app.MapObtenerEntidad();

app.Run();
