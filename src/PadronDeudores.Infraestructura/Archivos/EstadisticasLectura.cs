namespace PadronDeudores.Infraestructura.Archivos;

// Cuenta las líneas mientras se recorre el archivo, para el resumen final.
public sealed class EstadisticasLectura
{
    private readonly Dictionary<string, long> _lineasPorPeriodo = [];

    public long LineasLeidas { get; private set; }
    public long LineasInvalidas { get; private set; }
    public IReadOnlyDictionary<string, long> LineasPorPeriodo => _lineasPorPeriodo;

    public void ContarValida(string periodo)
    {
        LineasLeidas++;
        _lineasPorPeriodo[periodo] = _lineasPorPeriodo.GetValueOrDefault(periodo) + 1;
    }

    public void ContarInvalida()
    {
        LineasLeidas++;
        LineasInvalidas++;
    }
}
