/* ================================================================
   SISTEMA DE GESTIÓN DE CONCILIACIÓN SBSS
   Procedimientos de carga masiva de datos de prueba

   Objetivo: tener un dataset realista para probar filtros, dashboard,
   reportes y notificaciones, sin depender de cargar todo a mano.

   No reemplaza a BD-SBSS-Seed.sql (ese sigue siendo el que crea las
   credenciales admin/conciliador1) — este script genera VOLUMEN
   adicional: trabajadores, personas, materias, expedientes con sus
   audiencias/vínculos, y notificaciones de ejemplo.

   Ejecutar DESPUÉS de BD-SBSS-Completa.sql + BD-SBSS-SP.sql +
   BD-SBSS-Seed.sql (necesita al menos una Sede y un Usuario existentes
   para poder generar notificaciones y asignar conciliadores).

   Es seguro volver a ejecutarlo: cada corrida usa como punto de
   partida el máximo DNI/RUC/número de expediente numérico ya
   existente, así que agrega más datos en vez de chocar con los
   anteriores (siempre que esos valores existentes sean numéricos,
   como los que genera el propio script y el seed).
   ================================================================ */

USE SBSS_Conciliacion;
GO


-- ----------------------------------------------------------------
-- 1) MATERIAS: catálogo realista (Civil y Comercial, Familia,
--    Contrataciones con el Estado), igual al que se muestra en el
--    sitio y en la documentación del sistema. Idempotente: si el
--    tipo de materia ya existe, no lo duplica.
-- ----------------------------------------------------------------

CREATE OR ALTER PROCEDURE sp_CargaMasiva_Materias
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Tipos TABLE (nombre VARCHAR(100), descripcion VARCHAR(300));
    INSERT INTO @Tipos (nombre, descripcion) VALUES
        ('Civil y Comercial', 'Conflictos relacionados con obligaciones, contratos y relaciones entre particulares o empresas.'),
        ('Familia', 'Conflictos familiares, priorizando el bienestar de las personas involucradas.'),
        ('Contrataciones con el Estado', 'Controversias derivadas de contratos celebrados con entidades públicas.');

    INSERT INTO TipoMateria (nombreTipoMateria, descripcionTipoMateria)
    SELECT t.nombre, t.descripcion
    FROM @Tipos t
    WHERE NOT EXISTS (SELECT 1 FROM TipoMateria tm WHERE tm.nombreTipoMateria = t.nombre);

    DECLARE @Materias TABLE (idTipoMateria INT, nombreMateria VARCHAR(100), descripcionMateria VARCHAR(300));

    INSERT INTO @Materias (idTipoMateria, nombreMateria, descripcionMateria)
    SELECT tm.idTipoMateria, m.nombreMateria, m.descripcionMateria
    FROM TipoMateria tm
    CROSS APPLY (
        SELECT * FROM (VALUES
            ('Civil y Comercial', 'Pago de deudas u obligaciones', 'Incumplimiento en el pago de una suma de dinero.'),
            ('Civil y Comercial', 'Incumplimiento de contratos', 'Una de las partes no cumplió lo pactado.'),
            ('Civil y Comercial', 'Indemnización por daños y perjuicios', 'Resarcimiento por un daño causado.'),
            ('Civil y Comercial', 'Conflictos entre socios o empresas', 'Desacuerdos societarios o comerciales.'),
            ('Civil y Comercial', 'Problemas relacionados con bienes o servicios', 'Disputas sobre la entrega o calidad de bienes/servicios.'),
            ('Familia', 'Pensión de alimentos', 'Fijación o cobro de una pensión alimenticia.'),
            ('Familia', 'Tenencia de hijos', 'Determinar con quién vivirán los hijos menores.'),
            ('Familia', 'Régimen de visitas', 'Horario y condiciones de visita al menor.'),
            ('Familia', 'Variación o aumento de pensión de alimentos', 'Modificación de una pensión ya fijada.'),
            ('Familia', 'Liquidación de sociedad de gananciales', 'Reparto de bienes tras una separación.'),
            ('Contrataciones con el Estado', 'Conflictos derivados de contratos con el Estado', 'Controversias contractuales con una entidad pública.'),
            ('Contrataciones con el Estado', 'Discrepancias en la ejecución contractual', 'Desacuerdos durante la ejecución de un contrato público.'),
            ('Contrataciones con el Estado', 'Controversias por pagos u obligaciones', 'Disputas sobre pagos pendientes de una entidad pública.')
        ) AS x(tipo, nombreMateria, descripcionMateria)
        WHERE x.tipo = tm.nombreTipoMateria
    ) m
    WHERE NOT EXISTS (
        SELECT 1 FROM Materia ma WHERE ma.idTipoMateria = tm.idTipoMateria AND ma.nombreMateria = m.nombreMateria
    );

    INSERT INTO Materia (idTipoMateria, nombreMateria, descripcionMateria)
    SELECT idTipoMateria, nombreMateria, descripcionMateria FROM @Materias;

    SELECT (SELECT COUNT(*) FROM TipoMateria) AS totalTiposMateria, (SELECT COUNT(*) FROM Materia) AS totalMaterias;
