/* ================================================================
   SISTEMA DE GESTIÓN DE CONCILIACIÓN SBSS
   Procedimientos almacenados
   Requiere: BD-SBSS-Completa.sql ya ejecutado
   Motor: SQL Server (T-SQL)
   ================================================================ */
USE SBSS_Conciliacion;
GO


-- ================================================================
-- PARTE 2: PROCEDIMIENTOS ALMACENADOS
-- ================================================================

-- ----------------------------------------------------------------
-- 2.1 EXPEDIENTES
-- ----------------------------------------------------------------

CREATE OR ALTER PROCEDURE sp_Expediente_Insertar
    @numeroExpediente VARCHAR(50),
    @idTrabajador     INT,
    @idSede           INT,
    @fechaRecepcion   DATETIME,
    @plazoDias        INT = 30,
    @idExpedienteNuevo INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;

        INSERT INTO Expediente (idTrabajador, idSede, numeroExpediente, estadoProceso, fechaRecepcion, fechaVencimiento)
        VALUES (@idTrabajador, @idSede, @numeroExpediente, 'REGISTRADO', @fechaRecepcion, DATEADD(DAY, @plazoDias, @fechaRecepcion));

        SET @idExpedienteNuevo = SCOPE_IDENTITY();

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE sp_Expediente_AsignarConciliador
    @idExpediente  INT,
    @idConciliador INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE Expediente
       SET idConciliador = @idConciliador,
           estadoProceso = 'ASIGNADO'
     WHERE idExpediente = @idExpediente;
END
GO

CREATE OR ALTER PROCEDURE sp_Expediente_CambiarEstado
    @idExpediente INT,
    @nuevoEstado  VARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE Expediente
       SET estadoProceso = @nuevoEstado,
           fechaCierre = CASE WHEN @nuevoEstado IN ('CONCLUIDO','ARCHIVADO') THEN GETDATE() ELSE fechaCierre END
     WHERE idExpediente = @idExpediente;
END
GO

CREATE OR ALTER PROCEDURE sp_Expediente_ObtenerPorId
    @idExpediente INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT e.*, t.nombreTrabajador + ' ' + t.apePaterTrabajador AS nombreConciliador, s.nombre AS nombreSede
    FROM Expediente e
    LEFT JOIN Trabajador t ON t.idTrabajador = e.idConciliador
    LEFT JOIN Sede s ON s.idSede = e.idSede
    WHERE e.idExpediente = @idExpediente;

    SELECT p.idPersona, dp.rolParticipante, p.correoP, p.celularP
    FROM DetallePersonaExpediente dp
    INNER JOIN Personas p ON p.idPersona = dp.idPersona
    WHERE dp.idExpediente = @idExpediente;

    SELECT m.nombreMateria, dm.descripcionDtMateria
    FROM DetalleMateriaExpediente dm
    INNER JOIN Materia m ON m.idMateria = dm.idMateria
    WHERE dm.idExpediente = @idExpediente;
END
GO

CREATE OR ALTER PROCEDURE sp_Expediente_Listar
    @estado         VARCHAR(50) = NULL,
    @idConciliador  INT = NULL,
    @fechaInicio    DATETIME = NULL,
    @fechaFin       DATETIME = NULL,
    @pagina         INT = 1,
    @tamanioPagina  INT = 20
AS
BEGIN
    SET NOCOUNT ON;
    SELECT e.idExpediente, e.numeroExpediente, e.estadoProceso, e.fechaRecepcion,
           e.fechaVencimiento, t.nombreTrabajador AS conciliador
    FROM Expediente e
    LEFT JOIN Trabajador t ON t.idTrabajador = e.idConciliador
    WHERE (@estado IS NULL OR e.estadoProceso = @estado)
      AND (@idConciliador IS NULL OR e.idConciliador = @idConciliador)
      AND (@fechaInicio IS NULL OR e.fechaRecepcion >= @fechaInicio)
      AND (@fechaFin IS NULL OR e.fechaRecepcion <= @fechaFin)
    ORDER BY e.fechaRecepcion DESC
    OFFSET (@pagina - 1) * @tamanioPagina ROWS
    FETCH NEXT @tamanioPagina ROWS ONLY;
END
GO


-- ----------------------------------------------------------------
-- 2.2 AUDIENCIAS
-- ----------------------------------------------------------------

CREATE OR ALTER PROCEDURE sp_Audiencia_Programar
    @idExpediente      INT,
    @idTipoDocumentos  INT,   -- tipo de documento: "Citación" / "Acta de audiencia"
    @idConciliador     INT,
    @fecha             DATE,
    @hora              TIME,
    @numeroSesion      INT = 1,
    @idAudiencia       INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;

        INSERT INTO DocumentoGenerado (idTipoDocumentos, idExpediente, fechaGenerado)
        VALUES (@idTipoDocumentos, @idExpediente, GETDATE());

        SET @idAudiencia = SCOPE_IDENTITY();

        INSERT INTO Audiencia (idDocumento, fecha, hora, estado, idConciliador, numeroSesion)
        VALUES (@idAudiencia, @fecha, @hora, 'PROGRAMADA', @idConciliador, @numeroSesion);

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE sp_Audiencia_Reprogramar
    @idDocumento INT,
    @nuevaFecha  DATE,
    @nuevaHora   TIME,
    @motivo      VARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE Audiencia
       SET fecha = @nuevaFecha,
           hora = @nuevaHora,
           estado = 'REPROGRAMADA',
           observaciones = ISNULL(observaciones,'') + CHAR(13) + 'Reprogramada: ' + @motivo
     WHERE idDocumento = @idDocumento;
END
GO

