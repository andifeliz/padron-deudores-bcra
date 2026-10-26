using PadronDeudores.Importador.Api.Importaciones;
using PadronDeudores.Infraestructura;
using PadronDeudores.Infraestructura.Logs;
using PadronDeudores.Infraestructura.Persistencia;

var builder = WebApplication.CreateBuilder(args);

builder.AgregarLogsEstructurados();
builder.AgregarBaseDeDatos("Importador");
builder.AgregarColaImportaciones();
builder.AgregarArchivosS3();
builder.Services.AgregarOpciones<OpcionesImportacion>(OpcionesImportacion.Seccion);
builder.Services.AddSingleton<RepositorioImportaciones>();
builder.Services.AddSingleton<RegistroImportacion>();
builder.Services.AddProblemDetails();
builder.Services.AddHealthChecks();

var app = builder.Build();

app.MapHealthChecks("/health");
app.MapImportarDesdeRuta();
app.MapImportarDesdeUpload();
app.MapObtenerImportacion();

app.Run();