END
GO


-- ----------------------------------------------------------------
-- 2) SEDES adicionales (además de la que trae BD-SBSS-Seed.sql), para
--    poder probar filtros por sede con datos variados.
-- ----------------------------------------------------------------

CREATE OR ALTER PROCEDURE sp_CargaMasiva_Sedes
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Sedes TABLE (nombre VARCHAR(100), descripcion VARCHAR(300));
    INSERT INTO @Sedes (nombre, descripcion) VALUES
        ('Sede Lima', 'Av. Javier Prado Este 1234, San Isidro, Lima'),
        ('Sede Chiclayo', 'Av. Balta 567, Chiclayo, Lambayeque'),
        ('Sede Arequipa', 'Calle Mercaderes 210, Arequipa');

    INSERT INTO Sede (nombre, descripcion, estadoSede)
    SELECT s.nombre, s.descripcion, 'ACTIVA'
    FROM @Sedes s
    WHERE NOT EXISTS (SELECT 1 FROM Sede sx WHERE sx.nombre = s.nombre);

    SELECT COUNT(*) AS totalSedes FROM Sede;
END
GO


-- ----------------------------------------------------------------
-- 3) TRABAJADORES de prueba, repartidos entre las sedes existentes.
-- ----------------------------------------------------------------

CREATE OR ALTER PROCEDURE sp_CargaMasiva_Trabajadores
    @cantidad INT = 10
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @dni INT = ISNULL((SELECT MAX(TRY_CAST(dniTrabajador AS INT)) FROM Trabajador WHERE ISNUMERIC(dniTrabajador) = 1), 40000000) + 1;
    DECLARE @i INT = 1;
    DECLARE @totalSedes INT = (SELECT COUNT(*) FROM Sede);

    IF @totalSedes = 0
    BEGIN
        RAISERROR('No hay Sedes registradas. Ejecuta sp_CargaMasiva_Sedes (o BD-SBSS-Seed.sql) primero.', 16, 1);
        RETURN;
    END

    DECLARE @Nombres TABLE (nombre VARCHAR(50));
    INSERT INTO @Nombres VALUES ('Juan'),('Carlos'),('Luis'),('Miguel'),('José'),('Pedro'),('Jorge'),('Fernando'),
        ('Ricardo'),('Manuel'),('Diego'),('Raúl'),('Enrique'),('María'),('Rosa'),('Carmen'),('Ana'),('Patricia'),
        ('Sandra'),('Claudia'),('Karina'),('Lucía'),('Gabriela'),('Milagros'),('Fiorella'),('Pilar'),('Vanessa'),('Diana');

    DECLARE @Apellidos TABLE (apellido VARCHAR(50));
    INSERT INTO @Apellidos VALUES ('García'),('Rodríguez'),('Martínez'),('López'),('González'),('Pérez'),('Sánchez'),
        ('Ramírez'),('Torres'),('Flores'),('Vargas'),('Castillo'),('Rojas'),('Mendoza'),('Chávez'),('Quispe'),
        ('Huamán'),('Rivera'),('Gutiérrez'),('Ortiz');

    WHILE @i <= @cantidad
    BEGIN
        DECLARE @nombre VARCHAR(50), @apePat VARCHAR(50), @apeMat VARCHAR(50), @idSede INT;

        SELECT TOP 1 @nombre = nombre FROM @Nombres ORDER BY NEWID();
        SELECT TOP 1 @apePat = apellido FROM @Apellidos ORDER BY NEWID();
        SELECT TOP 1 @apeMat = apellido FROM @Apellidos ORDER BY NEWID();
        SELECT TOP 1 @idSede = idSede FROM Sede ORDER BY NEWID();

        INSERT INTO Trabajador (idSedeRef, dniTrabajador, nombreTrabajador, apePaterTrabajador, apeMaterTrabajador,
                                 emailTrabajador, telefonoTrabajador, estadoTrabajador)
        VALUES (
            @idSede,
            CAST(@dni AS VARCHAR(15)),
            @nombre, @apePat, @apeMat,
            LOWER(@nombre) + '.' + LOWER(@apePat) + '@gruposbss.com',
            '9' + RIGHT('00000000' + CAST(ABS(CHECKSUM(NEWID())) % 100000000 AS VARCHAR(8)), 8),
            'ACTIVO'
        );

        SET @dni += 1;
        SET @i += 1;
    END

    SELECT COUNT(*) AS totalTrabajadores FROM Trabajador;
