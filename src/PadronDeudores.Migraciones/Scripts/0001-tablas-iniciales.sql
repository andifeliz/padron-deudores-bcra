-- Tablas iniciales del padrón de deudores.
-- Corre como importador, que queda como dueño; consulta solo puede leer deudores y entidades.

CREATE TABLE importaciones (
    id            uuid        PRIMARY KEY,
    origen        text        NOT NULL CHECK (origen IN ('upload', 'ruta')),
    archivo       text        NOT NULL,
    estado        text        NOT NULL CHECK (estado IN ('pendiente', 'procesando', 'completada', 'fallida')),
    creada_en     timestamptz NOT NULL DEFAULT now(),
    iniciada_en   timestamptz,
    finalizada_en timestamptz,
    resumen       jsonb,
    error         text
);

-- Mesa de trabajo del worker: sin WAL, sin índices ni claves, para que el COPY sea rápido.
CREATE UNLOGGED TABLE staging (
    codigo_entidad     varchar(5)    NOT NULL,
    nro_identificacion varchar(11)   NOT NULL,
    situacion          smallint      NOT NULL,
    nivel_riesgo       smallint,
    prestamos          numeric(12,1) NOT NULL
);

CREATE TABLE deudores (
    nro_identificacion   varchar(11)   PRIMARY KEY,
    situacion_maxima     smallint      NOT NULL,
    suma_total_prestamos numeric(18,1) NOT NULL
);

-- Top N: suma de mayor a menor y, en empate, identificación de menor a mayor.
CREATE INDEX ix_deudores_top ON deudores (suma_total_prestamos DESC, nro_identificacion);

CREATE TABLE entidades (
    codigo_entidad       varchar(5)    PRIMARY KEY,
    suma_total_prestamos numeric(18,1) NOT NULL
);

GRANT SELECT ON deudores, entidades TO consulta;
