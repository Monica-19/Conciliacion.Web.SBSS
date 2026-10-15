/* ================================================================
   SISTEMA DE GESTIÓN DE CONCILIACIÓN SBSS
   Base de datos completa (esquema original + modificaciones)
   Motor: SQL Server (T-SQL)
   ================================================================ */

-- Crea la base de datos si no existe, y fuerza el contexto a partir de
-- aquí. Sin esto, si la ventana de consulta de SSMS está apuntando a
-- "master" (o a cualquier otra base) en vez de la tuya, TODO este script
-- corre silenciosamente en el lugar equivocado — y luego los scripts
-- BD-SBSS-SP.sql / BD-SBSS-Seed.sql / BD-SBSS-Ubigeo.sql fallan con
-- "Invalid object name" porque las tablas nunca se crearon donde ellos
-- buscan. Ajusta el nombre aquí si prefieres otro para tu base de datos.
IF DB_ID(N'SBSS_Conciliacion') IS NULL
BEGIN
    CREATE DATABASE SBSS_Conciliacion;
END
GO

USE SBSS_Conciliacion;
GO

-- ============================================================
-- 1. UBICACIÓN GEOGRÁFICA
-- ============================================================

CREATE TABLE Departamento (
    idDepartamento INT PRIMARY KEY IDENTITY(1,1),
    nombre VARCHAR(100) NOT NULL
);

CREATE TABLE Provincia (
    idProvincia INT PRIMARY KEY IDENTITY(1,1),
    idDepartamento INT NOT NULL,
    nombre VARCHAR(100) NOT NULL,
    FOREIGN KEY (idDepartamento) REFERENCES Departamento(idDepartamento)
);

CREATE TABLE Distrito (
    idDistrito INT PRIMARY KEY IDENTITY(1,1),
    idProvincia INT NOT NULL,
    nombre VARCHAR(100) NOT NULL,
    FOREIGN KEY (idProvincia) REFERENCES Provincia(idProvincia)
);

CREATE TABLE Direccion (
    idDireccion INT PRIMARY KEY IDENTITY(1,1),
    idDistrito INT NOT NULL,
    tipoVia VARCHAR(50),
    nombreVia VARCHAR(100),
    numero VARCHAR(20),
    manzana VARCHAR(20),
    lote VARCHAR(20),
    urbanizacion VARCHAR(100),
    referencia VARCHAR(MAX),
    codigoPostal VARCHAR(10),
    FOREIGN KEY (idDistrito) REFERENCES Distrito(idDistrito)
);

-- ============================================================
-- 2. SEDES, PERSONAS Y USUARIOS
-- ============================================================

CREATE TABLE Sede (
    idSede INT PRIMARY KEY IDENTITY(1,1),
    nombre VARCHAR(100) NOT NULL,
    descripcion VARCHAR(MAX),
    estadoSede VARCHAR(50)
);

CREATE TABLE Personas (
    idPersona INT PRIMARY KEY IDENTITY(1,1),
    idSede INT,
    celularP VARCHAR(20),
    correoP VARCHAR(100),
    whatsappP VARCHAR(20),
    estadoPersona VARCHAR(50),
    FOREIGN KEY (idSede) REFERENCES Sede(idSede)
);

CREATE TABLE DetalleDireccion (
    idPersona INT NOT NULL,
    idDireccion INT NOT NULL,
    esPrincipal BIT DEFAULT 0,
    PRIMARY KEY (idPersona, idDireccion),
    FOREIGN KEY (idPersona) REFERENCES Personas(idPersona),
    FOREIGN KEY (idDireccion) REFERENCES Direccion(idDireccion)
);

CREATE TABLE Juridico (
    idPersonaJuridica INT PRIMARY KEY,
    ruc VARCHAR(11) UNIQUE NOT NULL,
    razonSocial VARCHAR(150) NOT NULL,
    FOREIGN KEY (idPersonaJuridica) REFERENCES Personas(idPersona)
);

CREATE TABLE Representante (
    idRepresentante INT PRIMARY KEY IDENTITY(1,1),
    idPersonaJuridica INT NOT NULL,
    dniRepresentante VARCHAR(15),
    nombreRepresentante VARCHAR(100),
    apellidoPaterRepresentante VARCHAR(100),
    apellidoMaterRepresentante VARCHAR(100),
    cargo VARCHAR(100),
    FOREIGN KEY (idPersonaJuridica) REFERENCES Juridico(idPersonaJuridica)
);

CREATE TABLE Natural (
    idPersonaNatural INT PRIMARY KEY,
    dniPersonaN VARCHAR(15) UNIQUE NOT NULL,
    nombrePersonaN VARCHAR(100) NOT NULL,
    apellidoPaternoPersonaN VARCHAR(100) NOT NULL,
    apellidoMaternoPersonaN VARCHAR(100) NOT NULL,
    FOREIGN KEY (idPersonaNatural) REFERENCES Personas(idPersona)
);

