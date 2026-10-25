namespace PadronDeudores.Infraestructura.Archivos;

// Abre el archivo a importar, venga de donde venga (disco o S3).
public interface IOrigenArchivo
{
    Task<Stream> AbrirAsync(string archivo, CancellationToken ct);
}
