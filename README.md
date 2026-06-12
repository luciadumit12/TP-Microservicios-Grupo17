# TP-Microservicios-Grupo17
# E-Commerce System — Arquitectura de Microservicios (.NET 8)

## 1. Descripción General

Este proyecto implementa un sistema de E-Commerce basado en una arquitectura de microservicios, desarrollado con **C# y .NET 8** para la materia **Construcción de Aplicaciones Informáticas**.

La solución está compuesta por distintas **REST APIs independientes**, cada una con responsabilidades bien definidas, orientadas a gestionar productos, usuarios, órdenes, carritos y notificaciones. Según la implementación de cada servicio, se utiliza el patrón de **Minimal APIs** o **Controllers**.

El sistema integra lógica de negocio compleja y componentes transversales de nivel profesional, como manejo global de errores, logging estructurado, observabilidad, health checks, documentación con Swagger/OpenAPI y comunicación entre servicios.

---

## 2. Arquitectura del Sistema

El sistema sigue una topología de microservicios donde cada componente posee su propia base de datos, aplicando el enfoque **Database per Service**.

Los microservicios se comunican entre sí mediante llamadas HTTP directas para realizar validaciones críticas de negocio.

### Diagrama de Arquitectura

El diagrama de arquitectura se encuentra disponible en la documentación externa del proyecto:

[Ver documentación en Google Drive](https://drive.google.com/file/d/1bsc-O8ZqRWA7KSTwKya9eq1pzf718zYq/view?usp=drive_link)

---

## 3. Estructura de la Solución

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

### Microservicios incluidos

#### Products API

Responsable de la gestión del catálogo de productos y stock.

También participa en validaciones de stock y bloquea la eliminación de productos si existen órdenes activas en `Orders API`.

#### Users API

Responsable del registro, autenticación y gestión de usuarios.

Incluye una política de bloqueo luego de 3 intentos fallidos de login.

Además, dispara eventos de notificación hacia `Notifications API` luego de un registro exitoso.

#### Orders API

Responsable del procesamiento de órdenes de compra.

Valida stock contra `Products API` antes de confirmar una compra y gestiona transiciones de estado de las órdenes.

#### Cart API

Responsable de la gestión del carrito de compras por usuario.

Administra ítems persistentes por usuario y valida la existencia de productos en `Products API` y usuarios en `Users API`.

#### Notifications API

Responsable de la simulación de envío de notificaciones.

Contempla notificaciones por:

* Email
* SMS
* Push

---

## 4. Tecnologías y Componentes Utilizados

El proyecto implementa los siguientes pilares técnicos:

### Runtime

* .NET 8
* .NET 8.0 SDK o superior

### Persistencia

Cada microservicio posee su propia base de datos SQLite.

La persistencia se gestiona utilizando **Dapper** como micro-ORM, permitiendo un acceso eficiente a los datos.

Las bases de datos se inicializan automáticamente al primer arranque de cada servicio.

Ejemplos de archivos de base de datos:

* `products.db`
* `users.db`

### Documentación

Cada microservicio expone documentación interactiva mediante **Swagger / OpenAPI**, disponible en el endpoint `/swagger`.

Se utiliza **Swashbuckle** con comentarios XML y ejemplos de respuestas de éxito y error.

### Logging Estructurado

Se utiliza **Serilog** con doble destino:

* Consola, orientada principalmente a errores.
* Archivo JSON, utilizado para auditoría de requests.

### Comunicación entre Microservicios

Se utiliza **HttpClientFactory** para la comunicación entre servicios.

Ejemplo:

* `Orders API` consulta a `Products API` para validar stock antes de confirmar una compra.

---

## 5. Aspectos Transversales

### Manejo Global de Errores

Se implementa un sistema unificado de manejo de errores mediante `IExceptionHandler` de .NET 8.

Todas las respuestas de error, tanto 4xx como 5xx, siguen el estándar **Problem Details**.

Cada respuesta de error incluye:

* `errorCode`: código único del catálogo de errores.
* `errorMessage`: descripción legible del error de negocio.
* `instance`: ruta del endpoint que originó el error.

Ejemplos de códigos propios:

* `PRD-001`
* `ORD-005`, utilizado para casos de stock insuficiente.

Las capturas de Swagger muestran ejemplos de estas respuestas de error y se encuentran disponibles en la documentación externa del proyecto:

[Ver capturas en Google Drive](https://docs.google.com/document/d/1SySgl7b7wYgqREMab5SLqCl1qt3KAVAiNU3hg5uJvvc/edit?usp=drive_link)

---

### Observabilidad y Correlation ID

El sistema genera un `X-Correlation-Id` único por cada request HTTP.

Este identificador se propaga en todas las llamadas salientes entre microservicios y se incluye en todos los logs de Serilog.

Esto permite realizar la trazabilidad completa de una operación a través de los distintos servicios del sistema.

---

### Health Checks

Cada microservicio expone endpoints de monitoreo operativo para verificar el estado de la aplicación y sus dependencias.

Endpoints disponibles:

```
/health
/health/live
/health/ready
```

Detalle:

* `/health/live`: indica el estado del proceso.
* `/health/ready`: indica el estado de las dependencias, como la base de datos SQLite.

---

## 6. Flujo de Comunicación entre Servicios

El sistema opera mediante llamadas HTTP directas entre microservicios para resolver validaciones críticas.

Principales interacciones:

* `Cart API` valida la existencia de productos en `Products API`.
* `Cart API` valida usuarios en `Users API`.
* `Orders API` valida stock en `Products API` antes de confirmar una compra.
* `Users API` dispara eventos de notificación hacia `Notifications API` luego de un registro exitoso.
* `Products API` bloquea la eliminación de productos si existen órdenes activas en `Orders API`.

---

## 7. Guía de Ejecución

### Requisitos previos

Para ejecutar el proyecto es necesario contar con:

* .NET 8.0 SDK o superior.

### Pasos para iniciar los servicios

1. Clonar el repositorio.

2. Desde la raíz de la solución, restaurar las dependencias.

3. Ejecutar los servicios.

Se recomienda abrir una terminal por servicio o utilizar la opción **Multiple Startup Projects** en Visual Studio.

4. Acceder a la documentación interactiva de cada microservicio mediante Swagger.

---

## 8. URLs de Swagger

Cada servicio expone su documentación interactiva en `/swagger`.

URLs indicadas:

```text
Products: https://localhost:7268/swagger
Users: https://localhost:7075/swagger
Orders: https://localhost:7168/swagger
Cart: https://localhost:7168/swagger
Notifications: https://localhost:7185/swagger
```

---

## 9. Documentación

La documentación final del proyecto se encuentra disponible en Google Drive en la siguiente carpeta:

[Ver documentación del proyecto](https://drive.google.com/drive/u/0/folders/15alpDCkzEYtIvMggGUPe3p_fPFNtdHPU)

Incluye:

* Diagrama de arquitectura.
* Capturas de Swagger.
* Ejemplos de errores bajo el estándar Problem Details.

---

## 10. Resumen del Proyecto

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

Este proyecto fue desarrollado para la materia **Construcción de Aplicaciones Informáticas**.
