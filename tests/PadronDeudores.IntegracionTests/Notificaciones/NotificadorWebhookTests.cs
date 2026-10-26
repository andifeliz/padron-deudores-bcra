using System.Net;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using PadronDeudores.Importador.Worker.Notificaciones;
using PadronDeudores.Importador.Worker.Procesamiento;

namespace PadronDeudores.IntegracionTests.Notificaciones;

public class NotificadorWebhookTests
{
    private const string Url = "http://webhook.test/importaciones";

    [Fact]
    public async Task NotificarAsync_EnviaElResultadoEnJsonALaUrl()
    {
        var destino = new DestinoFalso(HttpStatusCode.OK);

        await CrearNotificador(destino).NotificarAsync(NotificacionImportacion.Completada("deudores.txt", Resumen()), CancellationToken.None);

        Assert.Equal((HttpMethod.Post, Url), (destino.Metodo, destino.Url));
        Assert.Contains("\"estado\":\"completada\"", destino.Cuerpo);
        Assert.Contains("\"deudores\":4", destino.Cuerpo);
    }

    [Fact]
    public async Task NotificarAsync_WebhookConError_NoLanzaExcepcion()
    {
        var notificador = CrearNotificador(new DestinoFalso(HttpStatusCode.InternalServerError));

        var error = await Record.ExceptionAsync(() =>
            notificador.NotificarAsync(NotificacionImportacion.Completada("deudores.txt", Resumen()), CancellationToken.None));

        Assert.Null(error);
    }

    private static NotificadorWebhook CrearNotificador(DestinoFalso destino)
    {
        var servicios = new ServiceCollection();
        servicios.AddHttpClient(NotificadorWebhook.Cliente).ConfigurePrimaryHttpMessageHandler(() => destino);
        var clientes = servicios.BuildServiceProvider().GetRequiredService<IHttpClientFactory>();
        return new NotificadorWebhook(clientes, Options.Create(new OpcionesWebhook { Url = Url }), NullLogger<NotificadorWebhook>.Instance);
    }

    private static ResumenImportacion Resumen() =>
        new(Guid.CreateVersion7(), 5, 1, 4, 2, new Dictionary<string, long> { ["202311"] = 3 }, new Dictionary<short, long>(), 0.1);

    // Reemplaza a la red: guarda el pedido recibido y responde con el código indicado.
    private sealed class DestinoFalso(HttpStatusCode codigo) : HttpMessageHandler
    {
        public HttpMethod? Metodo { get; private set; }
        public string? Url { get; private set; }
        public string Cuerpo { get; private set; } = "";

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage pedido, CancellationToken ct)
        {
            (Metodo, Url) = (pedido.Method, pedido.RequestUri?.ToString());
            Cuerpo = await pedido.Content!.ReadAsStringAsync(ct);
            return new HttpResponseMessage(codigo);
        }
    }
}