END
GO


-- ----------------------------------------------------------------
-- 4) PERSONAS de prueba: naturales (DNI) y jurídicas (RUC).
-- ----------------------------------------------------------------

CREATE OR ALTER PROCEDURE sp_CargaMasiva_Personas
    @cantidadNaturales  INT = 80,
    @cantidadJuridicas  INT = 20
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Nombres TABLE (nombre VARCHAR(50));
    INSERT INTO @Nombres VALUES ('Juan'),('Carlos'),('Luis'),('Miguel'),('José'),('Pedro'),('Jorge'),('Fernando'),
        ('Ricardo'),('Manuel'),('Diego'),('Alberto'),('Raúl'),('Óscar'),('Enrique'),('María'),('Rosa'),('Carmen'),
        ('Ana'),('Patricia'),('Sandra'),('Claudia'),('Karina'),('Lucía'),('Gabriela'),('Milagros'),('Fiorella'),
        ('Pilar'),('Vanessa'),('Diana');

    DECLARE @Apellidos TABLE (apellido VARCHAR(50));
    INSERT INTO @Apellidos VALUES ('García'),('Rodríguez'),('Martínez'),('López'),('González'),('Pérez'),('Sánchez'),
        ('Ramírez'),('Torres'),('Flores'),('Vargas'),('Castillo'),('Rojas'),('Mendoza'),('Chávez'),('Quispe'),
        ('Huamán'),('Rivera'),('Gutiérrez'),('Ortiz');

    DECLARE @RubrosEmpresa TABLE (rubro VARCHAR(50));
    INSERT INTO @RubrosEmpresa VALUES ('Comercial'),('Inversiones'),('Corporación'),('Distribuidora'),('Servicios'),('Grupo'),('Industrias');

    DECLARE @TiposEmpresa TABLE (sufijo VARCHAR(10));
    INSERT INTO @TiposEmpresa VALUES ('S.A.C.'),('E.I.R.L.'),('S.A.'),('S.R.L.');

    -- ===== Naturales =====
    DECLARE @dniPersona INT = ISNULL((SELECT MAX(TRY_CAST(dniPersonaN AS INT)) FROM Natural WHERE ISNUMERIC(dniPersonaN) = 1), 70000000) + 1;
    DECLARE @i INT = 1;

    WHILE @i <= @cantidadNaturales
    BEGIN
        DECLARE @nombre VARCHAR(50), @apePat VARCHAR(50), @apeMat VARCHAR(50), @idPersonaNueva INT;

        SELECT TOP 1 @nombre = nombre FROM @Nombres ORDER BY NEWID();
        SELECT TOP 1 @apePat = apellido FROM @Apellidos ORDER BY NEWID();
        SELECT TOP 1 @apeMat = apellido FROM @Apellidos ORDER BY NEWID();

        INSERT INTO Personas (celularP, correoP, whatsappP, estadoPersona)
        VALUES (
            '9' + RIGHT('00000000' + CAST(ABS(CHECKSUM(NEWID())) % 100000000 AS VARCHAR(8)), 8),
            LOWER(@nombre) + '.' + LOWER(@apePat) + CAST(@i AS VARCHAR(10)) + '@correo-prueba.com',
            '9' + RIGHT('00000000' + CAST(ABS(CHECKSUM(NEWID())) % 100000000 AS VARCHAR(8)), 8),
            'ACTIVO'
        );
        SET @idPersonaNueva = SCOPE_IDENTITY();

        INSERT INTO Natural (idPersonaNatural, dniPersonaN, nombrePersonaN, apellidoPaternoPersonaN, apellidoMaternoPersonaN)
        VALUES (@idPersonaNueva, CAST(@dniPersona AS VARCHAR(15)), @nombre, @apePat, @apeMat);

        SET @dniPersona += 1;
        SET @i += 1;
    END

    -- ===== Jurídicas =====
    -- BIGINT, no INT: un RUC peruano tiene 11 dígitos (ej. 20500000000) y el
    -- máximo de un INT en SQL Server es 2,147,483,647 — unas 10 veces menor,
    -- lo que provocaba "Arithmetic overflow error converting expression to
    -- data type int" al intentar guardar el valor inicial.
    DECLARE @rucBase BIGINT = ISNULL((SELECT MAX(TRY_CAST(ruc AS BIGINT)) FROM Juridico WHERE ISNUMERIC(ruc) = 1), 20500000000) + 1;
    SET @i = 1;

    WHILE @i <= @cantidadJuridicas
    BEGIN
        DECLARE @rubro VARCHAR(50), @apellidoEmpresa VARCHAR(50), @sufijo VARCHAR(10), @idPersonaJNueva INT;

        SELECT TOP 1 @rubro = rubro FROM @RubrosEmpresa ORDER BY NEWID();
        SELECT TOP 1 @apellidoEmpresa = apellido FROM @Apellidos ORDER BY NEWID();
        SELECT TOP 1 @sufijo = sufijo FROM @TiposEmpresa ORDER BY NEWID();

        INSERT INTO Personas (celularP, correoP, whatsappP, estadoPersona)
        VALUES (
            '01' + RIGHT('0000000' + CAST(ABS(CHECKSUM(NEWID())) % 10000000 AS VARCHAR(7)), 7),
            'contacto' + CAST(@i AS VARCHAR(10)) + '@empresa-prueba.com',
            NULL,
            'ACTIVO'
        );
        SET @idPersonaJNueva = SCOPE_IDENTITY();

        INSERT INTO Juridico (idPersonaJuridica, ruc, razonSocial)
        VALUES (@idPersonaJNueva, CAST(@rucBase AS VARCHAR(11)), @rubro + ' ' + @apellidoEmpresa + ' ' + @sufijo);

        SET @rucBase += 1;
        SET @i += 1;
    END

    SELECT
        (SELECT COUNT(*) FROM Natural)  AS totalPersonasNaturales,
        (SELECT COUNT(*) FROM Juridico) AS totalPersonasJuridicas;
