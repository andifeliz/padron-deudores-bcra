# ADR 0010 · Cada importación reemplaza los resúmenes

**En una frase:** al importar un archivo, `deudores` y `entidades` quedan exactamente iguales a lo que dice ese archivo: se arman de nuevo y reemplazan a las anteriores.

**Estado:** Aceptada

**Qué vas a encontrar**
1. [Contexto](#1-contexto)
2. [Decisión](#2-decisión)
3. [Alternativas consideradas](#3-alternativas-consideradas)
4. [Consecuencias](#4-consecuencias)
5. [Referencias](#5-referencias)

---

## 1. Contexto

- El enunciado pide: "Inserta o actualiza los datos en la base seleccionada".
- Cada archivo del BCRA es una **foto completa** del período, no una lista de cambios.
- Un deudor que canceló su deuda **deja de aparecer** en el archivo: se informan los deudores con saldo de deuda, salvo los de menos de \$ 25 mil (\*).

Ejemplo con un segundo archivo:

| Deudor | Noviembre | Diciembre    | Solo insertar o actualizar | Reemplazar |
| ------ | --------: | ------------ | -------------------------: | ---------: |
| Juan   |       150 | (no aparece) |                150 (viejo) |  (no está) |
| Ana    |        20 | 80           |                         80 |         80 |
| Pedro  |         — | 50           |                         50 |         50 |

---

## 2. Decisión

- Cada importación **reemplaza** los resúmenes: se arman tablas nuevas a partir del archivo y se intercambian por las actuales (*table swap*). Así quedan los deudores nuevos, los existentes con sus valores nuevos, y desaparecen los que ya no están.
- El reemplazo va en **la misma transacción** que el cálculo de los resúmenes: la API ve la foto anterior completa o la nueva completa, nunca una mezcla.
- El histórico queda en los **archivos originales en S3** y en la tabla **`importaciones`**. Como `deudores` y `entidades` son datos calculados, cualquier período se puede volver a obtener reprocesando su archivo.

---

## 3. Alternativas consideradas

| Alternativa                           | Por qué no                                                                                  |
| ------------------------------------- | ------------------------------------------------------------------------------------------- |
| Solo insertar o actualizar            | Quien canceló su deuda seguiría figurando con el monto del archivo anterior                 |
| Borrado lógico (marcar como inactivo) | Son datos calculados y recuperables; todas las consultas tendrían que filtrar los inactivos |
| Guardar una foto por período          | Agrega millones de filas por mes sin un caso de uso pedido                                  |

---

## 4. Consecuencias

**Lo que ganamos:**

- Los datos son siempre iguales al último archivo importado.
- Reimportar el mismo archivo deja el mismo resultado (idempotente).
- Quien consulta nunca ve una mezcla de datos viejos y nuevos.

**Lo que resignamos:**

- No hay histórico consultable por API: solo el último archivo.
- Cada importación recalcula todo, aunque haya cambiado una sola línea.

---

## 5. Referencias

(\*) Banco Central de la República Argentina. (2024). *Régimen Informativo Contable Mensual: Deudores del Sistema Financiero* (Texto ordenado, versión 17a, Comunicación "A" 8001), punto 2, p. 1. https://www.bcra.gob.ar/archivos/Pdfs/Texord/t-RI-DSF.pdf

> "Se informará la totalidad de los deudores de las entidades financieras y otros entes mencionados en el último párrafo del punto precedente, con excepción del B.C.R.A. y aquellos deudores cuyo saldo de deuda sea inferior a \$25 mil."