CREATE OR ALTER PROCEDURE sp_Audiencia_RegistrarResultado
    @idDocumento    INT,
    @resultado      VARCHAR(50),
    @observaciones  VARCHAR(MAX) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE Audiencia
       SET resultado = @resultado,
           estado = 'REALIZADA',
           observaciones = @observaciones
     WHERE idDocumento = @idDocumento;
END
GO

CREATE OR ALTER PROCEDURE sp_Audiencia_ListarPorExpediente
    @idExpediente INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT a.idDocumento, a.numeroSesion, a.fecha, a.hora, a.estado, a.resultado, a.observaciones,
           t.nombreTrabajador + ' ' + t.apePaterTrabajador AS conciliador
    FROM Audiencia a
    INNER JOIN DocumentoGenerado dg ON dg.idDocumento = a.idDocumento
    LEFT JOIN Trabajador t ON t.idTrabajador = a.idConciliador
    WHERE dg.idExpediente = @idExpediente
    ORDER BY a.numeroSesion;
END
GO


-- ----------------------------------------------------------------
-- 2.3 DOCUMENTOS LEGALES (.docx) Y ACTAS
-- ----------------------------------------------------------------

CREATE OR ALTER PROCEDURE sp_DocumentoGenerado_Insertar
    @idTipoDocumentos   INT,
    @idExpediente       INT,
    @idDocumentoNuevo   INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO DocumentoGenerado (idTipoDocumentos, idExpediente, fechaGenerado)
    VALUES (@idTipoDocumentos, @idExpediente, GETDATE());

    SET @idDocumentoNuevo = SCOPE_IDENTITY();
END
GO

CREATE OR ALTER PROCEDURE sp_ActaConciliacion_Registrar
    @idDocumento             INT,
    @descripcionControversia VARCHAR(MAX),
    @acuerdoConciliatorio    VARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (SELECT 1 FROM ActaConciliacion WHERE idDocumento = @idDocumento)
        UPDATE ActaConciliacion
           SET descripcionControversia = @descripcionControversia,
               acuerdoConciliatorio = @acuerdoConciliatorio
         WHERE idDocumento = @idDocumento;
    ELSE
        INSERT INTO ActaConciliacion (idDocumento, descripcionControversia, acuerdoConciliatorio)
        VALUES (@idDocumento, @descripcionControversia, @acuerdoConciliatorio);
END
GO

CREATE OR ALTER PROCEDURE sp_ActaNotificacion_Registrar
    @idDocumento        INT,
    @fechaNotificacion  DATE,
    @horaNotificacion   TIME
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO ActaNotificacion (idDocumento, fechaNotificacion, horaNotificacion)
    VALUES (@idDocumento, @fechaNotificacion, @horaNotificacion);

    UPDATE Expediente
       SET fechaNotificacion = CAST(@fechaNotificacion AS DATETIME)
     WHERE idExpediente = (SELECT idExpediente FROM DocumentoGenerado WHERE idDocumento = @idDocumento);
END
GO

CREATE OR ALTER PROCEDURE sp_Anexo_Insertar
    @idDocumento    INT,
    @nombreArchivo  VARCHAR(150),
    @rutaArchivo    VARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO Anexo (idDocumento, nombreArchivo, rutaArchivo)
    VALUES (@idDocumento, @nombreArchivo, @rutaArchivo);
END
GO


-- ----------------------------------------------------------------
-- 2.4 ARCHIVOS Y VIDEOS
-- ----------------------------------------------------------------

CREATE OR ALTER PROCEDURE sp_ArchivoAdjunto_Insertar
    @idExpediente   INT,
    @idTrabajador   INT,
    @tipoArchivo    VARCHAR(20),
    @nombreArchivo  VARCHAR(150),
    @rutaArchivo    VARCHAR(255),
    @tamanioKB      INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO ArchivoAdjunto (idExpediente, idTrabajador, tipoArchivo, nombreArchivo, rutaArchivo, tamanioKB)
    VALUES (@idExpediente, @idTrabajador, @tipoArchivo, @nombreArchivo, @rutaArchivo, @tamanioKB);
END
GO

CREATE OR ALTER PROCEDURE sp_ArchivoAdjunto_ListarPorExpediente
    @idExpediente INT,
    @tipoArchivo  VARCHAR(20) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT idArchivo, tipoArchivo, nombreArchivo, rutaArchivo, tamanioKB, fechaSubida
    FROM ArchivoAdjunto
    WHERE idExpediente = @idExpediente
      AND (@tipoArchivo IS NULL OR tipoArchivo = @tipoArchivo)
    ORDER BY fechaSubida DESC;
END
GO

-- Necesario para la descarga: valida a qué expediente pertenece el archivo
-- antes de servirlo (no basta con confiar en el idArchivo de la URL).
CREATE OR ALTER PROCEDURE sp_ArchivoAdjunto_ObtenerPorId
    @idArchivo INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT idArchivo, idExpediente, tipoArchivo, nombreArchivo, rutaArchivo, tamanioKB, fechaSubida
    FROM ArchivoAdjunto
    WHERE idArchivo = @idArchivo;
END
GO


-- ----------------------------------------------------------------
-- 2.5 NOTIFICACIONES (SignalR) Y ALERTAS DE VENCIMIENTO
-- ----------------------------------------------------------------

CREATE OR ALTER PROCEDURE sp_Notificacion_Crear
    @idUsuario          INT,
    @idExpediente       INT = NULL,
    @tipoNotificacion   VARCHAR(50),
    @mensaje            VARCHAR(500)
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO Notificacion (idUsuario, idExpediente, tipoNotificacion, mensaje)
    VALUES (@idUsuario, @idExpediente, @tipoNotificacion, @mensaje);

    SELECT SCOPE_IDENTITY() AS idNotificacion;
END
GO

