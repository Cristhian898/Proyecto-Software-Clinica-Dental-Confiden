USE ConfiDenDb;
GO

-- Pacientes
SELECT NombreCompleto, Cedula, Telefono, Direccion, FechaRegistro
FROM Pacientes
ORDER BY FechaRegistro DESC;

-- Odontólogos
SELECT Nombre, Especialidad, NumeroColegiado, Disponible
FROM Odontologos;

-- Servicios
SELECT NombreServicio, Descripcion, Precio
FROM Servicios;

-- Usuarios y roles
SELECT u.Correo, r.NombreRol, u.Activo
FROM Usuarios u
JOIN Roles r ON r.IdRol = u.IdRol;

-- Citas con paciente, odontólogo y servicio
SELECT c.FechaHora, c.Estado, c.TipoAtencion,
       p.NombreCompleto AS Paciente,
       o.Nombre AS Odontologo,
       s.NombreServicio AS Servicio
FROM Citas c
JOIN Pacientes p ON p.IdPaciente = c.PacienteId
JOIN Odontologos o ON o.IdOdontologo = c.OdontologoId
JOIN Servicios s ON s.IdServicio = c.ServicioId
ORDER BY c.FechaHora;

-- Citas próximas (no canceladas)
SELECT c.FechaHora, c.Estado, p.NombreCompleto AS Paciente, o.Nombre AS Odontologo
FROM Citas c
JOIN Pacientes p ON p.IdPaciente = c.PacienteId
JOIN Odontologos o ON o.IdOdontologo = c.OdontologoId
WHERE c.FechaHora >= GETDATE() AND c.Estado <> 'Cancelada'
ORDER BY c.FechaHora;

-- Cantidad de citas por estado
SELECT Estado, COUNT(*) AS Total
FROM Citas
GROUP BY Estado;

-- Cantidad de citas por odontólogo
SELECT o.Nombre, COUNT(*) AS Total
FROM Citas c
JOIN Odontologos o ON o.IdOdontologo = c.OdontologoId
WHERE c.Estado <> 'Cancelada'
GROUP BY o.Nombre;

-- Historial clínico por paciente
SELECT p.NombreCompleto, h.FechaCreacion, h.MotivoConsulta, h.Diagnostico, h.Observaciones,
       o.Nombre AS Odontologo
FROM Historiales h
JOIN Pacientes p ON p.IdPaciente = h.PacienteId
LEFT JOIN Odontologos o ON o.IdOdontologo = h.OdontologoId
ORDER BY h.FechaCreacion DESC;

-- Tratamientos por paciente
SELECT p.NombreCompleto, t.PiezaDental, t.DescripcionProcedimiento, t.EstadoTratamiento
FROM Tratamientos t
JOIN Pacientes p ON p.IdPaciente = t.PacienteId
ORDER BY p.NombreCompleto;

-- Tratamientos sin finalizar
SELECT p.NombreCompleto, t.PiezaDental, t.DescripcionProcedimiento, t.EstadoTratamiento
FROM Tratamientos t
JOIN Pacientes p ON p.IdPaciente = t.PacienteId
WHERE t.EstadoTratamiento <> 'Finalizado';

-- Registro de accesos (auditoría)
SELECT TOP 50 Fecha, Usuario, Accion
FROM AccesosAuditoria
ORDER BY Fecha DESC;