namespace PadronDeudores.Importador.Api.Importaciones;

// Respuesta de los pedidos de importación: el id para consultar su estado.
public sealed record ImportacionAceptada(Guid Id, string Estado);
