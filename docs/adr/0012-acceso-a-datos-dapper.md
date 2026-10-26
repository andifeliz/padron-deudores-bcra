# ADR 0012 · Acceso a datos: Dapper y Npgsql, sin Entity Framework

**En una frase:** las consultas se escriben en SQL con Dapper y la carga masiva usa Npgsql directo; no se usa Entity Framework.

**Estado:** Aceptada

**Qué vas a encontrar**
1. [Contexto](#1-contexto)
2. [Decisión](#2-decisión)
3. [Alternativas consideradas](#3-alternativas-consideradas)
4. [Consecuencias](#4-consecuencias)

---

## 1. Contexto

Lo que el sistema hace con la base:

| Operación                                   | Tipo                         |
| ------------------------------------------- | ---------------------------- |
| Cargar 34 M de líneas en staging            | Carga masiva (`COPY`)        |
| Calcular resúmenes y hacer el table swap    | SQL sobre conjuntos de datos |
| Consultar un deudor, una entidad o el top N | 3 consultas simples          |
| Registrar y actualizar importaciones        | 3 o 4 `INSERT`/`UPDATE`      |

---

## 2. Decisión

- **Npgsql directo** para el `COPY` binario ([ADR 0005](0005-procesamiento-streaming-copy-staging.md)).
- **Dapper** para todo lo demás: el SQL se escribe a mano y Dapper convierte los resultados en objetos C#.
- El SQL queda explícito y en un solo estilo de acceso a datos.

---

## 3. Alternativas consideradas

| Alternativa      | Por qué no                                                                                                                                                                                                  |
| ---------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Entity Framework | No hace `COPY` ni conviene para SQL masivo, que es lo central del sistema; habría dos formas de acceder a la base (EF y SQL a mano). Brilla en dominios con mucha edición de entidades, que no es este caso |

---

## 4. Consecuencias

**Lo que ganamos:**

- Control total del SQL, que es lo central del sistema, y un solo estilo de acceso a datos.

**Lo que resignamos:**

- El SQL va como texto: un error en el nombre de una columna aparece al ejecutar, no al compilar. Lo cubren los tests de integración, que corren contra un PostgreSQL real.
