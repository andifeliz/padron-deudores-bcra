# ADR 0003 · Se incluyen los deudores del exterior (tipos 98 y 99)

**En una frase:** los deudores identificados con tipo 98 o 99 se procesan como cualquier otro, y la identificación se trata siempre como texto de 11 caracteres.

**Estado:** Aceptada

**Qué vas a encontrar**
1. [Contexto](#1-contexto)
2. [Decisión](#2-decisión)
3. [Alternativas consideradas](#3-alternativas-consideradas)
4. [Consecuencias](#4-consecuencias)
5. [Referencias](#5-referencias)

---

## 1. Contexto

El enunciado define `nro_identificacion` como "Campo 4 - CUIT/CUIL". Pero el campo 3 (tipo de identificación) trae tres valores:

| Tipo | Significado                                    |     Líneas | Ejemplo de identificación |
| :--: | ---------------------------------------------- | ---------: | :-----------------------: |
|  11  | CUIT, CUIL o CDI                               | 34.385.282 |        20000000001        |
|  98  | Persona jurídica del exterior sin clave fiscal |        150 |        00007CN0001        |
|  99  | Persona física del exterior sin clave fiscal   |        135 |        00191CL0000        |

En los tipos 98 y 99, la identificación se arma con el código de la entidad que informa (5 dígitos), el código de país SWIFT (2 letras) y un número único (4 dígitos) (\*).

---

## 2. Decisión

- Los deudores con tipo 98 y 99 se **incluyen** en los resúmenes de deudores y de entidades.
- `nro_identificacion` se guarda como **texto de 11 caracteres**, nunca como número.
- `GET /deudores/{cuit}` acepta cualquier identificación de 11 caracteres, con o sin letras.

---

## 3. Alternativas consideradas

| Alternativa                           | Por qué no                                                                                        |
| ------------------------------------- | ------------------------------------------------------------------------------------------------- |
| Excluir los tipos 98 y 99             | Desaparecerían 285 líneas de deuda real, y los totales por entidad no coincidirían con el archivo |
| Guardar la identificación como número | Las de tipo 98 y 99 tienen letras, y se perderían los ceros a la izquierda (`00007CN0001`)        |

---

## 4. Consecuencias

**Lo que ganamos:**

- Los totales por entidad coinciden con el archivo: no se descarta ninguna deuda.
- Una sola regla para todas las identificaciones: texto de 11 caracteres.

**Lo que resignamos:**

- El endpoint se llama `/deudores/{cuit}`, pero también acepta identificaciones que no son CUIT.

---

## 5. Referencias

(\*) Banco Central de la República Argentina. (s. f.). *Presentación de Informaciones al Banco Central: Sección 3. Deudores del Sistema Financiero* (Texto ordenado), punto 3.1.1.5, pp. 2-3. https://www.bcra.gob.ar/archivos/Pdfs/Texord/t-SO-s03.pdf

> "Se utilizarán los tipos de identificación '98' y '99' para los deudores residentes en el exterior que sean personas jurídicas y físicas respectivamente, que no cuenten con la clave de identificación fiscal."
