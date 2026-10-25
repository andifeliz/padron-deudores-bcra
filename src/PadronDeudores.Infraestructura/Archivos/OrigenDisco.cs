namespace PadronDeudores.Infraestructura.Archivos;

// Abre un archivo del disco (ruta local), optimizado para leerlo de punta a punta.
public sealed class OrigenDisco : IOrigenArchivo
{
    private const int TamanioBuffer = 1024 * 1024;

    public Task<Stream> AbrirAsync(string archivo, CancellationToken ct) =>
        Task.FromResult<Stream>(new FileStream(
            archivo, FileMode.Open, FileAccess.Read, FileShare.Read, TamanioBuffer, FileOptions.Asynchronous | FileOptions.SequentialScan));
}
