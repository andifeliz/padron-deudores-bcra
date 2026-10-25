#!/bin/bash
# Crea los usuarios de la aplicación. Corre una sola vez, al crear la base.
set -e

psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname "$POSTGRES_DB" <<-SQL
	CREATE ROLE importador LOGIN PASSWORD '$DB_IMPORTADOR_PASSWORD';
	CREATE ROLE consulta   LOGIN PASSWORD '$DB_CONSULTA_PASSWORD';
	-- importador crea las tablas con las migraciones; consulta recibe permisos tabla por tabla.
	GRANT CREATE, USAGE ON SCHEMA public TO importador;
SQL