END
GO


-- ----------------------------------------------------------------
-- 5) EXPEDIENTES de prueba, con sus vínculos (personas/materias) y,
--    cuando el estado lo amerita, una audiencia. Distribución de
--    estados pensada para que el dashboard y los reportes tengan
--    datos variados que mostrar.
-- ----------------------------------------------------------------

CREATE OR ALTER PROCEDURE sp_CargaMasiva_Expedientes
    @cantidad INT = 150
AS
BEGIN
    SET NOCOUNT ON;

    IF (SELECT COUNT(*) FROM Sede) = 0 OR (SELECT COUNT(*) FROM Trabajador) = 0
    BEGIN
        RAISERROR('Se necesitan al menos una Sede y un Trabajador antes de generar expedientes.', 16, 1);
        RETURN;
    END
    IF (SELECT COUNT(*) FROM Personas) = 0
    BEGIN
        RAISERROR('Se necesitan Personas registradas (ejecuta sp_CargaMasiva_Personas primero).', 16, 1);
        RETURN;
    END
    IF (SELECT COUNT(*) FROM Materia) = 0
    BEGIN
        RAISERROR('Se necesitan Materias registradas (ejecuta sp_CargaMasiva_Materias primero).', 16, 1);
        RETURN;
    END

    DECLARE @idTipoDocCitacion INT = (SELECT TOP 1 idTipoDocumentos FROM Documentos WHERE nombre = 'Citación a audiencia');

    -- Número correlativo de expediente: continúa desde el máximo ya usado.
    DECLARE @numBase INT = ISNULL((
        SELECT MAX(TRY_CAST(RIGHT(numeroExpediente, 6) AS INT))
        FROM Expediente
        WHERE numeroExpediente LIKE 'EXP-2026-%' AND ISNUMERIC(RIGHT(numeroExpediente, 6)) = 1
    ), 0) + 1;

    DECLARE @i INT = 1;

    WHILE @i <= @cantidad
    BEGIN
        DECLARE @idSede INT, @idTrabajador INT, @idConciliador INT, @fechaRecepcion DATETIME,
                @plazoDias INT, @fechaVencimiento DATETIME, @estado VARCHAR(20), @fechaCierre DATETIME = NULL,
                @idExpedienteNuevo INT, @numeroExpediente VARCHAR(50), @dado INT;

        SELECT TOP 1 @idSede = idSede FROM Sede ORDER BY NEWID();
        SELECT TOP 1 @idTrabajador = idTrabajador FROM Trabajador ORDER BY NEWID();
        SELECT TOP 1 @idConciliador = idTrabajador FROM Trabajador ORDER BY NEWID();

        -- Fecha de recepción: entre hoy y 150 días atrás.
        SET @fechaRecepcion = DATEADD(DAY, -ABS(CHECKSUM(NEWID())) % 150, CAST(GETDATE() AS DATE));
        SET @plazoDias = 15 + ABS(CHECKSUM(NEWID())) % 31; -- entre 15 y 45 días
        SET @fechaVencimiento = DATEADD(DAY, @plazoDias, @fechaRecepcion);

        -- Distribución de estados (aprox.): 15% REGISTRADO, 15% ASIGNADO,
        -- 25% EN_PROCESO, 35% CONCLUIDO, 10% ARCHIVADO.
        SET @dado = ABS(CHECKSUM(NEWID())) % 100;
        SET @estado = CASE
            WHEN @dado < 15 THEN 'REGISTRADO'
            WHEN @dado < 30 THEN 'ASIGNADO'
            WHEN @dado < 55 THEN 'EN_PROCESO'
            WHEN @dado < 90 THEN 'CONCLUIDO'
            ELSE 'ARCHIVADO'
        END;

        IF @estado = 'REGISTRADO' SET @idConciliador = NULL;
        IF @estado IN ('CONCLUIDO', 'ARCHIVADO')
        BEGIN
            -- fechaCierre no puede ser futura: se limita a como máximo "hoy",
            -- aunque el plazo del expediente todavía no se haya cumplido.
            DECLARE @diasHastaHoy INT = DATEDIFF(DAY, @fechaRecepcion, CAST(GETDATE() AS DATE));
            DECLARE @maxOffsetCierre INT = CASE WHEN @diasHastaHoy < @plazoDias THEN @diasHastaHoy ELSE @plazoDias - 1 END;
            IF @maxOffsetCierre < 0 SET @maxOffsetCierre = 0;
            SET @fechaCierre = DATEADD(DAY, ABS(CHECKSUM(NEWID())) % (@maxOffsetCierre + 1), @fechaRecepcion);
        END

        SET @numeroExpediente = 'EXP-2026-' + RIGHT('000000' + CAST(@numBase AS VARCHAR(6)), 6);

        INSERT INTO Expediente (idConciliador, idTrabajador, idSede, numeroExpediente, estadoProceso,
                                 fechaRecepcion, fechaVencimiento, fechaCierre)
        VALUES (@idConciliador, @idTrabajador, @idSede, @numeroExpediente, @estado,
                @fechaRecepcion, @fechaVencimiento, @fechaCierre);

        SET @idExpedienteNuevo = SCOPE_IDENTITY();

        -- Vincular 1 a 2 personas con roles variados. No hace falta DISTINCT:
        -- idPersona ya es la clave primaria de Personas, cada fila es única
        -- (y SQL Server no permite ORDER BY NEWID() junto con SELECT DISTINCT,
        -- porque NEWID() no está en la lista de columnas seleccionadas).
        DECLARE @numPersonas INT = 1 + ABS(CHECKSUM(NEWID())) % 2;
        INSERT INTO DetallePersonaExpediente (idExpediente, idPersona, rolParticipante)
        SELECT TOP (@numPersonas) @idExpedienteNuevo, idPersona,
               CASE ABS(CHECKSUM(NEWID())) % 4
                   WHEN 0 THEN 'SOLICITANTE' WHEN 1 THEN 'INVITADO'
                   WHEN 2 THEN 'APODERADO' ELSE 'TESTIGO' END
        FROM Personas
        ORDER BY NEWID();

        -- Vincular 1 a 2 materias (mismo motivo: idMateria ya es único, es la PK de Materia).
        DECLARE @numMaterias INT = 1 + ABS(CHECKSUM(NEWID())) % 2;
        INSERT INTO DetalleMateriaExpediente (idMateria, idExpediente)
        SELECT TOP (@numMaterias) idMateria, @idExpedienteNuevo
        FROM Materia
        ORDER BY NEWID();

        -- Si el expediente ya pasó por asignación, generar su audiencia.
        IF @estado IN ('ASIGNADO', 'EN_PROCESO', 'CONCLUIDO', 'ARCHIVADO') AND @idTipoDocCitacion IS NOT NULL
        BEGIN
            DECLARE @idDocumentoNuevo INT, @fechaAudiencia DATE, @horaAudiencia TIME, @estadoAudiencia VARCHAR(20), @resultadoAudiencia VARCHAR(30) = NULL;

            INSERT INTO DocumentoGenerado (idTipoDocumentos, idExpediente, fechaGenerado)
            VALUES (@idTipoDocCitacion, @idExpedienteNuevo, @fechaRecepcion);
            SET @idDocumentoNuevo = SCOPE_IDENTITY();

            SET @fechaAudiencia = DATEADD(DAY, 3 + ABS(CHECKSUM(NEWID())) % 10, @fechaRecepcion);
            SET @horaAudiencia = DATEADD(MINUTE, (ABS(CHECKSUM(NEWID())) % 16) * 30, CAST('09:00' AS TIME)); -- 09:00 a 16:30

            IF @estado IN ('CONCLUIDO', 'ARCHIVADO')
            BEGIN
                SET @estadoAudiencia = 'REALIZADA';
                SET @resultadoAudiencia = CASE ABS(CHECKSUM(NEWID())) % 5
                    WHEN 0 THEN 'ACUERDO_TOTAL' WHEN 1 THEN 'ACUERDO_PARCIAL' WHEN 2 THEN 'NO_ACUERDO'
                    WHEN 3 THEN 'INASISTENCIA_AMBAS' ELSE 'INASISTENCIA_UNA' END;
            END
            ELSE
                SET @estadoAudiencia = 'PROGRAMADA';

            INSERT INTO Audiencia (idDocumento, idConciliador, numeroSesion, fecha, hora, estado, resultado)
            VALUES (@idDocumentoNuevo, @idConciliador, 1, @fechaAudiencia, @horaAudiencia, @estadoAudiencia, @resultadoAudiencia);
        END

        SET @numBase += 1;
        SET @i += 1;
    END

    SELECT COUNT(*) AS totalExpedientes FROM Expediente;
