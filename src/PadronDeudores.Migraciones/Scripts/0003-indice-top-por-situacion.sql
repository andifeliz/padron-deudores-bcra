-- Top de deudores filtrado por situación: una lista ordenada por suma para cada situación.
-- Las importaciones siguientes lo crean de nuevo en el intercambio de tablas.
CREATE INDEX ix_deudores_situacion_top ON deudores (situacion_maxima, suma_total_prestamos DESC, nro_identificacion);
