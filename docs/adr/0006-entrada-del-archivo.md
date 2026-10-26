# ADR 0006 · Entrada del archivo: ruta local y `/upload` en streaming

**En una frase:** el archivo puede llegar por una ruta local o subiéndolo a `/upload`; lo subido se guarda en S3 (LocalStack) sin cargarlo entero en memoria.

**Estado:** Aceptada

**Qué vas a encontrar**
1. [Contexto](#1-contexto)
2. [Decisión](#2-decisión)
3. [Alternativas consideradas](#3-alternativas-consideradas)
4. [Consecuencias](#4-consecuencias)
5. [Referencias](#5-referencias)

---

## 1. Contexto

- El enunciado pide "exponer un endpoint `/upload` o aceptar la ruta del archivo localmente", y como opcional, guardar el original en S3.
- El archivo pesa 5,9 GB. Kestrel rechaza por defecto todo lo que supere 30 MB (\*), y S3 admite como máximo 5 GiB en una sola subida (\*\*); el archivo son 5,54 GiB.

---

## 2. Decisión

| Entrada        | Cómo funciona                                                                                                                                                               |
| -------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **Ruta local** | La carpeta `data/` del proyecto se comparte con los contenedores como `/data`, en solo lectura. Se indica **solo el nombre** del archivo. Es la opción para el archivo real |
| **`/upload`**  | Se lee el body en streaming, sin `IFormFile`, y se sube a S3 con multipart, en partes de 8 MiB (S3 pide al menos 5 MiB) (\*\*)                                              |

- El límite de tamaño se sube solo en `/upload` (`IHttpMaxRequestBodySizeFeature`), con un tope configurable en `.env` (`UPLOAD_MAXIMO_MIB`, 8 GiB por defecto).
- **Seguridad:** la API rechaza nombres con carpetas (`../`), y el worker solo abre archivos de `/data` o del bucket del proyecto. Así nadie puede pedir que se lea otro archivo del servidor.
- Las dos entradas terminan en el mismo procesamiento ([ADR 0005](0005-procesamiento-streaming-copy-staging.md)).

---

## 3. Alternativas consideradas

| Alternativa                         | Por qué no                                                                                                                |
| ----------------------------------- | ------------------------------------------------------------------------------------------------------------------------- |
| Solo ruta local                     | Obliga a tener acceso a la carpeta del servidor, y no cumple el opcional de guardar el original en S3                     |
| Solo `/upload`                      | El archivo real (5,9 GB) se copiaría entero en cada importación; en LocalStack, además, puede quedarse sin memoria        |
| Formulario con `IFormFile`          | ASP.NET Core guarda el archivo entero en un temporal antes de entregarlo: 5,9 GB más en disco y el doble de tiempo        |
| Aceptar cualquier ruta del servidor | Cualquiera podría pedir que se lea un archivo ajeno (por ejemplo `/etc/passwd`): es la vulnerabilidad de *path traversal* |

---

## 4. Consecuencias

**Lo que ganamos:**

- La subida usa memoria constante (una parte de 8 MiB), sin importar el tamaño del archivo.
- Las dos entradas terminan en el mismo procesamiento: no hay código duplicado.

**Lo que resignamos:**

- La subida no es en segundo plano: el cliente espera a que el archivo termine de subir.
- Los archivos subidos quedan en S3 sin fecha de vencimiento.

---

## 5. Referencias

(\*) Microsoft. (s. f.). *Configure options for the ASP.NET Core Kestrel web server: Maximum request body size*. Microsoft Learn. https://learn.microsoft.com/en-us/aspnet/core/fundamentals/servers/kestrel/options

(\*\*) Amazon Web Services. (s. f.). *Amazon S3 multipart upload limits*. Amazon S3 User Guide. https://docs.aws.amazon.com/AmazonS3/latest/userguide/qfacts.html