END
GO


-- ----------------------------------------------------------------
-- 6) NOTIFICACIONES de ejemplo, para los usuarios ya existentes
--    (para que el panel de notificaciones no arranque vacío).
-- ----------------------------------------------------------------

CREATE OR ALTER PROCEDURE sp_CargaMasiva_Notificaciones
    @cantidad INT = 20
AS
BEGIN
    SET NOCOUNT ON;

    IF (SELECT COUNT(*) FROM Usuario) = 0
    BEGIN
        RAISERROR('No hay usuarios registrados (ejecuta BD-SBSS-Seed.sql primero).', 16, 1);
        RETURN;
    END

    DECLARE @i INT = 1;

    WHILE @i <= @cantidad
    BEGIN
        DECLARE @idUsuario INT, @idExpediente INT, @tipo VARCHAR(20), @numeroExp VARCHAR(50), @mensaje VARCHAR(500), @leida BIT;

        SELECT TOP 1 @idUsuario = idUsuario FROM Usuario ORDER BY NEWID();
        SELECT TOP 1 @idExpediente = idExpediente, @numeroExp = numeroExpediente FROM Expediente ORDER BY NEWID();
        SET @tipo = CASE ABS(CHECKSUM(NEWID())) % 3 WHEN 0 THEN 'ASIGNACION' WHEN 1 THEN 'AUDIENCIA' ELSE 'VENCIMIENTO' END;
        SET @leida = CASE WHEN ABS(CHECKSUM(NEWID())) % 2 = 0 THEN 1 ELSE 0 END;

        SET @mensaje = CASE @tipo
            WHEN 'ASIGNACION' THEN 'Se te asignó el expediente ' + ISNULL(@numeroExp, '') + ' como conciliador.'
            WHEN 'AUDIENCIA' THEN 'Se programó una audiencia para el expediente ' + ISNULL(@numeroExp, '') + '.'
            ELSE 'El expediente ' + ISNULL(@numeroExp, '') + ' está próximo a vencer.'
        END;

        INSERT INTO Notificacion (idUsuario, idExpediente, tipoNotificacion, mensaje, leida, fechaCreacion, fechaLectura)
        VALUES (
            @idUsuario, @idExpediente, @tipo, @mensaje, @leida,
            DATEADD(DAY, -ABS(CHECKSUM(NEWID())) % 20, GETDATE()),
            CASE WHEN @leida = 1 THEN GETDATE() ELSE NULL END
        );

        SET @i += 1;
    END

    SELECT COUNT(*) AS totalNotificaciones FROM Notificacion;
