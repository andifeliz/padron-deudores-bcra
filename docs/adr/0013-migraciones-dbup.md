# ADR 0013 · Migraciones versionadas con DbUp

**En una frase:** las tablas se crean y modifican con scripts SQL numerados que DbUp aplica al levantar el sistema, registrando cuáles ya corrieron.

**Estado:** Aceptada

**Qué vas a encontrar**
1. [Contexto](#1-contexto)
2. [Decisión](#2-decisión)
3. [Alternativas consideradas](#3-alternativas-consideradas)
4. [Consecuencias](#4-consecuencias)
5. [Referencias](#5-referencias)

---

## 1. Contexto

- Los scripts de inicio de PostgreSQL (`/docker-entrypoint-initdb.d/`) corren **una sola vez**, cuando la base se crea desde cero. Una base existente no se entera de cambios posteriores.
- El acceso a datos es SQL explícito con Dapper ([ADR 0012](0012-acceso-a-datos-dapper.md)).

---

## 2. Decisión

| Qué              | Cómo                                                                                    |
| ---------------- | --------------------------------------------------------------------------------------- |
| Usuarios         | Script de inicio de PostgreSQL `docker/postgres/01-usuarios.sh`: requiere administrador |
| Tablas e índices | Migraciones con **DbUp** (\*) en el proyecto `PadronDeudores.Migraciones`               |

- Cada migración es un script SQL numerado (`0001-tablas-iniciales.sql`, `0002-...`), incluido en el ensamblado.
- DbUp aplica solo las que faltan, cada una en su transacción, y las registra en la tabla `schemaversions`.
- Corre como el usuario `importador`, dueño de las tablas.
- En Docker Compose es un servicio que se ejecuta una vez y termina; los servicios .NET arrancan recién cuando terminó bien.

---

## 3. Alternativas consideradas

| Alternativa                     | Por qué no                                                                                                                  |
| ------------------------------- | --------------------------------------------------------------------------------------------------------------------------- |
| Solo scripts de inicio          | Sin versionado: una base existente no recibe los cambios                                                                    |
| Migraciones de Entity Framework | Requieren usar EF; además, el table swap crea y renombra tablas en ejecución, lo que choca con el registro de esquema de EF |

---

## 4. Consecuencias

**Lo que ganamos:**

- Cualquier base, nueva o existente, llega al mismo esquema con solo levantar el sistema.
- Los cambios de la base quedan versionados en el repositorio, junto al código.

**Lo que resignamos:**

- No hay vuelta atrás automática: para deshacer un cambio se escribe una migración nueva.

---

## 5. Referencias

(\*) DbUp. (s. f.). *DbUp Documentation*. https://dbup.readthedocs.io/
