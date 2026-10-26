using Npgsql;

namespace PadronDeudores.Infraestructura.Persistencia;

// Calcula los resúmenes por deudor y por entidad desde staging y reemplaza las tablas finales (table swap).
// Usa la transacción de quien lo llama: el reemplazo se confirma junto con lo demás, o no se confirma nada.
public sealed class ConsolidadorResumenes
{
    private const string Reemplazo = """
        DROP TABLE IF EXISTS deudores_nueva, entidades_nueva;
        CREATE TABLE deudores_nueva (LIKE deudores INCLUDING DEFAULTS INCLUDING CONSTRAINTS);
        CREATE TABLE entidades_nueva (LIKE entidades INCLUDING DEFAULTS INCLUDING CONSTRAINTS);

        -- Situación máxima: el código con mayor nivel de riesgo; en empate, el código más bajo.
        -- Los códigos sin nivel (null) quedan últimos: solo se usan si el deudor no tiene otros.
        INSERT INTO deudores_nueva (nro_identificacion, situacion_maxima, suma_total_prestamos)
        SELECT nro_identificacion,
               (array_agg(situacion ORDER BY nivel_riesgo DESC NULLS LAST, situacion))[1],
               sum(prestamos)
        FROM staging
        GROUP BY nro_identificacion;

        INSERT INTO entidades_nueva (codigo_entidad, suma_total_prestamos)
        SELECT codigo_entidad, sum(prestamos)
        FROM staging
        GROUP BY codigo_entidad;

        -- Claves e índice después de cargar: construirlos una vez es más rápido que mantenerlos fila por fila.
        ALTER TABLE deudores_nueva ADD CONSTRAINT deudores_nueva_pkey PRIMARY KEY (nro_identificacion);
        ALTER TABLE entidades_nueva ADD CONSTRAINT entidades_nueva_pkey PRIMARY KEY (codigo_entidad);
        CREATE INDEX ix_deudores_nueva_top ON deudores_nueva (suma_total_prestamos DESC, nro_identificacion);
        CREATE INDEX ix_deudores_nueva_situacion_top ON deudores_nueva (situacion_maxima, suma_total_prestamos DESC, nro_identificacion);
        GRANT SELECT ON deudores_nueva, entidades_nueva TO consulta;
        ANALYZE deudores_nueva, entidades_nueva;

        -- El intercambio: dura milisegundos y nadie ve un estado intermedio.
        DROP TABLE deudores, entidades;
        ALTER TABLE deudores_nueva RENAME TO deudores;
        ALTER TABLE entidades_nueva RENAME TO entidades;
        ALTER INDEX deudores_nueva_pkey RENAME TO deudores_pkey;
        ALTER INDEX entidades_nueva_pkey RENAME TO entidades_pkey;
        ALTER INDEX ix_deudores_nueva_top RENAME TO ix_deudores_top;
        ALTER INDEX ix_deudores_nueva_situacion_top RENAME TO ix_deudores_situacion_top;
        """;

    public async Task ReemplazarAsync(NpgsqlConnection conexion, NpgsqlTransaction transaccion, CancellationToken ct)
    {
        await using var comando = new NpgsqlCommand(Reemplazo, conexion, transaccion) { CommandTimeout = 0 };
        await comando.ExecuteNonQueryAsync(ct);
    }
}
