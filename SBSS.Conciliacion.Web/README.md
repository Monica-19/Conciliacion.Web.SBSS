# Gestión de Conciliación SBSS

Aplicación web ASP.NET Core 10.0 MVC para la gestión de expedientes de
conciliación extrajudicial del Centro de Conciliación Extrajudicial SBSS
(Ley N.° 26872 y D.S. N.° 004-2005-JUS).

## Arquitectura

- **Acceso a datos**: Dapper puro sobre SQL Server. No hay Entity Framework:
  cada repositorio en `/Repositories` llama directamente a un procedimiento
  almacenado de `BD-SBSS-SP.sql` (`CommandType.StoredProcedure`), sin SQL
  embebido en la aplicación.
- **Autenticación**: cookie de ASP.NET Core (`AddCookie`), validando contra
  `sp_Usuario_ObtenerParaLogin` + `BCrypt.Net` para el hash de contraseña.
- **Tiempo real**: `Hubs/NotificacionesHub.cs` (SignalR) + un
  `BackgroundService` (`AlertaVencimientoBackgroundService`) que revisa
  vencimientos periódicamente y llama a `sp_Notificacion_GenerarAlertasVencimiento`.
- **Documentos**: `.docx` con Open XML SDK (`DocumentoWordService`), Excel con
  ClosedXML y PDF con QuestPDF — todo sin depender de Word/Office instalado
  en el servidor.
- **Frontend**: `_Layout.cshtml` + `site.css` reutilizan la paleta y
  tipografía del manual de marca del Centro (`#0c3855`, `#e2e5e4`, `#1d1d1b`,
  Poppins/Lora) y el logo real extraído de los PDF de marca, en
  `wwwroot/images`.

## Puesta en marcha

1. Ejecutar en SQL Server, en este orden:
   - `Database/BD-SBSS-Completa.sql` (esquema completo)
   - `Database/BD-SBSS-SP.sql` (procedimientos almacenados)
   - `Database/BD-SBSS-Seed.sql` (datos de prueba — sede, trabajadores y usuarios)
2. Ajustar la cadena de conexión en `appsettings.Development.json`
   (`ConnectionStrings:DefaultConnection`).
3. `dotnet restore` y `dotnet run`, o abrir la solución en Visual Studio
   2026 (requiere el SDK de .NET 10) y F5.

### Credenciales de prueba (creadas por `BD-SBSS-Seed.sql`)

| Usuario        | Contraseña           | Rol            |
|----------------|-----------------------|----------------|
| `admin`        | `Conciliacion2026!`   | ADMINISTRADOR  |
| `conciliador1` | `Conciliacion2026!`   | CONCILIADOR    |

Las contraseñas están guardadas como hash BCrypt (factor de costo 11) en la
tabla `Usuario`, no en texto plano — el script `BD-SBSS-Seed.sql` ya trae el
hash generado, no hace falta calcularlo a mano. `conciliador1` sirve para
probar de punta a punta la asignación de expedientes y la programación de
audiencias: ese trabajador es quien recibe la notificación real (por
SignalR) cuando lo asignas desde la cuenta `admin`.

Para crear más usuarios manualmente más adelante (antes de tener el CRUD de
Trabajadores), el hash se genera con `BCrypt.Net.BCrypt.HashPassword("tu-clave")`.

## Estructura

```
Controllers/     HomeController, AccountController, ExpedientesController,
                 AudienciasController, DashboardController, NotificacionesController,
                 SedesController, TrabajadoresController, MateriasController,
                 PersonasController, CatalogosController (landing)
Data/            Fábrica de conexiones (IDbConnectionFactory / SqlConnectionFactory)
Hubs/            NotificacionesHub (SignalR)
Models/          Entidades + ViewModels
Repositories/    Un repositorio por módulo, cada método = un SP
Services/        Generación de .docx / Excel / PDF + job de alertas de vencimiento
Views/           Razor views, con _Layout.cshtml de marca
wwwroot/         css/site.css, js/site.js, images/ (logos y fotos de marca)
```

### Catálogos (Sedes, Trabajadores, Materias, Personas)

CRUD completo para los datos maestros que usa el resto del sistema:
- **Sedes**: alta, edición, activar/desactivar.
- **Trabajadores**: alta, edición, activar/desactivar, y "Crear acceso"
  (da de alta el `Usuario` de un trabajador ya registrado, con su
  contraseña hasheada en BCrypt desde el propio controlador).
- **Materias**: se organizan en dos niveles — primero un `TipoMateria`
  ("Civil y Comercial", "Familia"...) y luego las `Materia` dentro de
  cada tipo.
- **Personas**: naturales y jurídicas, con búsqueda por nombre/razón
  social/documento, y sirven para vincularse a un expediente.

Los combos de `Expedientes/Create` y `Audiencias/Programar` (sede,
trabajador, conciliador, tipo de documento) ahora salen de estos
catálogos reales en vez de pedir un ID a mano. En `Expedientes/Details`
también se puede vincular una persona o materia ya existente al
expediente (antes no existía forma de completar esas relaciones).