CREATE OR ALTER PROCEDURE sp_Notificacion_MarcarLeida
    @idNotificacion INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE Notificacion
       SET leida = 1, fechaLectura = GETDATE()
     WHERE idNotificacion = @idNotificacion;
END
GO

CREATE OR ALTER PROCEDURE sp_Notificacion_ListarPorUsuario
    @idUsuario      INT,
    @soloNoLeidas   BIT = 0
AS
BEGIN
    SET NOCOUNT ON;
    SELECT idNotificacion, idUsuario, idExpediente, tipoNotificacion, mensaje, leida, fechaCreacion
    FROM Notificacion
    WHERE idUsuario = @idUsuario
      AND (@soloNoLeidas = 0 OR leida = 0)
    ORDER BY fechaCreacion DESC;
END
GO

-- Pensado para ejecutarse desde un job de SQL Server Agent (una vez al día)
-- y desde ahí notificar por SignalR a los conciliadores/trabajadores.
CREATE OR ALTER PROCEDURE sp_Notificacion_GenerarAlertasVencimiento
    @diasAnticipacion INT = 5
AS
BEGIN
    SET NOCOUNT ON;

    -- OUTPUT devuelve como result set las notificaciones recién insertadas,
    -- para que la aplicación las empuje de inmediato por SignalR (ver
    -- AlertaVencimientoBackgroundService.cs). Antes este SP era "mudo":
    -- insertaba en la tabla pero nadie se enteraba hasta refrescar la página.
    INSERT INTO Notificacion (idUsuario, idExpediente, tipoNotificacion, mensaje)
    OUTPUT inserted.idNotificacion, inserted.idUsuario, inserted.idExpediente,
           inserted.tipoNotificacion, inserted.mensaje, inserted.leida, inserted.fechaCreacion
    SELECT u.idUsuario, e.idExpediente, 'VENCIMIENTO',
           'El expediente ' + e.numeroExpediente + ' vence el ' + CONVERT(VARCHAR(10), e.fechaVencimiento, 103)
    FROM Expediente e
    INNER JOIN Usuario u ON u.idTrabajador = e.idConciliador
    WHERE e.estadoProceso NOT IN ('CONCLUIDO','ARCHIVADO')
      AND e.fechaVencimiento BETWEEN GETDATE() AND DATEADD(DAY, @diasAnticipacion, GETDATE())
      AND NOT EXISTS (
          SELECT 1 FROM Notificacion n
          WHERE n.idExpediente = e.idExpediente
            AND n.tipoNotificacion = 'VENCIMIENTO'
            AND CAST(n.fechaCreacion AS DATE) = CAST(GETDATE() AS DATE)
      );
END
GO


-- ----------------------------------------------------------------
-- 2.6 DASHBOARD (KPIs)
-- ----------------------------------------------------------------

CREATE OR ALTER PROCEDURE sp_Dashboard_ResumenGeneral
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        COUNT(*) AS totalExpedientes,
        SUM(CASE WHEN estadoProceso = 'REGISTRADO' THEN 1 ELSE 0 END) AS registrados,
        SUM(CASE WHEN estadoProceso = 'ASIGNADO' THEN 1 ELSE 0 END) AS asignados,
        SUM(CASE WHEN estadoProceso = 'EN_PROCESO' THEN 1 ELSE 0 END) AS enProceso,
        SUM(CASE WHEN estadoProceso = 'CONCLUIDO' THEN 1 ELSE 0 END) AS concluidos,
        SUM(CASE WHEN estadoProceso = 'ARCHIVADO' THEN 1 ELSE 0 END) AS archivados,
        SUM(CASE WHEN fechaVencimiento < GETDATE() AND estadoProceso NOT IN ('CONCLUIDO','ARCHIVADO') THEN 1 ELSE 0 END) AS vencidos
    FROM Expediente;
END
GO

CREATE OR ALTER PROCEDURE sp_Dashboard_ExpedientesPorMes
    @anio INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT MONTH(fechaRecepcion) AS mes, COUNT(*) AS cantidad
    FROM Expediente
    WHERE YEAR(fechaRecepcion) = @anio
    GROUP BY MONTH(fechaRecepcion)
    ORDER BY mes;
END
GO

CREATE OR ALTER PROCEDURE sp_Dashboard_ExpedientesPorConciliador
AS
BEGIN
    SET NOCOUNT ON;
    SELECT t.nombreTrabajador + ' ' + t.apePaterTrabajador AS conciliador,
           COUNT(e.idExpediente) AS totalAsignados,
           SUM(CASE WHEN e.estadoProceso = 'CONCLUIDO' THEN 1 ELSE 0 END) AS concluidos
    FROM Trabajador t
    LEFT JOIN Expediente e ON e.idConciliador = t.idTrabajador
    GROUP BY t.nombreTrabajador, t.apePaterTrabajador;
END
GO

CREATE OR ALTER PROCEDURE sp_Dashboard_TasaExitoConciliacion
AS
BEGIN
    SET NOCOUNT ON;
    SELECT resultado, COUNT(*) AS cantidad
    FROM Audiencia
    WHERE resultado IS NOT NULL
    GROUP BY resultado;
END
GO


-- ----------------------------------------------------------------
-- 2.7 REPORTES (fuente de datos para exportar a Excel / PDF)
-- ----------------------------------------------------------------

CREATE OR ALTER PROCEDURE sp_Reporte_ExpedientesPorRango
    @fechaInicio DATETIME,
    @fechaFin    DATETIME
AS
BEGIN
    SET NOCOUNT ON;
    SELECT e.numeroExpediente, e.estadoProceso, e.fechaRecepcion, e.fechaNotificacion,
           e.fechaVencimiento, e.fechaCierre,
           t.nombreTrabajador + ' ' + t.apePaterTrabajador AS conciliador,
           s.nombre AS sede
    FROM Expediente e
    LEFT JOIN Trabajador t ON t.idTrabajador = e.idConciliador
    LEFT JOIN Sede s ON s.idSede = e.idSede
    WHERE e.fechaRecepcion BETWEEN @fechaInicio AND @fechaFin
    ORDER BY e.fechaRecepcion;
