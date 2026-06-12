﻿# TP-Microservicios-Grupo17
# E-Commerce System — Arquitectura de Microservicios (.NET 8)

Sistema de E-Commerce desarrollado con **C# y .NET 8**, basado en una arquitectura de microservicios.
El proyecto fue realizado para la materia **Construcción de Aplicaciones Informáticas**.

La solución está compuesta por distintas **REST APIs independientes**, cada una con una responsabilidad específica dentro del sistema: productos, usuarios, órdenes, carritos y notificaciones.

---

## Integrantes

| Integrante                      | Servicio/s desarrollado/s |
| ------------                    | ------------------------- |
| Lucia Dumit                     | Products.API              |
| Lourdes Sofia Figueredo         | Users.API                 |
| Katia Nicole Hellwag            | Orders.API                |
| Lucia Dumit                     | Cart.API                  |
| Katia & Sofia                   | Notifications.API         |

---

## 1. Descripción General

Este proyecto implementa un sistema de E-Commerce basado en una arquitectura de microservicios, desarrollado con **C# y .NET 8**.

Cada microservicio funciona como una API REST independiente y posee responsabilidades bien definidas. Según la implementación de cada servicio, se utiliza el patrón de **Minimal APIs** o **Controllers**.

El sistema integra lógica de negocio y componentes transversales orientados a mejorar la calidad, trazabilidad y mantenibilidad de la solución, tales como:

* Manejo global de errores.
* Logging estructurado.
* Correlation ID.
* Health Checks.
* Documentación interactiva con Swagger/OpenAPI.
* Comunicación HTTP entre microservicios.
* Persistencia independiente por servicio.

---

## 2. Arquitectura del Sistema

El sistema sigue una topología de microservicios, donde cada componente representa una unidad funcional independiente.

Cada API posee su propia base de datos SQLite, aplicando el enfoque **Database per Service**. Esto permite que cada servicio administre sus propios datos sin depender directamente de las tablas internas de otros microservicios.

La comunicación entre servicios se realiza mediante llamadas HTTP directas, principalmente para resolver validaciones de negocio que requieren información de otro dominio.

Por ejemplo:

* `Cart API` valida productos contra `Products API`.
* `Orders API` valida stock contra `Products API`.
* `Users API` se comunica con `Notifications API` para disparar notificaciones luego del registro.
* `Products API` valida contra `Orders API` antes de permitir ciertas operaciones sobre productos.

---

## 3. Diagrama de Arquitectura

El diagrama de arquitectura se encuentra disponible en la documentación externa del proyecto:

