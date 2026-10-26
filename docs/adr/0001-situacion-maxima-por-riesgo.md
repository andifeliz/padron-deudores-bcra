# ADR 0001 · Situación máxima por nivel de riesgo

**En una frase:** `situacion_maxima` guarda el código de la línea más riesgosa del deudor, comparando por nivel de riesgo y no por valor numérico.

**Estado:** Aceptada

**Qué vas a encontrar**
1. [Contexto](#1-contexto)
2. [Decisión](#2-decisión)
3. [Alternativas consideradas](#3-alternativas-consideradas)
4. [Consecuencias](#4-consecuencias)
5. [Referencias](#5-referencias)

---

## 1. Contexto

El enunciado define: `situacion_maxima` = "Máximo valor entre registros coincidentes - Campo 6".

El máximo numérico funciona para los códigos 1 a 5, donde un número más alto significa más riesgo. Pero el BCRA usa otros códigos que no siguen esa escala:

| Código | Significado                               | Problema con el máximo numérico                |
| :----: | ----------------------------------------- | ---------------------------------------------- |
|   11   | Cubierta 100 % con garantía preferida "A" | La norma la trata como **normal**, pero 11 > 5 |
| 21, 22 | Riesgo bajo, variantes "2 a" y "2 b"      | 21 > 5                                         |
|   23   | En tratamiento especial                   | 23 > 5                                         |
|   99   | No clasificado (sin deuda a fin de mes)   | 99 > 5                                         |

---

## 2. Decisión

### 2.1 Regla

Cada código se traduce a un **nivel de riesgo** para poder compararlo:

|    Códigos    |  Nivel  | Fuente                                                                               |
| :-----------: | :-----: | ------------------------------------------------------------------------------------ |
|       99      |    0    | Criterio propio: sin clasificación, no le gana a una nota real (\*\*\*)              |
|     1, 11     |    1    | Texto ordenado, apartado B, punto 1 (\*)                                             |
| 2, 21, 22, 23 |    2    | Sección 3, punto 3.1.1.4 (\*\*). El 23, por su ubicación en la tabla (**hipótesis**) |
|    3, 4, 5    | 3, 4, 5 | —                                                                                    |

1. Por deudor se elige la línea con **mayor nivel**.
2. En **empate de nivel** (por ejemplo 1 y 11) gana el **código más bajo**: el campo debe mostrar lo más preocupante, y 1 es una calificación real, mientras que 11 indica cobertura.
3. En `deudores.situacion_maxima` se guarda el **código del BCRA** de esa línea (por ejemplo 5 u 11), no su nivel.

| Deudor | Códigos en el archivo | `situacion_maxima` |
| ------ | :-------------------: | :----------------: |
| Juan   |         5 y 11        |         5          |
| Ana    |         1 y 11        |         1          |
| María  |         99 y 1        |         1          |
| Pedro  |        solo 11        |         11         |

### 2.2 Códigos no documentados

- **No frenan** la importación.
- Se informan en el **resumen final** de la importación (log y notificación), con la cantidad de líneas por código.
- La línea **suma su préstamo** igual.
- **No tienen nivel de riesgo**, así que no compiten con los códigos documentados. Ejemplo: un deudor con un código desconocido y un 2 queda con `situacion_maxima` = 2.
- Si un deudor tiene **solo** códigos no documentados, se guarda ese código tal cual. No se pierde ni el registro ni el dato.

---

## 3. Alternativas consideradas

| Alternativa                              | Por qué no                                                                 |
| ---------------------------------------- | -------------------------------------------------------------------------- |
| Máximo numérico (literal)                | Un deudor con 5 y 11 quedaría en 11 y parecería "cubierto"                 |
| Guardar el nivel (1–5) en vez del código | Se pierde el código original (11, 21…), que es el dato que publica el BCRA |

---

## 4. Consecuencias

**Lo que ganamos:**

- `situacion_maxima` refleja el riesgo real: un 11 nunca tapa a un 5.
- Se conserva el código original del BCRA, que es el dato que se publica.

**Lo que resignamos:**

- La comparación depende de una tabla de niveles (`NivelDeRiesgo`), que hay que actualizar si el BCRA agrega códigos. Mientras tanto, un código nuevo no frena la importación: se informa en el resumen.
- El nivel del 23 es una hipótesis, porque la norma no lo dice explícitamente.

---

## 5. Referencias

(\*) Banco Central de la República Argentina. (2024). *Régimen Informativo Contable Mensual: Deudores del Sistema Financiero* (Texto ordenado, versión 17a, Comunicación "A" 8001), apartado B, punto 1, p. 6. https://www.bcra.gob.ar/archivos/Pdfs/Texord/t-RI-DSF.pdf

> "Los deudores 'no clasificados' por estar las asistencias cubiertas en su totalidad con garantías preferidas 'A' se deberán informar cómo situación 'normal'."

(\*\*) Banco Central de la República Argentina. (s. f.). *Presentación de Informaciones al Banco Central: Sección 3. Deudores del Sistema Financiero* (Texto ordenado), punto 3.1.1.4, p. 2. https://www.bcra.gob.ar/archivos/Pdfs/Texord/t-SO-s03.pdf

> "21 En observación (2 a) / Riesgo bajo · 22 En negociación o con acuerdos de refinanciación (2 b) · 23 En tratamiento especial · 11 Con asistencias cubiertas en su totalidad con garantías preferidas 'A' · 99 Deudor no clasificado"

(\*\*\*) Banco Central de la República Argentina. (s. f.). *Presentación de Informaciones al Banco Central: Sección 3. Deudores del Sistema Financiero* (Texto ordenado), punto 3.1.2.2.ii), p. 5. https://www.bcra.gob.ar/archivos/Pdfs/Texord/t-SO-s03.pdf

> "Si no hubiera sido clasificado, se consignará '99'."