END
GO

CREATE OR ALTER PROCEDURE sp_Reporte_ExpedientesPorMateria
    @idTipoMateria INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT tm.nombreTipoMateria, m.nombreMateria, COUNT(dme.idExpediente) AS cantidad
    FROM Materia m
    INNER JOIN TipoMateria tm ON tm.idTipoMateria = m.idTipoMateria
    LEFT JOIN DetalleMateriaExpediente dme ON dme.idMateria = m.idMateria
    WHERE (@idTipoMateria IS NULL OR m.idTipoMateria = @idTipoMateria)
    GROUP BY tm.nombreTipoMateria, m.nombreMateria;
END
GO


-- ----------------------------------------------------------------
-- 2.8 USUARIOS Y SEGURIDAD
-- ----------------------------------------------------------------

-- IMPORTANTE: este SP solo devuelve el hash almacenado; la comparación
-- de la contraseña (BCrypt/PBKDF2, etc.) debe hacerse en la capa de
-- aplicación (.NET), nunca comparando texto plano en el SP.
CREATE OR ALTER PROCEDURE sp_Usuario_ObtenerParaLogin
    @usuario VARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT u.idUsuario, u.usuario, u.contrasenia, u.rol, u.idTrabajador,
           t.nombreTrabajador, t.apePaterTrabajador, t.estadoTrabajador
    FROM Usuario u
    INNER JOIN Trabajador t ON t.idTrabajador = u.idTrabajador
    WHERE u.usuario = @usuario;
END
GO

CREATE OR ALTER PROCEDURE sp_PasswordResetToken_Generar
    @idUsuario        INT,
    @token            VARCHAR(255),
    @minutosValidez   INT = 30
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO PasswordResetToken (idUsuario, token, fechaExpiracion)
    VALUES (@idUsuario, @token, DATEADD(MINUTE, @minutosValidez, GETDATE()));
END
GO

CREATE OR ALTER PROCEDURE sp_PasswordResetToken_Validar
    @token VARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT idUsuario
    FROM PasswordResetToken
    WHERE token = @token
      AND esUsado = 0
      AND fechaExpiracion > GETDATE();
END
GO

CREATE OR ALTER PROCEDURE sp_PasswordResetToken_MarcarUsado
    @token VARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE PasswordResetToken SET esUsado = 1 WHERE token = @token;
END
GO

-- ----------------------------------------------------------------
-- 2.9 CATÁLOGOS: SEDES
-- ----------------------------------------------------------------

CREATE OR ALTER PROCEDURE sp_Sede_Insertar
    @nombre         VARCHAR(100),
    @descripcion    VARCHAR(MAX) = NULL,
    @idSedeNuevo    INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO Sede (nombre, descripcion, estadoSede)
    VALUES (@nombre, @descripcion, 'ACTIVA');

    SET @idSedeNuevo = SCOPE_IDENTITY();
END
GO

CREATE OR ALTER PROCEDURE sp_Sede_Actualizar
    @idSede         INT,
    @nombre         VARCHAR(100),
    @descripcion    VARCHAR(MAX) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE Sede
       SET nombre = @nombre,
           descripcion = @descripcion
     WHERE idSede = @idSede;
END
GO

CREATE OR ALTER PROCEDURE sp_Sede_CambiarEstado
    @idSede         INT,
    @estadoSede     VARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE Sede SET estadoSede = @estadoSede WHERE idSede = @idSede;
END
GO

CREATE OR ALTER PROCEDURE sp_Sede_ObtenerPorId
    @idSede INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT idSede, nombre, descripcion, estadoSede FROM Sede WHERE idSede = @idSede;
END
GO

CREATE OR ALTER PROCEDURE sp_Sede_Listar
    @soloActivas BIT = 0
AS
BEGIN
    SET NOCOUNT ON;
    SELECT idSede, nombre, descripcion, estadoSede
    FROM Sede
    WHERE (@soloActivas = 0 OR estadoSede = 'ACTIVA')
    ORDER BY nombre;
END
GO


-- ----------------------------------------------------------------
-- 2.10 CATÁLOGOS: TRABAJADORES Y USUARIOS DE ACCESO
-- ----------------------------------------------------------------

CREATE OR ALTER PROCEDURE sp_Trabajador_Insertar
    @idSedeRef                 INT,
    @dniTrabajador              VARCHAR(15),
    @nombreTrabajador           VARCHAR(100),
    @apePaterTrabajador         VARCHAR(100),
    @apeMaterTrabajador         VARCHAR(100),
    @emailTrabajador            VARCHAR(100) = NULL,
    @telefonoTrabajador         VARCHAR(20) = NULL,
    @registroCivilComercial     VARCHAR(100) = NULL,
    @idTrabajadorNuevo          INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO Trabajador (idSedeRef, dniTrabajador, nombreTrabajador, apePaterTrabajador, apeMaterTrabajador,
                             emailTrabajador, telefonoTrabajador, estadoTrabajador, registroCivilComercial)
    VALUES (@idSedeRef, @dniTrabajador, @nombreTrabajador, @apePaterTrabajador, @apeMaterTrabajador,
            @emailTrabajador, @telefonoTrabajador, 'ACTIVO', @registroCivilComercial);

    SET @idTrabajadorNuevo = SCOPE_IDENTITY();
END
GO

