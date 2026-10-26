# ADR 0008 · Notificación: webhook y log estructurado

**En una frase:** al terminar, el worker escribe un log estructurado con el resumen y hace un POST a un webhook configurable.

**Estado:** Aceptada

**Qué vas a encontrar**
1. [Contexto](#1-contexto)
2. [Decisión](#2-decisión)
3. [Alternativas consideradas](#3-alternativas-consideradas)
4. [Consecuencias](#4-consecuencias)

---

## 1. Contexto

El enunciado pide, al finalizar, "un correo electrónico o un webhook POST", y como alternativa opcional "un log estructurado indicando tiempo total y resumen de carga".

---

## 2. Decisión

- **Log estructurado, siempre:** con la duración y el resumen de la carga.
- **Webhook POST:** a una URL configurada en `.env` (`WEBHOOK_URL`). Si la URL está vacía, el resultado queda solo en el log.
- **Cuándo se avisa:** al completar, o al fallar **el último intento** (`SQS_MAX_INTENTOS`, la misma variable con la que se crea la cola). Un intento intermedio fallido solo se loguea, porque el siguiente puede salir bien.
- **Reintentos y timeout:** con `AddStandardResilienceHandler` de Microsoft: 3 reintentos con espera creciente y un límite de tiempo por intento.
- **Si el webhook no responde:** se registra un error y la importación sigue `completada`, porque los datos ya están guardados.

El cuerpo del POST:

```json
{
  "importacionId": "01a1759b-8633-7ec6-af7e-6a99c36a8f47",
  "estado": "completada",
  "archivo": "/data/deudores.txt",
  "resumen": {
    "importacionId": "01a1759b-8633-7ec6-af7e-6a99c36a8f47",
    "lineasLeidas": 34385567,
    "lineasInvalidas": 0,
    "deudores": 19926484,
    "entidades": 485,
    "lineasPorPeriodo": { "202311": 33799893, "202310": 514095, "202309": 71579 },
    "codigosSituacionDesconocidos": {},
    "duracionSegundos": 189.7
  },
  "error": null
}
```

Si falla, `estado` es `"fallida"`, `resumen` es `null` y `error` trae el motivo.

---

## 3. Alternativas consideradas

| Alternativa | Por qué no                                                                                                                         |
| ----------- | ---------------------------------------------------------------------------------------------------------------------------------- |
| Email       | Necesita un servidor de correo y credenciales; el webhook se integra con cualquier sistema (otra app, Slack) y se prueba más fácil |
| Solo log    | Nadie se entera de que terminó si no está mirando los logs                                                                         |

---

## 4. Consecuencias

**Lo que ganamos:**

- Cualquier sistema puede enterarse del resultado, sin consultar la API.
- El log queda siempre, aunque el webhook falle.

**Lo que resignamos:**

- El webhook no va firmado: quien lo recibe no puede verificar que venga de este sistema.
- Si el destino está caído más tiempo que los reintentos, ese aviso se pierde (el estado sigue disponible en la API).
