/* ================================================================
   SISTEMA DE GESTIÓN DE CONCILIACIÓN SBSS
   Datos de prueba (seed) — sede, trabajadores y usuarios de acceso.

   Ejecutar DESPUÉS de BD-SBSS-Completa.sql y BD-SBSS-SP.sql.

   Las contraseñas ya están hasheadas con BCrypt (factor de costo 11),
   generadas con la misma librería (BCrypt.Net-Next / bcrypt estándar)
   que usa AccountController.Login para verificarlas. NO son texto
   plano en la base de datos, tal como exige sp_Usuario_ObtenerParaLogin.

   Credenciales de prueba:
     Usuario: admin           Contraseña: Conciliacion2026!   Rol: ADMINISTRADOR
     Usuario: conciliador1    Contraseña: Conciliacion2026!   Rol: CONCILIADOR
   ================================================================ */
USE SBSS_Conciliacion;
GO


DECLARE @idSede INT;
DECLARE @idTrabajadorAdmin INT;
DECLARE @idTrabajadorConciliador INT;
DECLARE @hash VARCHAR(255) = '$2b$11$ECu.jr.Cr/hJvHZkNJ/vTeiWGAaRT2926TcDAo67I.7ZP9bEo0XwS';

INSERT INTO Sede (nombre, descripcion, estadoSede)
VALUES ('Sede Trujillo', 'Calle La Corte Mz. J Lt. 33-A – Of. 201 Urb. Natasha, Trujillo', 'ACTIVA');
SET @idSede = SCOPE_IDENTITY();

-- Usuario administrador (para probar catálogos, dashboard y todo el flujo)
INSERT INTO Trabajador (idSedeRef, dniTrabajador, nombreTrabajador, apePaterTrabajador, apeMaterTrabajador,
                         emailTrabajador, telefonoTrabajador, estadoTrabajador)
VALUES (@idSede, '00000001', 'Admin', 'Sistema', 'SBSS', 'admin@gruposbss.com', '943793075', 'ACTIVO');
SET @idTrabajadorAdmin = SCOPE_IDENTITY();

INSERT INTO Usuario (idTrabajador, usuario, contrasenia, rol)
VALUES (@idTrabajadorAdmin, 'admin', @hash, 'ADMINISTRADOR');

-- Un segundo trabajador con rol conciliador, para probar asignación de
-- expedientes / programación de audiencias y que reciba notificaciones reales.
INSERT INTO Trabajador (idSedeRef, dniTrabajador, nombreTrabajador, apePaterTrabajador, apeMaterTrabajador,
                         emailTrabajador, telefonoTrabajador, estadoTrabajador)
VALUES (@idSede, '00000002', 'María', 'Torres', 'Vega', 'conciliador1@gruposbss.com', '943793076', 'ACTIVO');
SET @idTrabajadorConciliador = SCOPE_IDENTITY();

INSERT INTO Usuario (idTrabajador, usuario, contrasenia, rol)
VALUES (@idTrabajadorConciliador, 'conciliador1', @hash, 'CONCILIADOR');

-- Catálogo de tipos de documento (antes vacío: sin esto, el combo de
-- "tipo de documento" al programar una audiencia no tenía opciones).
INSERT INTO Documentos (nombre) VALUES
    ('Citación a audiencia'),
    ('Acta de conciliación'),
    ('Acta de notificación'),
    ('Constancia de inasistencia');

SELECT @idSede AS idSede, @idTrabajadorAdmin AS idTrabajadorAdmin, @idTrabajadorConciliador AS idTrabajadorConciliador;
