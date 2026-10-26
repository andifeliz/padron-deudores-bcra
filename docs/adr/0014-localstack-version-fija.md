# ADR 0014 · LocalStack fijado en la versión 4.14.0

**En una frase:** se usa `localstack/localstack:4.14.0`, la última versión que funciona sin cuenta ni token, para que cualquiera pueda levantar el sistema con `docker compose up`.

**Estado:** Aceptada

**Qué vas a encontrar**
1. [Contexto](#1-contexto)
2. [Decisión](#2-decisión)
3. [Alternativas consideradas](#3-alternativas-consideradas)
4. [Consecuencias](#4-consecuencias)
5. [Referencias](#5-referencias)

---

## 1. Contexto

- El enunciado pide levantar todo con `docker-compose up` y valora el uso de LocalStack para simular S3 y SQS.
- Desde la versión 2026.03.0, LocalStack exige una cuenta y un token de autenticación para arrancar (\*). Sin token, el contenedor se cierra.
- Las versiones anteriores siguen pudiendo usarse sin autenticación (\*\*).

---

## 2. Decisión

- Se fija la imagen en **`localstack/localstack:4.14.0`**, solo con S3 y SQS.
- Así, quien clone el repo levanta el sistema sin registrarse en ningún servicio.

---

## 3. Alternativas consideradas

| Alternativa                                                | Por qué no                                                        |
| ---------------------------------------------------------- | ----------------------------------------------------------------- |
| Última versión de LocalStack con token                     | Cada evaluador tendría que crear una cuenta y configurar su token |
| Otros emuladores (por ejemplo, para S3 o SQS por separado) | Se pierde el uso de LocalStack, que es criterio de evaluación     |

---

## 4. Consecuencias

**Lo que ganamos:**

- Cualquiera levanta el sistema sin registrarse en ningún servicio.

**Lo que resignamos:**

- Esa versión no recibe actualizaciones. Para un entorno de desarrollo local, alcanza.
- LocalStack tiene límites de memoria con archivos de varios GB; S3 real no los tiene.

---

## 5. Referencias

(\*) LocalStack. (2026). *Announcing the LocalStack for AWS 2026.03.0 Release*. LocalStack Blog. https://blog.localstack.cloud/localstack-for-aws-release-2026-03-0/

(\*\*) LocalStack. (2026). *The Road Ahead for LocalStack: Upcoming Changes to the Delivery of Our AWS Cloud Emulators*. LocalStack Blog. https://blog.localstack.cloud/the-road-ahead-for-localstack/