CREATE OR ALTER PROCEDURE sp_Trabajador_Actualizar
    @idTrabajador           INT,
    @idSedeRef              INT,
    @nombreTrabajador       VARCHAR(100),
    @apePaterTrabajador     VARCHAR(100),
    @apeMaterTrabajador     VARCHAR(100),
    @emailTrabajador        VARCHAR(100) = NULL,
    @telefonoTrabajador     VARCHAR(20) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE Trabajador
       SET idSedeRef = @idSedeRef,
           nombreTrabajador = @nombreTrabajador,
           apePaterTrabajador = @apePaterTrabajador,
           apeMaterTrabajador = @apeMaterTrabajador,
           emailTrabajador = @emailTrabajador,
           telefonoTrabajador = @telefonoTrabajador
     WHERE idTrabajador = @idTrabajador;
END
GO

CREATE OR ALTER PROCEDURE sp_Trabajador_CambiarEstado
    @idTrabajador       INT,
    @estadoTrabajador   VARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE Trabajador SET estadoTrabajador = @estadoTrabajador WHERE idTrabajador = @idTrabajador;
END
GO

CREATE OR ALTER PROCEDURE sp_Trabajador_ObtenerPorId
    @idTrabajador INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT t.idTrabajador, t.idSedeRef, t.dniTrabajador, t.nombreTrabajador, t.apePaterTrabajador,
           t.apeMaterTrabajador, t.emailTrabajador, t.telefonoTrabajador, t.estadoTrabajador,
           t.registroCivilComercial, s.nombre AS nombreSede
    FROM Trabajador t
    LEFT JOIN Sede s ON s.idSede = t.idSedeRef
    WHERE t.idTrabajador = @idTrabajador;
END
GO

CREATE OR ALTER PROCEDURE sp_Trabajador_Listar
    @idSedeRef      INT = NULL,
    @soloActivos    BIT = 0
AS
BEGIN
    SET NOCOUNT ON;
    SELECT t.idTrabajador, t.dniTrabajador, t.nombreTrabajador, t.apePaterTrabajador, t.apeMaterTrabajador,
           t.emailTrabajador, t.telefonoTrabajador, t.estadoTrabajador, s.nombre AS nombreSede
    FROM Trabajador t
    LEFT JOIN Sede s ON s.idSede = t.idSedeRef
    WHERE (@idSedeRef IS NULL OR t.idSedeRef = @idSedeRef)
      AND (@soloActivos = 0 OR t.estadoTrabajador = 'ACTIVO')
    ORDER BY t.nombreTrabajador;
END
GO

-- Crea las credenciales de acceso al sistema para un trabajador ya registrado.
-- El hash de la contraseña (BCrypt) se calcula en la aplicación, nunca en el SP.
CREATE OR ALTER PROCEDURE sp_Usuario_Insertar
    @idTrabajador       INT,
    @usuario            VARCHAR(50),
    @contraseniaHash    VARCHAR(255),
    @rol                VARCHAR(50),
    @idUsuarioNuevo     INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO Usuario (idTrabajador, usuario, contrasenia, rol)
    VALUES (@idTrabajador, @usuario, @contraseniaHash, @rol);

    SET @idUsuarioNuevo = SCOPE_IDENTITY();
END
GO


-- ----------------------------------------------------------------
-- 2.11 CATÁLOGOS: TIPOS DE DOCUMENTO
-- ----------------------------------------------------------------

CREATE OR ALTER PROCEDURE sp_Documentos_Listar
AS
BEGIN
    SET NOCOUNT ON;
    SELECT idTipoDocumentos, nombre FROM Documentos ORDER BY nombre;
END
GO


-- ----------------------------------------------------------------
-- 2.12 CATÁLOGOS: TIPOS DE MATERIA Y MATERIAS
-- ----------------------------------------------------------------

CREATE OR ALTER PROCEDURE sp_TipoMateria_Insertar
    @nombreTipoMateria      VARCHAR(100),
    @descripcionTipoMateria VARCHAR(MAX) = NULL,
    @idTipoMateriaNuevo     INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO TipoMateria (nombreTipoMateria, descripcionTipoMateria)
    VALUES (@nombreTipoMateria, @descripcionTipoMateria);

    SET @idTipoMateriaNuevo = SCOPE_IDENTITY();
END
GO

CREATE OR ALTER PROCEDURE sp_TipoMateria_Listar
AS
BEGIN
    SET NOCOUNT ON;
    SELECT idTipoMateria, nombreTipoMateria, descripcionTipoMateria
    FROM TipoMateria
    ORDER BY nombreTipoMateria;
END
GO

CREATE OR ALTER PROCEDURE sp_Materia_Insertar
    @idTipoMateria      INT,
    @nombreMateria      VARCHAR(100),
    @descripcionMateria VARCHAR(MAX) = NULL,
    @idMateriaNuevo     INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO Materia (idTipoMateria, nombreMateria, descripcionMateria)
    VALUES (@idTipoMateria, @nombreMateria, @descripcionMateria);

    SET @idMateriaNuevo = SCOPE_IDENTITY();
END
GO

CREATE OR ALTER PROCEDURE sp_Materia_Actualizar
    @idMateria          INT,
    @idTipoMateria      INT,
    @nombreMateria      VARCHAR(100),
    @descripcionMateria VARCHAR(MAX) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE Materia
       SET idTipoMateria = @idTipoMateria,
           nombreMateria = @nombreMateria,
           descripcionMateria = @descripcionMateria
     WHERE idMateria = @idMateria;
END
GO

CREATE OR ALTER PROCEDURE sp_Materia_ObtenerPorId
    @idMateria INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT m.idMateria, m.idTipoMateria, m.nombreMateria, m.descripcionMateria, tm.nombreTipoMateria
    FROM Materia m
    INNER JOIN TipoMateria tm ON tm.idTipoMateria = m.idTipoMateria
    WHERE m.idMateria = @idMateria;
END
GO