[Ver documentación en Google Drive](https://drive.google.com/drive/folders/15alpDCkzEYtIvMggGUPe3p_fPFNtdHPU?usp=sharing)

---

## 4. Estructura de la Solución

La solución está organizada de la siguiente manera:

```text
TP-Microservicios-Grupo17/
├── .github/
├── docs/
├── ECommerce-Microservicios/
│   ├── Cart.API/
│   ├── Notifications.API/
│   ├── Orders.API/
│   ├── Products.API/
│   ├── Users.API/
│   └── ECommerce-Microservicios.slnx
├── .gitattributes
├── .gitignore
└── README.md
```

### Descripción de carpetas principales

* `.github/`: contiene configuraciones relacionadas con GitHub.
* `docs/`: carpeta destinada a documentación, capturas y recursos complementarios del proyecto.
* `ECommerce-Microservicios/`: carpeta principal de la solución.
* `Cart.API/`: microservicio encargado del carrito de compras.
* `Notifications.API/`: microservicio encargado de las notificaciones.
* `Orders.API/`: microservicio encargado de las órdenes de compra.
* `Products.API/`: microservicio encargado del catálogo de productos y stock.
* `Users.API/`: microservicio encargado de usuarios, registro y autenticación.
* `ECommerce-Microservicios.slnx`: archivo de solución del proyecto.
* `README.md`: guía principal del proyecto.

---

## 5. Microservicios Incluidos

### Products API

Microservicio responsable de la gestión del catálogo de productos y el stock disponible.

Entre sus responsabilidades se encuentran:

* Administrar productos del catálogo.
* Gestionar información asociada al stock.
* Participar en validaciones de stock solicitadas por otros servicios.
* Bloquear la eliminación de productos si existen órdenes activas en `Orders API`.

---

### Users API

Microservicio responsable del registro, autenticación y gestión de usuarios.

Entre sus responsabilidades se encuentran:

* Registrar nuevos usuarios.
* Gestionar el inicio de sesión.
* Aplicar una política de bloqueo luego de 3 intentos fallidos de login.
* Disparar eventos de notificación hacia `Notifications API` luego de un registro exitoso.

---

### Orders API

Microservicio responsable del procesamiento de órdenes de compra.

Entre sus responsabilidades se encuentran:

* Crear y gestionar órdenes.
* Validar stock contra `Products API` antes de confirmar una compra.
* Gestionar transiciones de estado de las órdenes.
* Mantener la información propia de las compras realizadas.

---

### Cart API

Microservicio responsable de la gestión del carrito de compras por usuario.

Entre sus responsabilidades se encuentran:

* Administrar ítems persistentes por usuario.
* Agregar, modificar o quitar productos del carrito.
* Validar la existencia de productos en `Products API`.
* Validar usuarios en `Users API`.

---

### Notifications API

Microservicio responsable de la simulación del envío de notificaciones.

Contempla distintos canales de notificación:

* Email.
* SMS.
* Push.

Su objetivo es centralizar la lógica asociada al envío o registro de alertas generadas por otros servicios del sistema.

---

## 6. Tecnologías Utilizadas

El proyecto utiliza las siguientes tecnologías y componentes:

### Lenguaje y Framework

* C#.
* .NET 8.
* ASP.NET Core.
* REST APIs.

### Persistencia

* SQLite como motor de base de datos.
* Dapper como micro-ORM para el acceso a datos.

### Documentación

* Swagger.
* OpenAPI.
* Swashbuckle.
* Comentarios XML y ejemplos de respuestas.

### Observabilidad y Logging

* Serilog.
* Logs en consola.
* Logs en archivo JSON.
* Correlation ID para trazabilidad entre servicios.

### Comunicación entre Servicios

* HttpClientFactory.
* Llamadas HTTP entre microservicios.

### Monitoreo

* Health Checks.
* Endpoints `/health`, `/health/live` y `/health/ready`.

---

## 7. Persistencia y Bases de Datos

Cada microservicio posee su propia base de datos SQLite.

Este enfoque permite mantener el aislamiento de datos entre servicios y evita que una API dependa directamente de las tablas internas de otra.

Las bases de datos se inicializan automáticamente al primer arranque de cada servicio.

Ejemplos de archivos de base de datos:

* `products.db`
* `users.db`

---

## 8. Aspectos Transversales

### Manejo Global de Errores

Se implementa un sistema unificado de manejo de errores mediante `IExceptionHandler` de .NET 8.

Todas las respuestas de error, tanto 4xx como 5xx, siguen el estándar **Problem Details**.

Cada respuesta de error incluye:

* `errorCode`: código único del catálogo de errores.
* `errorMessage`: descripción legible del error de negocio.
* `instance`: ruta del endpoint que originó el error.

Ejemplos de códigos propios utilizados en el proyecto:

* `PRD-001`.
* `ORD-005`, utilizado para casos de stock insuficiente.

Las capturas de Swagger muestran ejemplos de estas respuestas de error y se encuentran disponibles en la documentación externa del proyecto:

[Ver capturas en Google Drive](https://drive.google.com/drive/u/0/folders/15alpDCkzEYtIvMggGUPe3p_fPFNtdHPU)

---

### Observabilidad y Correlation ID

El sistema genera un `X-Correlation-Id` único por cada request HTTP.

Este identificador se propaga en las llamadas salientes entre microservicios y se incluye en los logs generados por Serilog.

Esto permite realizar la trazabilidad completa de una operación a través de los distintos servicios del sistema.

Esta funcionalidad es especialmente útil para analizar errores o seguir el recorrido de una solicitud cuando intervienen varias APIs.

---

### Logging Estructurado

Se utiliza **Serilog** como herramienta de logging estructurado.

El proyecto contempla doble destino para los logs:

* Consola, orientada principalmente a errores y seguimiento durante la ejecución.
* Archivo JSON, utilizado para auditoría de requests.

El uso de logs estructurados permite consultar información de forma más clara y ordenada, facilitando el análisis de incidentes y el seguimiento de operaciones.

---

### Health Checks

Cada microservicio expone endpoints de monitoreo operativo para verificar el estado de la aplicación y sus dependencias.

Endpoints disponibles:

```text
/health
/health/live
/health/ready
```

Detalle:

* `/health`: informa el estado general del servicio.
* `/health/live`: indica si el proceso se encuentra activo.
* `/health/ready`: indica si las dependencias del servicio están disponibles, como la base de datos SQLite.

---

## 9. Comunicación entre Microservicios

Los microservicios se comunican mediante llamadas HTTP directas.

Para esta comunicación se utiliza **HttpClientFactory**, lo que permite centralizar y administrar de forma más ordenada los clientes HTTP utilizados por las APIs.

Principales interacciones:

* `Cart API` valida la existencia de productos en `Products API`.
* `Cart API` valida usuarios en `Users API`.
* `Orders API` valida stock en `Products API` antes de confirmar una compra.
* `Users API` dispara eventos de notificación hacia `Notifications API` luego de un registro exitoso.
* `Products API` bloquea la eliminación de productos si existen órdenes activas en `Orders API`.

Debido a estas dependencias, para probar correctamente los flujos completos del sistema se recomienda ejecutar todos los microservicios al mismo tiempo.

---

## 10. Requisitos Previos

Para ejecutar el proyecto es necesario contar con:

* .NET 8.0 SDK o superior.
* Git.
* Visual Studio 2022 o una terminal compatible con .NET CLI.

---

## 11. Cómo Ejecutar el Proyecto

### 1. Clonar el repositorio

```bash
git clone URL_DEL_REPOSITORIO
cd TP-Microservicios-Grupo17
```

### 2. Ingresar a la carpeta de la solución

```bash
cd ECommerce-Microservicios
```

### 3. Restaurar dependencias

```bash
dotnet restore
```

### 4. Ejecutar los servicios

Cada microservicio puede ejecutarse desde una terminal independiente.

#### Products API

```bash
cd Products.API
dotnet run
```

#### Users API

```bash
cd Users.API
dotnet run
```

#### Orders API

```bash
cd Orders.API
dotnet run
```

#### Cart API

```bash
cd Cart.API
dotnet run
```

#### Notifications API

```bash
cd Notifications.API
dotnet run
```

También puede utilizarse la opción **Multiple Startup Projects** desde Visual Studio para levantar varios servicios al mismo tiempo.

---

## 12. Acceso a Swagger

Cada servicio expone su documentación interactiva en el endpoint `/swagger`.

URLs indicadas:

```text
Products: https://localhost:7268/swagger
Users: https://localhost:7075/swagger
Orders: https://localhost:7168/swagger
Cart: https://localhost:7199/swagger
Notifications: https://localhost:7185/swagger
```

Los puertos pueden revisarse o modificarse desde el archivo:

```text
Properties/launchSettings.json
```

dentro de cada microservicio.

---

## 13. Acceso a Health Checks

Una vez iniciado un microservicio, se pueden consultar sus endpoints de monitoreo desde el navegador o una herramienta como Postman.

Endpoints disponibles:

```text
https://localhost:[PUERTO]/health
https://localhost:[PUERTO]/health/live
https://localhost:[PUERTO]/health/ready
```

Estos endpoints permiten verificar si el servicio está activo y si sus dependencias están listas para operar.

---

## 14. Recomendaciones para Pruebas

Para probar operaciones simples de cada API, puede ejecutarse únicamente el microservicio correspondiente.

Sin embargo, para probar flujos que requieren validaciones cruzadas entre servicios, se recomienda levantar todas las APIs en simultáneo.

Ejemplos:

* Para validar operaciones del carrito, se recomienda tener activo `Cart API`, `Products API` y `Users API`.
* Para crear órdenes, se recomienda tener activo `Orders API`, `Products API` y `Users API`.
* Para validar notificaciones luego del registro, se recomienda tener activo `Users API` y `Notifications API`.
* Para validar restricciones relacionadas con productos y órdenes, se recomienda tener activo `Products API` y `Orders API`.

---

## 15. Documentación del Proyecto

La documentación adicional del proyecto se encuentra disponible en Google Drive:

[Ver documentación del proyecto](https://drive.google.com/drive/u/0/folders/15alpDCkzEYtIvMggGUPe3p_fPFNtdHPU)

Incluye:

* Diagrama de arquitectura.
* Capturas de Swagger.
* Ejemplos de errores bajo el estándar Problem Details.

---

## 16. Resumen del Proyecto

Este sistema de E-Commerce implementa una arquitectura de microservicios con servicios independientes para productos, usuarios, órdenes, carrito y notificaciones.

Cada microservicio posee su propia base de datos SQLite y expone endpoints REST documentados con Swagger/OpenAPI.

Además, el proyecto incorpora componentes transversales como:

* Manejo global de errores con `IExceptionHandler`.
* Respuestas estandarizadas con Problem Details.
* Códigos propios de error.
* Logging estructurado con Serilog.
* Correlation ID para trazabilidad.
* Health Checks para monitoreo operativo.
* Comunicación HTTP entre microservicios mediante HttpClientFactory.

El proyecto fue desarrollado para la materia **Construcción de Aplicaciones Informáticas**.
