# ADR 0009 · Montos en miles de pesos, como vienen

**En una frase:** los montos se guardan y se devuelven en miles de pesos con un decimal, igual que en el archivo del BCRA.

**Estado:** Aceptada

**Qué vas a encontrar**
1. [Contexto](#1-contexto)
2. [Decisión](#2-decisión)
3. [Alternativas consideradas](#3-alternativas-consideradas)
4. [Consecuencias](#4-consecuencias)
5. [Referencias](#5-referencias)

---

## 1. Contexto

El archivo informa los importes en miles de pesos con un decimal (\*): `2406,0` significa \$ 2.406.000. La resolución real es de \$ 100.

---

## 2. Decisión

- Se guardan **en miles**, tal cual vienen, como `numeric` con un decimal.
- La API los devuelve también en miles.
- El README y la documentación de la API aclaran la unidad.

---

## 3. Alternativas consideradas

| Alternativa                              | Por qué no                                                                                    |
| ---------------------------------------- | --------------------------------------------------------------------------------------------- |
| Convertir a pesos (multiplicar por 1000) | Cambia el dato de la fuente, y para compararlo con el archivo habría que volver a convertirlo |

---

## 4. Consecuencias

**Lo que ganamos:**

- Los datos son idénticos a la fuente, así que se verifican directo contra el archivo.

**Lo que resignamos:**

- Quien usa la API tiene que saber la unidad: por eso está documentada, y el panel web muestra los montos ya convertidos a pesos.

---

## 5. Referencias

(\*) Banco Central de la República Argentina. (2024). *Central de Deudores del Sistema Financiero: LEAME* [Documento incluido en el archivo 202311DEUDORES_20240122204012], p. 1.

> "Los importes están expresados en miles de pesos con un decimal."