CREATE OR ALTER PROCEDURE sp_Materia_Listar
    @idTipoMateria INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT m.idMateria, m.idTipoMateria, m.nombreMateria, m.descripcionMateria, tm.nombreTipoMateria
    FROM Materia m
    INNER JOIN TipoMateria tm ON tm.idTipoMateria = m.idTipoMateria
    WHERE (@idTipoMateria IS NULL OR m.idTipoMateria = @idTipoMateria)
    ORDER BY tm.nombreTipoMateria, m.nombreMateria;
END
GO


-- ----------------------------------------------------------------
-- 2.13 CATÁLOGOS: PERSONAS (NATURAL / JURÍDICA)
-- ----------------------------------------------------------------

CREATE OR ALTER PROCEDURE sp_PersonaNatural_Insertar
    @idSede                     INT = NULL,
    @celularP                   VARCHAR(20) = NULL,
    @correoP                    VARCHAR(100) = NULL,
    @whatsappP                  VARCHAR(20) = NULL,
    @dniPersonaN                VARCHAR(15),
    @nombrePersonaN             VARCHAR(100),
    @apellidoPaternoPersonaN    VARCHAR(100),
    @apellidoMaternoPersonaN    VARCHAR(100),
    @idPersonaNuevo             INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;

        INSERT INTO Personas (idSede, celularP, correoP, whatsappP, estadoPersona)
        VALUES (@idSede, @celularP, @correoP, @whatsappP, 'ACTIVO');

        SET @idPersonaNuevo = SCOPE_IDENTITY();

        INSERT INTO Natural (idPersonaNatural, dniPersonaN, nombrePersonaN, apellidoPaternoPersonaN, apellidoMaternoPersonaN)
        VALUES (@idPersonaNuevo, @dniPersonaN, @nombrePersonaN, @apellidoPaternoPersonaN, @apellidoMaternoPersonaN);

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE sp_PersonaJuridica_Insertar
    @idSede             INT = NULL,
    @celularP           VARCHAR(20) = NULL,
    @correoP            VARCHAR(100) = NULL,
    @whatsappP          VARCHAR(20) = NULL,
    @ruc                VARCHAR(11),
    @razonSocial        VARCHAR(150),
    @idPersonaNuevo     INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;

        INSERT INTO Personas (idSede, celularP, correoP, whatsappP, estadoPersona)
        VALUES (@idSede, @celularP, @correoP, @whatsappP, 'ACTIVO');

        SET @idPersonaNuevo = SCOPE_IDENTITY();

        INSERT INTO Juridico (idPersonaJuridica, ruc, razonSocial)
        VALUES (@idPersonaNuevo, @ruc, @razonSocial);

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE sp_Persona_ObtenerPorId
    @idPersona INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT p.idPersona, p.celularP, p.correoP, p.whatsappP, p.estadoPersona,
           CASE WHEN n.idPersonaNatural IS NOT NULL THEN 'NATURAL'
                WHEN j.idPersonaJuridica IS NOT NULL THEN 'JURIDICA' END AS tipoPersona,
           n.dniPersonaN, n.nombrePersonaN, n.apellidoPaternoPersonaN, n.apellidoMaternoPersonaN,
           j.ruc, j.razonSocial
    FROM Personas p
    LEFT JOIN Natural n ON n.idPersonaNatural = p.idPersona
    LEFT JOIN Juridico j ON j.idPersonaJuridica = p.idPersona
    WHERE p.idPersona = @idPersona;
END
GO

CREATE OR ALTER PROCEDURE sp_Persona_Listar
    @busqueda VARCHAR(150) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT p.idPersona, p.correoP, p.celularP, p.estadoPersona,
           CASE WHEN n.idPersonaNatural IS NOT NULL THEN 'NATURAL' ELSE 'JURIDICA' END AS tipoPersona,
           COALESCE(n.nombrePersonaN + ' ' + n.apellidoPaternoPersonaN + ' ' + n.apellidoMaternoPersonaN, j.razonSocial) AS nombreCompleto,
           COALESCE(n.dniPersonaN, j.ruc) AS documento
    FROM Personas p
    LEFT JOIN Natural n ON n.idPersonaNatural = p.idPersona
    LEFT JOIN Juridico j ON j.idPersonaJuridica = p.idPersona
    WHERE (@busqueda IS NULL
        OR n.nombrePersonaN LIKE '%' + @busqueda + '%'
        OR n.apellidoPaternoPersonaN LIKE '%' + @busqueda + '%'
        OR n.dniPersonaN LIKE '%' + @busqueda + '%'
        OR j.razonSocial LIKE '%' + @busqueda + '%'
        OR j.ruc LIKE '%' + @busqueda + '%')
    ORDER BY nombreCompleto;
END
GO

CREATE OR ALTER PROCEDURE sp_Persona_CambiarEstado
    @idPersona      INT,
    @estadoPersona  VARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE Personas SET estadoPersona = @estadoPersona WHERE idPersona = @idPersona;
END
GO


-- ----------------------------------------------------------------
-- 2.14 VINCULAR PERSONAS Y MATERIAS A UN EXPEDIENTE
-- (cierran un vacío: antes no existía forma de completar estas
-- relaciones una vez creado el expediente)
-- ----------------------------------------------------------------

CREATE OR ALTER PROCEDURE sp_DetallePersonaExpediente_Agregar
    @idExpediente       INT,
    @idPersona          INT,
    @rolParticipante    VARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (SELECT 1 FROM DetallePersonaExpediente WHERE idExpediente = @idExpediente AND idPersona = @idPersona)
        UPDATE DetallePersonaExpediente
           SET rolParticipante = @rolParticipante
         WHERE idExpediente = @idExpediente AND idPersona = @idPersona;
    ELSE
        INSERT INTO DetallePersonaExpediente (idExpediente, idPersona, rolParticipante)
        VALUES (@idExpediente, @idPersona, @rolParticipante);