### Archivos y videos por expediente

`Expedientes/Details` tiene un panel para subir y descargar archivos:

- **Tipos aceptados**: DOCUMENTO (.pdf/.doc/.docx, 15 MB), IMAGEN
  (.jpg/.png/.webp, 15 MB), VIDEO (.mp4/.mov/.avi/.webm, 200 MB) y AUDIO
  (.mp3/.wav/.m4a, 50 MB) — límites en `TiposArchivoAdjunto` (Models/Documento.cs).
- **Dónde se guardan**: en disco, fuera de `wwwroot` (para que no queden
  servidos como estáticos sin control de acceso), bajo
  `App_Data/uploads/{idExpediente}/` — configurable en
  `appsettings.json` → `Almacenamiento:CarpetaArchivos`.
- **Seguridad**: `ArchivoAlmacenamientoService` nunca usa el nombre
  original del archivo para la ruta física (genera un GUID), valida
  extensión y tamaño según el tipo declarado, y la descarga
  (`Expedientes/DescargarArchivo/{id}`) resuelve el archivo por su
  `idArchivo` en la base de datos, no por una ruta que venga del cliente.
- Si se hospeda detrás de IIS, además del límite de Kestrel configurado
  en `Program.cs` (200 MB) hay que subir `maxAllowedContentLength` en
  `web.config`, porque IIS tiene su propio límite (~30 MB) independiente.

### Recuperación de contraseña

`Account/OlvideContrasenia` → genera un token (`sp_PasswordResetToken_Generar`)
y `Account/RestablecerContrasenia` lo valida y actualiza el hash
(`sp_Usuario_ActualizarContrasenia` + `sp_PasswordResetToken_MarcarUsado`).

El sistema no tiene un servicio de correo configurado (no estaba en el
alcance original), así que en vez de enviar un email que no llegaría a
ningún lado, el enlace de restablecimiento se muestra en pantalla para
que el administrador se lo haga llegar al usuario. Para conectarlo a un
correo real, basta con reemplazar esa pantalla por una llamada a un
`IEmailSender` — el resto del flujo (token, expiración a los 30 minutos,
un solo uso) ya está completo.

### Reportes

`/Reportes` dos vistas, cada una exportable a Excel y PDF:
- **Por rango de fechas**: `sp_Reporte_ExpedientesPorRango`.
- **Por materia**: `sp_Reporte_ExpedientesPorMateria`, con filtro opcional por tipo de materia.

## Estado de los procedimientos almacenados

Los 58 procedimientos de `BD-SBSS-SP.sql` están conectados a la
aplicación — no queda ninguno "mudo". Cada vez que se tocó este
proyecto se volvió a cruzar programáticamente cada llamada en
`Repositories/*.cs` contra su definición SQL (nombre y parámetros).

## Carga masiva de datos de prueba

`Database/BD-SBSS-CargaMasiva.sql` agrega volumen realista para poder
probar filtros, dashboard y reportes con datos variados (el seed
normal solo trae 1 sede y 2 trabajadores). Ejecutar después de
`BD-SBSS-Completa.sql` + `BD-SBSS-SP.sql` + `BD-SBSS-Seed.sql`.

Uso más simple — genera el catálogo de materias, 3 sedes adicionales,
10 trabajadores, 80 personas naturales, 20 jurídicas, 150 expedientes
(con sus audiencias y vínculos a personas/materias) y 25 notificaciones:

```sql
EXEC sp_CargaMasiva_DatosPrueba;
```

O con tamaños propios:

```sql
EXEC sp_CargaMasiva_DatosPrueba
    @cantidadTrabajadores = 5,
    @cantidadPersonasNaturales = 40,
    @cantidadPersonasJuridicas = 10,
    @cantidadExpedientes = 300,
    @cantidadNotificaciones = 50;
```

También se pueden ejecutar por separado (`sp_CargaMasiva_Materias`,
`sp_CargaMasiva_Sedes`, `sp_CargaMasiva_Trabajadores`,
`sp_CargaMasiva_Personas`, `sp_CargaMasiva_Expedientes`,
`sp_CargaMasiva_Notificaciones`) si solo quieres agregar volumen a una
parte puntual.

Es seguro volver a ejecutarlo: cada corrida continúa desde el máximo
DNI/RUC/número de expediente numérico ya existente, así que suma más
datos en vez de chocar con los anteriores.

Los expedientes generados reparten estados de forma realista (~15%
REGISTRADO, 15% ASIGNADO, 25% EN_PROCESO, 35% CONCLUIDO, 10%
ARCHIVADO), y la `fechaCierre` nunca es una fecha futura aunque el
expediente sea reciente y su plazo todavía no se haya cumplido — este
es un bug real que encontré simulando la lógica en Python antes de
entregar el script, no algo que el chequeo de sintaxis pudiera detectar.

## Punto de entrada