CREATE TABLE Apoderado (
    idApoderado INT PRIMARY KEY IDENTITY(1,1),
    idPersonaNatural INT NOT NULL,
    dni_apoderado VARCHAR(15),
    nombre_apoderado VARCHAR(100),
    apePater_apoderado VARCHAR(100),
    apeMater_apoderado VARCHAR(100),
    FOREIGN KEY (idPersonaNatural) REFERENCES Natural(idPersonaNatural)
);

CREATE TABLE Trabajador (
    idTrabajador INT PRIMARY KEY IDENTITY(1,1),
    idSedeRef INT,
    dniTrabajador VARCHAR(15) UNIQUE NOT NULL,
    nombreTrabajador VARCHAR(100) NOT NULL,
    apePaterTrabajador VARCHAR(100) NOT NULL,
    apeMaterTrabajador VARCHAR(100) NOT NULL,
    emailTrabajador VARCHAR(100),
    telefonoTrabajador VARCHAR(20),
    estadoTrabajador VARCHAR(50),
    registroCivilComercial VARCHAR(100),
    FOREIGN KEY (idSedeRef) REFERENCES Sede(idSede)
);

CREATE TABLE Usuario (
    idUsuario INT PRIMARY KEY IDENTITY(1,1),
    idTrabajador INT NOT NULL,
    usuario VARCHAR(50) UNIQUE NOT NULL,
    contrasenia VARCHAR(255) NOT NULL,
    rol VARCHAR(50),
    FOREIGN KEY (idTrabajador) REFERENCES Trabajador(idTrabajador)
);

CREATE TABLE PasswordResetToken (
    id INT PRIMARY KEY IDENTITY(1,1),
    idUsuario INT NOT NULL,
    token VARCHAR(255) NOT NULL,
    fechaExpiracion DATETIME NOT NULL,
    esUsado BIT DEFAULT 0,
    FOREIGN KEY (idUsuario) REFERENCES Usuario(idUsuario)
);

-- ============================================================
-- 3. MESAS DE PARTES Y MATERIAS
-- ============================================================

CREATE TABLE Mesa_de_Partes (
    idMesaPartes INT PRIMARY KEY IDENTITY(1,1),
    nombreMP VARCHAR(100) NOT NULL,
    descriptionMP VARCHAR(MAX)
);

CREATE TABLE TipoMateria (
    idTipoMateria INT PRIMARY KEY IDENTITY(1,1),
    nombreTipoMateria VARCHAR(100) NOT NULL,
    descripcionTipoMateria VARCHAR(MAX)
);

CREATE TABLE Materia (
    idMateria INT PRIMARY KEY IDENTITY(1,1),
    idTipoMateria INT NOT NULL,
    nombreMateria VARCHAR(100) NOT NULL,
    descripcionMateria VARCHAR(MAX),
    FOREIGN KEY (idTipoMateria) REFERENCES TipoMateria(idTipoMateria)
);

CREATE TABLE DetalleMateria (
    idMesaPartes INT NOT NULL,
    idMateria INT NOT NULL,
    fechaDtMateria DATETIME,
    descripcionDtMateria VARCHAR(MAX),
    obligatorioDtMateria BIT DEFAULT 0,
    PRIMARY KEY (idMesaPartes, idMateria),
    FOREIGN KEY (idMesaPartes) REFERENCES Mesa_de_Partes(idMesaPartes),
    FOREIGN KEY (idMateria) REFERENCES Materia(idMateria)
);

-- ============================================================
-- 4. EXPEDIENTE Y RELACIONES
-- ============================================================

CREATE TABLE Expediente (
    idExpediente INT PRIMARY KEY IDENTITY(1,1),
    idConciliador INT,
    idTrabajador INT,
    idSede INT,
    numeroExpediente VARCHAR(50) UNIQUE NOT NULL,
    estadoProceso VARCHAR(50),
    fechaRecepcion DATETIME,
    fechaNotificacion DATETIME,
    fechaVencimiento DATETIME,
    fechaCierre DATETIME,
    FOREIGN KEY (idConciliador) REFERENCES Trabajador(idTrabajador),
    FOREIGN KEY (idTrabajador) REFERENCES Trabajador(idTrabajador),
    FOREIGN KEY (idSede) REFERENCES Sede(idSede)
);

CREATE TABLE DetallePersonaExpediente (
    idExpediente INT NOT NULL,
    idPersona INT NOT NULL,
    rolParticipante VARCHAR(50),
    PRIMARY KEY (idExpediente, idPersona),
    FOREIGN KEY (idExpediente) REFERENCES Expediente(idExpediente),
    FOREIGN KEY (idPersona) REFERENCES Personas(idPersona)
);

CREATE TABLE DetalleMateriaExpediente (
    idMateria INT NOT NULL,
    idExpediente INT NOT NULL,
    descripcionDtMateria VARCHAR(MAX),
    PRIMARY KEY (idMateria, idExpediente),
    FOREIGN KEY (idMateria) REFERENCES Materia(idMateria),
    FOREIGN KEY (idExpediente) REFERENCES Expediente(idExpediente)
);