END
GO

CREATE OR ALTER PROCEDURE sp_DetalleMateriaExpediente_Agregar
    @idExpediente           INT,
    @idMateria              INT,
    @descripcionDtMateria   VARCHAR(MAX) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (SELECT 1 FROM DetalleMateriaExpediente WHERE idExpediente = @idExpediente AND idMateria = @idMateria)
        UPDATE DetalleMateriaExpediente
           SET descripcionDtMateria = @descripcionDtMateria
         WHERE idExpediente = @idExpediente AND idMateria = @idMateria;
    ELSE
        INSERT INTO DetalleMateriaExpediente (idMateria, idExpediente, descripcionDtMateria)
        VALUES (@idMateria, @idExpediente, @descripcionDtMateria);
END
GO

-- ----------------------------------------------------------------
-- 2.15 NOTIFICACIONES: complementos para el panel completo
-- ----------------------------------------------------------------

CREATE OR ALTER PROCEDURE sp_Notificacion_MarcarTodasLeidas
    @idUsuario INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE Notificacion
       SET leida = 1, fechaLectura = GETDATE()
     WHERE idUsuario = @idUsuario AND leida = 0;
END
GO

-- Resuelve el idUsuario (credenciales de acceso) a partir del idTrabajador,
-- para poder notificar a un conciliador cuando se le asigna un expediente
-- o se le programa una audiencia. Devuelve 0 filas si el trabajador aún
-- no tiene un Usuario creado (no todos los trabajadores acceden al sistema).
CREATE OR ALTER PROCEDURE sp_Usuario_ObtenerIdPorTrabajador
    @idTrabajador INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT idUsuario FROM Usuario WHERE idTrabajador = @idTrabajador;
END
GO

-- ----------------------------------------------------------------
-- 2.16 RECUPERACIÓN DE CONTRASEÑA: actualizar el hash
-- (faltaba: sin esto, un token válido no permitía cambiar nada)
-- ----------------------------------------------------------------

CREATE OR ALTER PROCEDURE sp_Usuario_ActualizarContrasenia
    @idUsuario          INT,
    @contraseniaHash    VARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE Usuario SET contrasenia = @contraseniaHash WHERE idUsuario = @idUsuario;
END
GO

-- ----------------------------------------------------------------
-- 2.17 UBIGEO: combos en cascada y vinculación de direcciones
-- (las tablas ya existían y estaban vacías; se poblaron con
-- BD-SBSS-Ubigeo.sql. Estos SP son los que faltaban para usarlas
-- desde la aplicación.)
-- ----------------------------------------------------------------

CREATE OR ALTER PROCEDURE sp_Departamento_Listar
AS
BEGIN
    SET NOCOUNT ON;
    SELECT idDepartamento, nombre FROM Departamento ORDER BY nombre;
END
GO

CREATE OR ALTER PROCEDURE sp_Provincia_ListarPorDepartamento
    @idDepartamento INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT idProvincia, idDepartamento, nombre
    FROM Provincia
    WHERE idDepartamento = @idDepartamento
    ORDER BY nombre;
END
GO

CREATE OR ALTER PROCEDURE sp_Distrito_ListarPorProvincia
    @idProvincia INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT idDistrito, idProvincia, nombre
    FROM Distrito
    WHERE idProvincia = @idProvincia
    ORDER BY nombre;
END
GO

-- Intenta emparejar el distrito/provincia/departamento que devuelve SUNAT
-- (texto libre, ej. "SAN ISIDRO"/"LIMA"/"LIMA") contra el catálogo ya
-- sembrado, para autoseleccionar la cascada en el formulario. Comparación
-- insensible a mayúsculas (UPPER) — no corrige acentos: si el servidor usa
-- una collation *_CI_AI (la más común en instalaciones en español) también
-- ignora acentos; con una collation *_CS o *_CI_AS podría no encontrar
-- coincidencia si SUNAT y el catálogo difieren en tildes, y el formulario
-- simplemente queda para seleccionar a mano, sin romperse.
CREATE OR ALTER PROCEDURE sp_Distrito_BuscarPorNombres
    @nombreDistrito     VARCHAR(100),
    @nombreProvincia    VARCHAR(100),
    @nombreDepartamento VARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT TOP 1
        dt.idDistrito, dt.idProvincia, p.idDepartamento
    FROM Distrito dt
    INNER JOIN Provincia p ON p.idProvincia = dt.idProvincia
    INNER JOIN Departamento dep ON dep.idDepartamento = p.idDepartamento
    WHERE UPPER(dt.nombre) = UPPER(@nombreDistrito)
      AND UPPER(p.nombre) = UPPER(@nombreProvincia)
      AND UPPER(dep.nombre) = UPPER(@nombreDepartamento);
END
GO

CREATE OR ALTER PROCEDURE sp_Direccion_Insertar
    @idDistrito     INT,
    @tipoVia        VARCHAR(50) = NULL,
    @nombreVia      VARCHAR(100) = NULL,
    @numero         VARCHAR(20) = NULL,
    @manzana        VARCHAR(20) = NULL,
    @lote           VARCHAR(20) = NULL,
    @urbanizacion   VARCHAR(100) = NULL,
    @referencia     VARCHAR(MAX) = NULL,
    @codigoPostal   VARCHAR(10) = NULL,
    @idDireccionNueva INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO Direccion (idDistrito, tipoVia, nombreVia, numero, manzana, lote, urbanizacion, referencia, codigoPostal)
    VALUES (@idDistrito, @tipoVia, @nombreVia, @numero, @manzana, @lote, @urbanizacion, @referencia, @codigoPostal);

    SET @idDireccionNueva = SCOPE_IDENTITY();
END
GO

