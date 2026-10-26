# ADR 0002 · Se procesan todas las líneas, sin filtrar por período

**En una frase:** se procesan las líneas de todos los períodos que trae el archivo, y el resumen final informa cuántas líneas hubo de cada uno.

**Estado:** Aceptada

**Qué vas a encontrar**
1. [Contexto](#1-contexto)
2. [Decisión](#2-decisión)
3. [Alternativas consideradas](#3-alternativas-consideradas)
4. [Consecuencias](#4-consecuencias)
5. [Referencias](#5-referencias)

---

## 1. Contexto

El archivo corresponde a noviembre 2023, pero el campo 2 (período, `AAAAMM`) trae tres valores:

| Período |     Líneas | Entidades |
| :-----: | ---------: | --------: |
|  202311 | 33.799.893 |       436 |
|  202310 |    514.095 |        38 |
|  202309 |     71.579 |        11 |

- **Cada entidad tiene un solo período**: ninguna aparece en dos meses, así que no hay datos duplicados.
- La base se generó el **18/01/2024**, según el nombre del archivo `Fecha_Proceso_20240118.txt` (\*). El LEAME aclara que no incluye novedades posteriores a la **fecha de corte**, es decir, al momento en que se tomó la "foto" de la base (\*).
- **Ningún documento explica este caso.** La norma de presentación solo dice que las entidades informan con frecuencia mensual (\*\*), sin aclarar qué se publica si una se atrasa; el texto ordenado tampoco lo trata.

**Hipótesis:** al 18/01/2024, esas 49 entidades todavía no habían informado noviembre, y el BCRA publicó el último dato que tenía de cada una. Si lo hubieran enviado a tiempo, aparecería noviembre y no octubre o septiembre. Lo respalda que cada entidad tiene un solo período, pero ningún documento lo confirma.

---

## 2. Decisión

- Se procesan **todas las líneas**, sin mirar el período.
- El resumen final (log estructurado y notificación) informa las **líneas por período**:

```json
"lineasPorPeriodo": { "202311": 33799893, "202310": 514095, "202309": 71579 }
```

---

## 3. Alternativas consideradas

| Alternativa                                 | Por qué no                                 |
| ------------------------------------------- | ------------------------------------------ |
| Procesar solo el período principal (202311) | 49 entidades y sus deudores desaparecerían |
| Guardar el período de cada entidad          | Agrega una columna a `entidades`           |

---

## 4. Consecuencias

**Lo que ganamos:**

- No se pierde ninguna entidad: las 49 que informaron tarde siguen en los resultados.
- El resumen deja a la vista cuántas líneas hay de cada período.

**Lo que resignamos:**

- Los resultados mezclan datos de distintos meses, sin poder distinguirlos por API, porque no se guarda el período de cada entidad.

---

## 5. Referencias

(\*) Banco Central de la República Argentina. (2024). *Central de Deudores del Sistema Financiero: LEAME* [Documento incluido en el archivo 202311DEUDORES_20240122204012], primer párrafo y sección "Aclaración".

> "La información contenida en esta base de datos no incluye las novedades presentadas por las entidades informantes [...] con posterioridad a la fecha de corte utilizada para la elaboración de la presente publicación."
>
> "A fin de brindar la fecha de actualización de los datos que componen la Central de Deudores del Sistema Financiero, se incluye un archivo cuya denominación es la correspondiente a la fecha de generación de la base."

(\*\*) Banco Central de la República Argentina. (s. f.). *Presentación de Informaciones al Banco Central: Sección 3. Deudores del Sistema Financiero* (Texto ordenado), punto 3.1.1.1, p. 1. https://www.bcra.gob.ar/archivos/Pdfs/Texord/t-SO-s03.pdf

> "La información incluida en este capítulo tendrá frecuencia mensual"
