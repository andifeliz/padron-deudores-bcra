-- Outbox: la importación queda guardada aunque la cola falle; encolada_en dice si su mensaje ya se envió a SQS.
ALTER TABLE importaciones ADD COLUMN encolada_en timestamptz;

-- Las importaciones anteriores a esta migración ya se habían encolado.
UPDATE importaciones SET encolada_en = creada_en;

-- Índice chico: solo contiene las que faltan encolar, que es lo que busca el repartidor.
CREATE INDEX ix_importaciones_sin_encolar ON importaciones (creada_en) WHERE encolada_en IS NULL;