CREATE OR ALTER PROCEDURE sp_DetalleDireccion_Insertar
    @idPersona      INT,
    @idDireccion    INT,
    @esPrincipal    BIT = 1
AS
BEGIN
    SET NOCOUNT ON;
    -- Si se marca como principal, primero se desmarca cualquier otra
    -- dirección principal previa de la misma persona.
    IF @esPrincipal = 1
        UPDATE DetalleDireccion SET esPrincipal = 0 WHERE idPersona = @idPersona;

    INSERT INTO DetalleDireccion (idPersona, idDireccion, esPrincipal)
    VALUES (@idPersona, @idDireccion, @esPrincipal);
END
GO

-- Dirección principal de una persona, con el desglose geográfico completo,
-- para mostrarla en Personas/Details.
CREATE OR ALTER PROCEDURE sp_Persona_ObtenerDireccionPrincipal
    @idPersona INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT TOP 1
        dir.idDireccion, dir.tipoVia, dir.nombreVia, dir.numero, dir.manzana,
        dir.lote, dir.urbanizacion, dir.referencia, dir.codigoPostal,
        dt.nombre AS distrito, p.nombre AS provincia, dep.nombre AS departamento
    FROM DetalleDireccion dd
    INNER JOIN Direccion dir ON dir.idDireccion = dd.idDireccion
    INNER JOIN Distrito dt ON dt.idDistrito = dir.idDistrito
    INNER JOIN Provincia p ON p.idProvincia = dt.idProvincia
    INNER JOIN Departamento dep ON dep.idDepartamento = p.idDepartamento
    WHERE dd.idPersona = @idPersona
    ORDER BY dd.esPrincipal DESC;
END
GO

-- ----------------------------------------------------------------
-- 2.18 ELIMINAR EXPEDIENTE (con toda su cascada)
-- El esquema no tiene ON DELETE CASCADE, así que hay que borrar en el
-- orden correcto (hijos antes que padres) dentro de una transacción.
-- ----------------------------------------------------------------

CREATE OR ALTER PROCEDURE sp_Expediente_Eliminar
    @idExpediente INT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;

        -- Documentos generados del expediente y todo lo que cuelga de ellos
        -- (Audiencia / ActaConciliacion / ActaNotificacion / Anexo comparten
        -- idDocumento como llave con DocumentoGenerado).
        DECLARE @Documentos TABLE (idDocumento INT);
        INSERT INTO @Documentos (idDocumento)
        SELECT idDocumento FROM DocumentoGenerado WHERE idExpediente = @idExpediente;

        DELETE FROM Anexo WHERE idDocumento IN (SELECT idDocumento FROM @Documentos);
        DELETE FROM Audiencia WHERE idDocumento IN (SELECT idDocumento FROM @Documentos);
        DELETE FROM ActaConciliacion WHERE idDocumento IN (SELECT idDocumento FROM @Documentos);
        DELETE FROM ActaNotificacion WHERE idDocumento IN (SELECT idDocumento FROM @Documentos);
        DELETE FROM DocumentoGenerado WHERE idExpediente = @idExpediente;

        DELETE FROM ArchivoAdjunto WHERE idExpediente = @idExpediente;
        DELETE FROM Notificacion WHERE idExpediente = @idExpediente;
        DELETE FROM DetallePersonaExpediente WHERE idExpediente = @idExpediente;
        DELETE FROM DetalleMateriaExpediente WHERE idExpediente = @idExpediente;
        DELETE FROM DetalleMesaPartes WHERE idExpediente = @idExpediente;
        DELETE FROM Solicitud WHERE idExpediente = @idExpediente;

        DELETE FROM Expediente WHERE idExpediente = @idExpediente;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO


-- ----------------------------------------------------------------
-- 2.19 AUDIENCIA: detalle completo (para la vista de "ver audiencia")
-- ----------------------------------------------------------------

CREATE OR ALTER PROCEDURE sp_Audiencia_ObtenerPorId
    @idDocumento INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        a.idDocumento, a.numeroSesion, a.fecha, a.hora, a.estado, a.resultado, a.observaciones,
        dg.idExpediente, e.numeroExpediente,
        doc.nombre AS tipoDocumento,
        t.nombreTrabajador + ' ' + t.apePaterTrabajador + ' ' + t.apeMaterTrabajador AS conciliador
    FROM Audiencia a
    INNER JOIN DocumentoGenerado dg ON dg.idDocumento = a.idDocumento
    INNER JOIN Expediente e ON e.idExpediente = dg.idExpediente
    INNER JOIN Documentos doc ON doc.idTipoDocumentos = dg.idTipoDocumentos
    LEFT JOIN Trabajador t ON t.idTrabajador = a.idConciliador
    WHERE a.idDocumento = @idDocumento;
END
GO

-- ----------------------------------------------------------------
-- 2.20 ASIGNACIÓN ALEATORIA DE CONCILIADOR
-- Reemplaza la asignación manual: ya no se elige el conciliador desde
-- la UI, el sistema escoge uno al azar entre los trabajadores activos.
-- ----------------------------------------------------------------

CREATE OR ALTER PROCEDURE sp_Expediente_AsignarConciliadorAleatorio
    @idExpediente           INT,
    @idConciliadorAsignado  INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP 1 @idConciliadorAsignado = idTrabajador
    FROM Trabajador
    WHERE estadoTrabajador = 'ACTIVO'
    ORDER BY NEWID();

    IF @idConciliadorAsignado IS NULL
    BEGIN
        RAISERROR('No hay trabajadores activos disponibles para asignar como conciliador.', 16, 1);
        RETURN;
    END

    UPDATE Expediente
       SET idConciliador = @idConciliadorAsignado,
           estadoProceso = 'ASIGNADO'
     WHERE idExpediente = @idExpediente;
END
GO
