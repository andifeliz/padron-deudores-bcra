# ADR 0011 · Top N: límite máximo y desempate por identificación

**En una frase:** `GET /deudores/top/{n}` acepta N entre 1 y un máximo configurable (1000 por defecto), y si dos deudores deben lo mismo, va primero el de identificación más baja.

**Estado:** Aceptada

**Qué vas a encontrar**
1. [Contexto](#1-contexto)
2. [Decisión](#2-decisión)
3. [Alternativas consideradas](#3-alternativas-consideradas)
4. [Consecuencias](#4-consecuencias)

---

## 1. Contexto

El enunciado pide `GET /deudores/top/{n}`, que "devuelve los N deudores con mayor suma de préstamos". No define:

- **Un límite para N:** con 19,9 M de deudores, `top/20000000` devolvería una respuesta gigante.
- **Qué pasa en un empate:** si el último lugar lo comparten dos deudores con la misma suma, la base puede devolver uno distinto en cada llamada.

| Deudor | Suma | `top/3` sin desempate | `top/3` con desempate    |
| ------ | ---: | --------------------- | ------------------------ |
| Ana    |  500 | ✅                     | ✅                        |
| Juan   |  400 | ✅                     | ✅                        |
| Pedro  |  300 | A veces               | ✅ (identificación menor) |
| Lucía  |  300 | A veces               | —                        |

---

## 2. Decisión

- **N válido:** entre 1 y `TOP_MAXIMO`, configurable en `.env` (1000 por defecto). Fuera de ese rango, la API responde **400 Bad Request** con un mensaje que indica el rango permitido.
- **Orden:** por `suma_total_prestamos` de mayor a menor y, en empate, por `nro_identificacion` de menor a mayor. Así el resultado es siempre el mismo para los mismos datos.
- El criterio de desempate se documenta en el README.
- **Filtro opcional por una situación:** `?situacion=5`. Es por **código exacto**: `situacion=1` no incluye el 11, y `situacion=2` no incluye 21, 22 ni 23. Un código que no existe responde **400**; uno válido sin deudores, una lista vacía.
- **Rendimiento del filtro:** un índice por (situación, suma). Medido con el archivo real: menos de 15 ms para cualquier situación (sin el índice, hasta 3 s).

---

## 3. Alternativas consideradas

| Alternativa                                   | Por qué no                                                                                        |
| --------------------------------------------- | ------------------------------------------------------------------------------------------------- |
| Sin límite para N                             | Una sola llamada podría pedir los 19,9 M de deudores y trabar la API y la base                    |
| Paginación (`?pagina=`)                       | El enunciado pide "los N" en una ruta; paginar agrega parámetros sin un caso de uso               |
| Filtrar por varias situaciones a la vez       | No hay un caso de uso pedido; una situación por código exacto es más simple de usar y de explicar |
| Filtrar por nivel de riesgo (1 incluye al 11) | El BCRA publica códigos, no niveles: el filtro devuelve exactamente el código que se pide         |

---

## 4. Consecuencias

**Lo que ganamos:**

- Respuestas acotadas, siempre en el mismo orden y rápidas, gracias a los índices.

**Lo que resignamos:**

- Para pedir más de 1000 deudores hay que cambiar `TOP_MAXIMO`.
- Cada importación arma un índice más.