CREATE TABLE DetalleMesaPartes (
    idMesaPartes INT NOT NULL,
    idExpediente INT NOT NULL,
    descripcionMesaPartes VARCHAR(MAX),
    fechaRegistroMesaPartes DATETIME,
    PRIMARY KEY (idMesaPartes, idExpediente),
    FOREIGN KEY (idMesaPartes) REFERENCES Mesa_de_Partes(idMesaPartes),
    FOREIGN KEY (idExpediente) REFERENCES Expediente(idExpediente)
);

CREATE TABLE Solicitud (
    idSolicitud INT PRIMARY KEY IDENTITY(1,1),
    idExpediente INT NOT NULL,
    hechosConflicto VARCHAR(MAX),
    otrasPersonasDerechoAliment VARCHAR(MAX),
    fechaSolicitud DATETIME,
    estadoSolicitud VARCHAR(50),
    FOREIGN KEY (idExpediente) REFERENCES Expediente(idExpediente)
);

-- ============================================================
-- 5. DOCUMENTOS Y ESPECIALIZACIONES
-- ============================================================

CREATE TABLE Documentos (
    idTipoDocumentos INT PRIMARY KEY IDENTITY(1,1),
    nombre VARCHAR(100) NOT NULL
);

CREATE TABLE DocumentoGenerado (
    idDocumento INT PRIMARY KEY IDENTITY(1,1),
    idTipoDocumentos INT NOT NULL,
    idExpediente INT NOT NULL,
    fechaGenerado DATETIME,
    FOREIGN KEY (idTipoDocumentos) REFERENCES Documentos(idTipoDocumentos),
    FOREIGN KEY (idExpediente) REFERENCES Expediente(idExpediente)
);

CREATE TABLE Anexo (
    idDocumento INT PRIMARY KEY,
    nombreArchivo VARCHAR(150),
    rutaArchivo VARCHAR(255),
    FOREIGN KEY (idDocumento) REFERENCES DocumentoGenerado(idDocumento)
);

CREATE TABLE Audiencia (
    idDocumento INT PRIMARY KEY,
    idConciliador INT,
    numeroSesion INT NOT NULL DEFAULT 1,
    fecha DATE,
    hora TIME,
    estado VARCHAR(50),
    resultado VARCHAR(50),
    observaciones VARCHAR(MAX),
    FOREIGN KEY (idDocumento) REFERENCES DocumentoGenerado(idDocumento),
    FOREIGN KEY (idConciliador) REFERENCES Trabajador(idTrabajador)
);

CREATE TABLE ActaConciliacion (
    idDocumento INT PRIMARY KEY,
    descripcionControversia VARCHAR(MAX),
    acuerdoConciliatorio VARCHAR(MAX),
    FOREIGN KEY (idDocumento) REFERENCES DocumentoGenerado(idDocumento)
);

CREATE TABLE ActaNotificacion (
    idDocumento INT PRIMARY KEY,
    fechaNotificacion DATE,
    horaNotificacion TIME,
    FOREIGN KEY (idDocumento) REFERENCES DocumentoGenerado(idDocumento)
);

-- ============================================================
-- 6. ARCHIVOS ADJUNTOS (documentos, videos, imágenes, audio)
-- ============================================================

CREATE TABLE ArchivoAdjunto (
    idArchivo       INT PRIMARY KEY IDENTITY(1,1),
    idExpediente    INT NOT NULL,
    idTrabajador    INT NOT NULL,
    tipoArchivo     VARCHAR(20) NOT NULL,   -- DOCUMENTO, VIDEO, IMAGEN, AUDIO
    nombreArchivo   VARCHAR(150) NOT NULL,
    rutaArchivo     VARCHAR(255) NOT NULL,
    tamanioKB       INT NULL,
    fechaSubida     DATETIME NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (idExpediente) REFERENCES Expediente(idExpediente),
    FOREIGN KEY (idTrabajador) REFERENCES Trabajador(idTrabajador)
);

-- ============================================================
-- 7. NOTIFICACIONES EN TIEMPO REAL (SignalR) Y ALERTAS
-- ============================================================

CREATE TABLE Notificacion (
    idNotificacion    INT PRIMARY KEY IDENTITY(1,1),
    idUsuario         INT NOT NULL,
    idExpediente      INT NULL,
    tipoNotificacion  VARCHAR(50) NOT NULL,  -- VENCIMIENTO, AUDIENCIA, ASIGNACION, SISTEMA
    mensaje           VARCHAR(500) NOT NULL,
    leida             BIT NOT NULL DEFAULT 0,
    fechaCreacion     DATETIME NOT NULL DEFAULT GETDATE(),
    fechaLectura      DATETIME NULL,
    FOREIGN KEY (idUsuario) REFERENCES Usuario(idUsuario),
    FOREIGN KEY (idExpediente) REFERENCES Expediente(idExpediente)
);

-- ============================================================
-- 8. ÍNDICES PARA DASHBOARD Y REPORTES
-- ============================================================

CREATE INDEX IX_Expediente_Estado ON Expediente(estadoProceso);
CREATE INDEX IX_Expediente_FechaRecepcion ON Expediente(fechaRecepcion);
CREATE INDEX IX_Audiencia_Fecha ON Audiencia(fecha);