END
GO


-- ----------------------------------------------------------------
-- 7) ORQUESTADOR: ejecuta todo lo anterior en el orden correcto.
--    Uso típico:  EXEC sp_CargaMasiva_DatosPrueba;
--    o con tamaños propios:
--      EXEC sp_CargaMasiva_DatosPrueba @cantidadExpedientes = 300;
-- ----------------------------------------------------------------

CREATE OR ALTER PROCEDURE sp_CargaMasiva_DatosPrueba
    @cantidadTrabajadores       INT = 10,
    @cantidadPersonasNaturales  INT = 80,
    @cantidadPersonasJuridicas  INT = 20,
    @cantidadExpedientes        INT = 150,
    @cantidadNotificaciones     INT = 25
AS
BEGIN
    SET NOCOUNT ON;

    EXEC sp_CargaMasiva_Materias;
    EXEC sp_CargaMasiva_Sedes;
    EXEC sp_CargaMasiva_Trabajadores @cantidad = @cantidadTrabajadores;
    EXEC sp_CargaMasiva_Personas @cantidadNaturales = @cantidadPersonasNaturales, @cantidadJuridicas = @cantidadPersonasJuridicas;
    EXEC sp_CargaMasiva_Expedientes @cantidad = @cantidadExpedientes;
    EXEC sp_CargaMasiva_Notificaciones @cantidad = @cantidadNotificaciones;

    SELECT
        (SELECT COUNT(*) FROM Sede)        AS sedes,
        (SELECT COUNT(*) FROM Trabajador)  AS trabajadores,
        (SELECT COUNT(*) FROM Natural)     AS personasNaturales,
        (SELECT COUNT(*) FROM Juridico)    AS personasJuridicas,
        (SELECT COUNT(*) FROM TipoMateria) AS tiposMateria,
        (SELECT COUNT(*) FROM Materia)     AS materias,
        (SELECT COUNT(*) FROM Expediente)  AS expedientes,
        (SELECT COUNT(*) FROM Audiencia)   AS audiencias,
        (SELECT COUNT(*) FROM Notificacion) AS notificaciones;