El sistema es de uso interno exclusivo del personal del centro — no
existe un sitio público de cara al cliente. La ruta raíz ("/") abre
directamente `Account/Login`. `HomeController` ya solo maneja la
página de error (`/Home/Error`); no tiene una acción `Index`. El
`_Layout.cshtml` no muestra menú alguno hasta que hay sesión iniciada.

## Catálogo de ubigeo (Departamento / Provincia / Distrito)

`Database/BD-SBSS-Ubigeo.sql` llena las tablas `Departamento`,
`Provincia` y `Distrito` (existían en el esquema desde el inicio pero
estaban vacías). Datos: 25 departamentos, 195 provincias, 1839
distritos, fuente RENIEC (paquete de datos abierto `ubigeo-peru`).
Ejecutar después de `BD-SBSS-Completa.sql`; es independiente de
`BD-SBSS-SP.sql` y `BD-SBSS-Seed.sql`.

El script resuelve cada padre (provincia → departamento, distrito →
provincia) por JOIN contra los IDs reales que genera SQL Server, nunca
asumiendo valores de IDENTITY a mano. Se validó que las 1839 filas de
distrito resuelven su provincia y las 195 de provincia resuelven su
departamento sin ninguna huérfana.

Esto **no** incluye todavía el uso de estas tablas en los formularios
(combos en cascada Departamento → Provincia → Distrito, ni guardar la
dirección de una persona) — es el paso previo necesario para poder
hacerlo más adelante. **Actualización:** ya se conectó — ver la
siguiente sección.

## Ubigeo conectado a Personas (Departamento → Provincia → Distrito)

`Personas/CreateNatural` y `Personas/CreateJuridica` ahora tienen un
bloque de dirección opcional con 3 combos en cascada
(`UbigeoController` → `/api/ubigeo/departamentos|provincias/{id}|distritos/{id}`,
poblados por `wwwroot/js/ubigeo.js`).

- Al guardar, si se seleccionó un distrito, se crea una fila en
  `Direccion` y se vincula a la persona en `DetalleDireccion`
  (`sp_Direccion_Insertar` + `sp_DetalleDireccion_Insertar`). Es un paso
  separado del alta de la persona, a propósito: si falla, la persona
  ya quedó creada igual (la dirección es metadata adicional, no un
  dato indispensable del registro).
- Al buscar un RUC en SUNAT, el sistema intenta emparejar el
  distrito/provincia/departamento de texto que devuelve la API contra
  el catálogo ya sembrado (`sp_Distrito_BuscarPorNombres`, comparación
  `UPPER()`) y, si encuentra coincidencia, autoselecciona la cascada
  completa. Si no encuentra coincidencia exacta (acentos, nombre
  distinto, etc.), no rompe nada: simplemente queda para seleccionar a
  mano, y el mensaje en pantalla lo indica.
- `Personas/Details` ahora muestra la dirección principal si existe
  (`sp_Persona_ObtenerDireccionPrincipal`).

## Integración con Decolecta (DNI / RUC)

`Personas/CreateNatural`, `Personas/CreateJuridica` y
`Trabajadores/Create` tienen un botón "Buscar en RENIEC / SUNAT" junto
al campo de documento, que llama a `/api/consultas/dni/{numero}` o
`/api/consultas/ruc/{numero}` (`ConsultasController`) y autocompleta el
resto del formulario.

- **Configurar la API key** (no incluida): `dotnet user-secrets set "Decolecta:ApiKey" "tu_api_key"`
  en desarrollo, o una variable de entorno `Decolecta__ApiKey` en producción.
  Nunca en `appsettings.json` dentro del repositorio.
- **Arquitectura**: `DecolectaService` (HttpClient tipado) →
  `ConsultasController` (valida el formato del documento antes de
  llamar a la API externa, para no gastar cuota con valores inválidos) →
  `wwwroot/js/consultas.js` (fetch + autocompletado del formulario).
- Sin la key, o si el servicio no responde, el formulario no se bloquea:
  se muestra un mensaje y los campos se llenan a mano, igual que antes.
- La dirección que devuelve SUNAT para el RUC se muestra solo como
  referencia en pantalla — no se guarda en la base de datos porque el
  catálogo de ubigeo, aunque ya está poblado (ver sección anterior),
  todavía no está conectado a los formularios de Personas.
- `DecolectaService` se probó (de verdad, no solo compilado) contra un
  servidor HTTP simulado con las respuestas exactas de la documentación
  de Decolecta (incluidos los códigos 400/422), porque este entorno de
  construcción no tiene salida a `api.decolecta.com`. Antes de
  producción conviene una prueba manual contra la API real con una key
  válida.

## Notas para producción

- `AlertaVencimientoBackgroundService` corre dentro de la propia app.
  Si se prefiere desacoplar la revisión de vencimientos del ciclo de
  vida del proceso web, se puede reemplazar por un job de SQL Server
  Agent que llame a `sp_Notificacion_GenerarAlertasVencimiento`.
