# ADR 0004 · Base de datos: PostgreSQL

**En una frase:** se usa PostgreSQL porque permite cargar 34 millones de líneas en minutos y agruparlas en la propia base, que es justo lo que pide el procesamiento.

**Estado:** Aceptada

**Qué vas a encontrar**
1. [Contexto](#1-contexto)
2. [Decisión](#2-decisión)
3. [Alternativas consideradas](#3-alternativas-consideradas)
4. [Consecuencias](#4-consecuencias)
5. [Referencias](#5-referencias)

---

## 1. Contexto

El enunciado permite MySQL, PostgreSQL o DynamoDB. Lo que la base tiene que resolver:

| Necesidad                                          | Tamaño                              |
| -------------------------------------------------- | ----------------------------------- |
| Cargar las líneas del archivo                      | 34.385.567 líneas (5,9 GB)          |
| Agrupar por deudor y por entidad                   | 19.926.484 deudores y 485 entidades |
| Reemplazar los resultados sin cortar las consultas | ~20 M de filas                      |

---

## 2. Decisión

Se usa **PostgreSQL**, en un contenedor Docker con la imagen oficial.

### 2.1 Carga masiva con `COPY` binario

- `COPY` carga muchas filas en **una sola operación**, en lugar de un `INSERT` por fila, que obliga a ir y volver a la base 34 millones de veces (\*).
- En **formato binario**, los valores viajan ya en el formato interno de PostgreSQL: no hay que convertir números a texto y de vuelta, ni escapar caracteres.
- **Npgsql** es la librería que usa .NET para hablar con PostgreSQL (el *driver*). Para `COPY` ofrece `BeginBinaryImportAsync` (\*\*), que trabaja en tres pasos:
  1. **Abrir:** se abre la carga hacia la tabla staging.
  2. **Escribir cada fila:** por cada línea leída del archivo se avisa "empieza una fila nueva" (`StartRowAsync`) y se pasa el valor de cada columna (`WriteAsync`): entidad, identificación, situación, préstamo, etc. Npgsql no manda cada fila sola por la red: las acumula en un **buffer** (un espacio chico de memoria) y lo envía cuando se llena.
  3. **Confirmar:** al terminar el archivo, `CompleteAsync` confirma la carga. Si algo falla antes, no se guarda nada.

  En memoria solo está la línea actual y ese buffer: el archivo **fluye** del disco a la base sin juntarse entero en ningún lado.

### 2.2 Staging con tablas `UNLOGGED`

- Por seguridad, PostgreSQL anota cada cambio en un registro llamado **WAL** (*write-ahead log*) antes de escribirlo en la tabla. Si el servidor se corta, usa ese registro para recuperar los datos. El costo es que **cada dato se escribe dos veces**.
- Una tabla `UNLOGGED` **no escribe en el WAL**, así que la carga es mucho más rápida (\*\*\*).
- La contra: si el servidor se corta, esa tabla queda vacía. Para staging no importa, porque es una mesa de trabajo temporal y el archivo se puede volver a procesar.

### 2.3 Reemplazo atómico: DDL transaccional

- En PostgreSQL, **crear y renombrar tablas también va dentro de una transacción** (*DDL transaccional*): o se aplica todo, o no se aplica nada.
- Eso permite el **table swap** de [ADR 0010](0010-reimportacion-reemplaza-resumenes.md): los resúmenes nuevos se arman en tablas aparte y se intercambian por las actuales en un instante, sin que la consulta-api vea datos a medias.

---

## 3. Alternativas consideradas

| Alternativa | Por qué no                                                                                                                                                                                       |
| ----------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| MySQL       | También es viable (tiene `LOAD DATA` y `RENAME TABLE` atómico), pero no tiene un equivalente directo a `UNLOGGED` por tabla ni un `COPY` binario en streaming tan directo desde .NET             |
| DynamoDB    | Es **no relacional** (clave-valor): no tiene `GROUP BY` ni sumas, así que habría que agrupar 19,9 M de deudores en la aplicación o con millones de escrituras, y el top N requiere índices extra |

### Comparación

| Necesidad         | PostgreSQL                                      | MySQL                                                                                          | DynamoDB                                                                 |
| ----------------- | ----------------------------------------------- | ---------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------ |
| Tipo de base      | Relacional (SQL)                                | Relacional (SQL)                                                                               | **No relacional** (clave-valor): se consulta por clave, sin SQL ni joins |
| Carga masiva      | `COPY` binario en streaming                     | `LOAD DATA LOCAL INFILE`: en texto (CSV) y deshabilitado por defecto en el servidor (\*\*\*\*) | Escrituras de a 25 ítems: ~1,4 M de llamadas para 34 M de líneas         |
| Staging rápido    | Tablas `UNLOGGED`                               | Sin equivalente por tabla (el motor `MEMORY` guarda todo en RAM)                               | No aplica                                                                |
| Agrupar y sumar   | `GROUP BY` + `SUM`                              | `GROUP BY` + `SUM`                                                                             | **No existe**: habría que agrupar 19,9 M de deudores en la aplicación    |
| Reemplazo atómico | DDL transaccional (`RENAME` en una transacción) | `RENAME TABLE` atómico, pero el resto del DDL no es transaccional                              | No aplica: no hay tablas que intercambiar                                |
| Top N por monto   | Índice ordenado                                 | Índice ordenado                                                                                | Requiere un índice secundario extra o recorrer toda la tabla             |
| Montos exactos    | `numeric`                                       | `DECIMAL` (equivalente)                                                                        | `Number` (exacto)                                                        |

---

## 4. Consecuencias

**Lo que ganamos:**

- La carga del archivo real lleva minutos, y la agrupación la hace la base, no la aplicación.
- El reemplazo de resultados es atómico, gracias al DDL transaccional.

**Lo que resignamos:**

- El procesamiento usa funciones propias de PostgreSQL (`COPY` binario, `UNLOGGED`, DDL transaccional): cambiar de motor obligaría a reescribir la carga y el reemplazo.

---

## 5. Referencias

(\*) PostgreSQL Global Development Group. (s. f.). *COPY*. PostgreSQL Documentation. https://www.postgresql.org/docs/current/sql-copy.html

(\*\*) Npgsql. (s. f.). *COPY*. Npgsql Documentation. https://www.npgsql.org/doc/copy.html

(\*\*\*) PostgreSQL Global Development Group. (s. f.). *CREATE TABLE: UNLOGGED*. PostgreSQL Documentation. https://www.postgresql.org/docs/current/sql-createtable.html

> "Data written to unlogged tables is not written to the write-ahead log [...], which makes them considerably faster than ordinary tables. However, they are not crash-safe: an unlogged table is automatically truncated after a crash or unclean shutdown."

(\*\*\*\*) Oracle. (s. f.). *LOAD DATA Statement*. MySQL 8.0 Reference Manual. https://dev.mysql.com/doc/refman/8.0/en/load-data.html
