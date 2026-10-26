# Decisiones de arquitectura

Cada decisión importante del proyecto está en un **ADR** (*Architecture Decision Record*): un documento corto que explica qué se decidió, por qué, qué otras opciones había y qué implica.

**Qué vas a encontrar**
1. [Cómo leer un ADR](#1-cómo-leer-un-adr)
2. [Reglas de negocio](#2-reglas-de-negocio)
3. [Procesamiento y datos](#3-procesamiento-y-datos)
4. [Integración entre servicios](#4-integración-entre-servicios)
5. [Entorno, repositorio y panel](#5-entorno-repositorio-y-panel)

---

## 1. Cómo leer un ADR

Todos tienen la misma estructura:

| Sección                       | Qué responde                                         |
| ----------------------------- | ---------------------------------------------------- |
| **En una frase**              | La decisión, resumida                                |
| **Estado**                    | Si está vigente (*Aceptada*) o la reemplazó otra     |
| **Contexto**                  | Qué problema o pregunta había que resolver           |
| **Decisión**                  | Qué se eligió y cómo funciona                        |
| **Alternativas consideradas** | Qué otras opciones había y por qué no se eligieron   |
| **Consecuencias**             | Qué ganamos y qué resignamos con la decisión         |
| **Referencias**               | Las fuentes: normas del BCRA y documentación oficial |

Un ADR no se reescribe cuando cambia una decisión: se crea uno nuevo que lo reemplaza, y así queda la historia de por qué se cambió. Están numerados en el orden en que se tomaron.

---

## 2. Reglas de negocio

Cómo se interpretan los datos del BCRA y lo que el enunciado no define.

| ADR                                               | Decisión                                                                           |
| ------------------------------------------------- | ---------------------------------------------------------------------------------- |
| [0001](0001-situacion-maxima-por-riesgo.md)       | La situación máxima se elige por nivel de riesgo, no por número                    |
| [0002](0002-periodos-del-archivo.md)              | Se procesan todas las líneas, aunque traigan distintos períodos                    |
| [0003](0003-identificaciones-del-exterior.md)     | Se incluyen los deudores del exterior (tipos 98 y 99)                              |
| [0009](0009-montos-en-miles.md)                   | Los montos se guardan y se devuelven en miles de pesos, como vienen                |
| [0010](0010-reimportacion-reemplaza-resumenes.md) | Cada importación reemplaza los datos de deudores y entidades                       |
| [0011](0011-top-n-limite-y-desempate.md)          | El top N tiene un máximo, un desempate fijo y un filtro por situación              |
| [0017](0017-nombres-de-entidades-y-deudores.md)   | No se cargan los nombres de entidades ni de deudores (`Maeent.txt` y `Nomdeu.txt`) |

---

## 3. Procesamiento y datos

Cómo se procesa un archivo de 34 millones de líneas y cómo se guarda.

| ADR                                                  | Decisión                                                                 |
| ---------------------------------------------------- | ------------------------------------------------------------------------ |
| [0004](0004-base-de-datos-postgresql.md)             | Base de datos: PostgreSQL                                                |
| [0005](0005-procesamiento-streaming-copy-staging.md) | Lectura línea por línea, carga masiva con `COPY` y agrupación en la base |
| [0012](0012-acceso-a-datos-dapper.md)                | Acceso a datos con Dapper y Npgsql, sin Entity Framework                 |
| [0013](0013-migraciones-dbup.md)                     | Las tablas se crean con migraciones versionadas (DbUp)                   |

---

## 4. Integración entre servicios

Cómo llega el archivo, cómo se reparte el trabajo y cómo se avisa el resultado.

| ADR                                 | Decisión                                                                                  |
| ----------------------------------- | ----------------------------------------------------------------------------------------- |
| [0006](0006-entrada-del-archivo.md) | El archivo llega por ruta local o subiéndolo a `/upload`, que lo guarda en S3             |
| [0007](0007-cola-sqs.md)            | Una cola SQS separa la recepción del procesamiento; Outbox para que no se pierdan pedidos |
| [0008](0008-notificacion.md)        | Al terminar, se avisa por webhook y queda un log estructurado                             |

---

## 5. Entorno, repositorio y panel

| ADR                                     | Decisión                                                              |
| --------------------------------------- | --------------------------------------------------------------------- |
| [0014](0014-localstack-version-fija.md) | LocalStack fijado en la versión 4.14.0, la última sin cuenta ni token |
| [0015](0015-monorepo.md)                | Un solo repositorio para todos los servicios                          |
| [0016](0016-panel-web.md)               | Panel web en React y Vite, servido por nginx en su propio contenedor  |
