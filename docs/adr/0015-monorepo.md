# ADR 0015 · Un solo repositorio para todos los servicios (monorepo)

**En una frase:** los tres servicios, el panel web, las librerías compartidas, las migraciones, los tests y el entorno Docker viven en un mismo repositorio.

**Estado:** Aceptada

**Qué vas a encontrar**
1. [Contexto](#1-contexto)
2. [Decisión](#2-decisión)
3. [Alternativas consideradas](#3-alternativas-consideradas)
4. [Consecuencias](#4-consecuencias)

---

## 1. Contexto

- La solución tiene tres servicios .NET (importador-api, importador-worker y consulta-api; ver [arquitectura](../arquitectura.md)), que comparten las librerías `Dominio` e `Infraestructura`, y un panel web ([ADR 0016](0016-panel-web.md)).
- Un microservicio es independiente porque **corre y se despliega por separado** (su propia imagen y su propio contenedor), no por dónde vive su código.
- El challenge se entrega como un único proyecto que se levanta con `docker-compose up`.

---

## 2. Decisión

- **Un solo repositorio** con todos los proyectos `.NET`, el panel web (`frontend/`), los tests, las migraciones y el `docker-compose.yml`.
- Cada servicio tiene su propio Dockerfile e imagen: sigue siendo independiente en ejecución y despliegue.
- El código compartido se usa como referencia entre proyectos, sin publicarlo como paquete.

---

## 3. Alternativas consideradas

| Alternativa                 | Por qué no                                                                                                                                                                                                                                   |
| --------------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Un repositorio por servicio | Habría que publicar `Dominio` e `Infraestructura` como paquetes NuGet con versiones, coordinar cambios entre repos y levantar el entorno desde varios lugares. Se justifica con equipos distintos dueños de cada servicio, que no es el caso |

---

## 4. Consecuencias

**Lo que ganamos:**

- Un solo `git clone` y un solo `docker compose up` levantan todo.
- Un cambio que toca varios servicios va en un solo commit.

**Lo que resignamos:**

- Cualquier cambio dispara la compilación de todo el repositorio.
- Los permisos son por repositorio, no por servicio.