END
GO


-- ================================================================
-- 8) EJECUCIÓN AUTOMÁTICA
--
-- IMPORTANTE: todo lo anterior solo CREA los procedimientos; no
-- inserta ni una fila por sí solo. Esta sección los ejecuta, para
-- que al correr este archivo los datos queden cargados de verdad
-- sin tener que acordarse de lanzar un EXEC aparte.
--
-- Si prefieres controlar tú el momento (o las cantidades), comenta
-- el bloque de abajo y ejecuta manualmente:
--     EXEC sp_CargaMasiva_DatosPrueba @cantidadExpedientes = 300;
-- ================================================================

PRINT '--- Iniciando carga masiva de datos de prueba ---';

BEGIN TRY
    EXEC sp_CargaMasiva_DatosPrueba;
    PRINT '--- Carga masiva finalizada correctamente ---';
END TRY
BEGIN CATCH
    PRINT '--- ERROR durante la carga masiva ---';
    PRINT 'Procedimiento: ' + ISNULL(ERROR_PROCEDURE(), '(ninguno)');
    PRINT 'Línea: ' + CAST(ERROR_LINE() AS VARCHAR(10));
    PRINT 'Mensaje: ' + ERROR_MESSAGE();
    THROW;
END CATCH
GO

-- Verificación final: si alguno de estos conteos sale en 0, esa parte
-- no se cargó y el mensaje de error de arriba dice por qué.
SELECT
    (SELECT COUNT(*) FROM Sede)         AS sedes,
    (SELECT COUNT(*) FROM Trabajador)   AS trabajadores,
    (SELECT COUNT(*) FROM Natural)      AS personasNaturales,
    (SELECT COUNT(*) FROM Juridico)     AS personasJuridicas,
    (SELECT COUNT(*) FROM TipoMateria)  AS tiposMateria,
    (SELECT COUNT(*) FROM Materia)      AS materias,
    (SELECT COUNT(*) FROM Expediente)   AS expedientes,
    (SELECT COUNT(*) FROM Audiencia)    AS audiencias,
    (SELECT COUNT(*) FROM Notificacion) AS notificaciones;
GO
