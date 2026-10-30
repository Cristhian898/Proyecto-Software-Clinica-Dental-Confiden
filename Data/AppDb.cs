using ConfiDen.Web.Services;
using ConfiDen.Web.Models;
using Microsoft.EntityFrameworkCore;
namespace ConfiDen.Web.Data;

public class AppDb(DbContextOptions<AppDb> o) : DbContext(o)
{
    public DbSet<Rol> Roles => Set<Rol>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Paciente> Pacientes => Set<Paciente>();
    public DbSet<Odontologo> Odontologos => Set<Odontologo>();
    public DbSet<Servicio> Servicios => Set<Servicio>();
    public DbSet<Cita> Citas => Set<Cita>();
    public DbSet<HistorialClinico> Historiales => Set<HistorialClinico>();
    public DbSet<Tratamiento> Tratamientos => Set<Tratamiento>();
    public DbSet<AccesoAuditoria> Accesos => Set<AccesoAuditoria>();

    protected override void OnModelCreating(ModelBuilder m)
    {
        m.Entity<Usuario>().HasKey(x => x.IdUsuario);
        m.Entity<Paciente>().HasKey(x => x.IdPaciente);
        m.Entity<Odontologo>().HasKey(x => x.IdOdontologo);
        m.Entity<Servicio>().HasKey(x => x.IdServicio);
        m.Entity<Usuario>().HasOne(u => u.Rol).WithMany().HasForeignKey(u => u.IdRol); // IdRol es la llave foránea hacia Rol
        m.Entity<Usuario>().HasIndex(x => x.Correo).IsUnique();
        m.Entity<Paciente>().HasIndex(x => x.Cedula).IsUnique();                 // RD-02
        m.Entity<Odontologo>().HasIndex(x => x.NumeroColegiado).IsUnique();      // RD-03
        // RN-01 / RN-05: sin doble cita activa por odontólogo; las canceladas liberan el horario
        m.Entity<Cita>().HasIndex(x => new { x.OdontologoId, x.FechaHora }).IsUnique().HasFilter("[Estado] <> 'Cancelada'");
        m.Entity<AccesoAuditoria>().ToTable("AccesosAuditoria");
        m.Entity<Servicio>().Property(x => x.Precio).HasPrecision(10, 2);
        m.Entity<HistorialClinico>().HasOne<Paciente>().WithMany().HasForeignKey(x => x.PacienteId);
        m.Entity<Tratamiento>().HasOne<Paciente>().WithMany().HasForeignKey(x => x.PacienteId);
        m.Entity<Tratamiento>().HasOne<HistorialClinico>().WithMany().HasForeignKey(x => x.HistorialId);
        foreach (var fk in m.Model.GetEntityTypes().SelectMany(t => t.GetForeignKeys()))
            fk.DeleteBehavior = DeleteBehavior.Restrict;
    }
}

public static class Seed
{
    public static void Run(AppDb db)
    {
        db.Database.EnsureCreated(); // Para producción: usar migraciones (dotnet ef migrations add Inicial)
        // Crea la tabla de auditoría si la base ya existía sin ella (no hace falta borrar la base)
        db.Database.ExecuteSqlRaw(@"IF OBJECT_ID(N'dbo.AccesosAuditoria', N'U') IS NULL
CREATE TABLE dbo.AccesosAuditoria (Id uniqueidentifier NOT NULL PRIMARY KEY, Fecha datetime2 NOT NULL, Usuario nvarchar(100) NOT NULL, Accion nvarchar(200) NOT NULL, PacienteId uniqueidentifier NULL)");
        if (!db.Roles.Any()) SembrarBase(db);
        if (!db.Pacientes.Any()) SembrarDemo(db); // datos de ejemplo (borrar este bloque en producción)
    }

    static void SembrarBase(AppDb db)
    {
        db.Roles.AddRange(
            new Rol { IdRol = "ADM", NombreRol = "Administrador", Descripcion = "Gestiona usuarios, odontólogos y servicios" },
            new Rol { IdRol = "REC", NombreRol = "Recepcionista", Descripcion = "Gestiona pacientes y citas" },
            new Rol { IdRol = "ODO", NombreRol = "Odontólogo", Descripcion = "Accede a información clínica" });
        var o1 = new Odontologo { Nombre = "Dr. Carlos Mendoza", Especialidad = "Odontología general", NumeroColegiado = "COL-1001" };
        var o2 = new Odontologo { Nombre = "Dra. Lucía Rivas", Especialidad = "Ortodoncia", NumeroColegiado = "COL-1002" };
        db.Odontologos.AddRange(o1, o2);
        db.Servicios.AddRange(
            new Servicio { NombreServicio = "Limpieza dental", Descripcion = "Profilaxis y remoción de sarro.", Precio = 500 },
            new Servicio { NombreServicio = "Restauraciones (calzas)", Descripcion = "Reparación de piezas con caries.", Precio = 800 },
            new Servicio { NombreServicio = "Prótesis fijas", Descripcion = "Coronas y puentes a medida.", Precio = 4500 },
            new Servicio { NombreServicio = "Ortodoncia", Descripcion = "Brackets y control mensual.", Precio = 1800 });
        db.Usuarios.AddRange(
            new Usuario { Correo = "admin@confiden.com", IdRol = "ADM", ContraseniaHash = BCrypt.Net.BCrypt.HashPassword("Admin123") },
            new Usuario { Correo = "recepcion@confiden.com", IdRol = "REC", ContraseniaHash = BCrypt.Net.BCrypt.HashPassword("Recep123") },
            new Usuario { Correo = "odonto@confiden.com", IdRol = "ODO", OdontologoId = o1.IdOdontologo, ContraseniaHash = BCrypt.Net.BCrypt.HashPassword("Odonto123") });
        db.SaveChanges();
    }

    // Día laborable más cercano a hoy ± n días (las citas solo se agendan de lunes a viernes)
    static DateTime Lab(int n, int hora, int min = 0)
    {
        var d = DateTime.Today.AddDays(n);
        var paso = n >= 0 ? 1 : -1;
        while (!Horario.EsLaborable(d)) d = d.AddDays(paso);
        return d.AddHours(hora).AddMinutes(min);
    }

    /// <summary>Datos ficticios para probar el sistema. Cédulas y teléfonos son inventados.</summary>
    static void SembrarDemo(AppDb db)
    {
        var od = db.Odontologos.OrderBy(o => o.NumeroColegiado).ToList();
        var sv = db.Servicios.ToList();
        if (od.Count < 2 || sv.Count == 0) return;
        Servicio S(string clave) => sv.FirstOrDefault(x => x.NombreServicio.Contains(clave, StringComparison.OrdinalIgnoreCase)) ?? sv[0];
        var mendoza = od[0]; var rivas = od[1];

        var pac = new[]
        {
            new Paciente { NombreCompleto = "María Fernanda López", Cedula = "001-150390-0012A", Telefono = "88123456", Direccion = "Barrio Altagracia, de la iglesia 2 cuadras al sur, Managua", FechaRegistro = DateTime.Today.AddDays(-120) },
            new Paciente { NombreCompleto = "José Antonio Martínez", Cedula = "001-220885-0034B", Telefono = "87234567", Direccion = "Residencial Las Colinas, casa 14, Managua", FechaRegistro = DateTime.Today.AddDays(-95) },
            new Paciente { NombreCompleto = "Ana Lucía Gutiérrez", Cedula = "001-030998-0056C", Telefono = "55345678", Direccion = "Reparto San Juan, frente al parque, Managua", FechaRegistro = DateTime.Today.AddDays(-80) },
            new Paciente { NombreCompleto = "Carlos Eduardo Pérez", Cedula = "001-171076-0078D", Telefono = "88456789", Direccion = "Villa Fontana, del Hospital Militar 1 cuadra al lago", FechaRegistro = DateTime.Today.AddDays(-70) },
            new Paciente { NombreCompleto = "Sofía Isabel Hernández", Cedula = "001-090702-0090E", Telefono = "78567890", Direccion = "Ciudad Jardín, casa G-22, Managua", FechaRegistro = DateTime.Today.AddDays(-55) },
            new Paciente { NombreCompleto = "Luis Alberto Rodríguez", Cedula = "001-281294-0102F", Telefono = "87678901", Direccion = "Colonia Centroamérica, Calle Principal, Managua", FechaRegistro = DateTime.Today.AddDays(-40) },
            new Paciente { NombreCompleto = "Daniela Alejandra Castillo", Cedula = "001-140600-0114G", Telefono = "55789012", Direccion = "Bello Horizonte, del Rotonda 3 cuadras arriba", FechaRegistro = DateTime.Today.AddDays(-30) },
            new Paciente { NombreCompleto = "Roberto Javier Sánchez", Cedula = "001-050568-0126H", Telefono = "88890123", Direccion = "Barrio Monseñor Lezcano, casa 8, Managua", FechaRegistro = DateTime.Today.AddDays(-21) },
            new Paciente { NombreCompleto = "Valeria Nicole Ramírez", Cedula = "001-261011-0138J", Telefono = "78901234", Direccion = "Los Robles, Edificio Torre 2, apto. 3B, Managua", FechaRegistro = DateTime.Today.AddDays(-12) },
            new Paciente { NombreCompleto = "Miguel Ángel Torres", Cedula = "001-190372-0150K", Telefono = "87012345", Direccion = "Barrio Ducualí, de la farmacia 1 cuadra abajo, Managua", FechaRegistro = DateTime.Today.AddDays(-5) },
        };
        db.Pacientes.AddRange(pac);

        // Citas pasadas (Realizada), próximas (Confirmada / Solicitada) y una Cancelada. Horas distintas por odontólogo.
        db.Citas.AddRange(
            new Cita { Paciente = pac[0], Odontologo = mendoza, Servicio = S("limpieza"), FechaHora = Lab(-14, 8, 30), Estado = "Realizada", TipoAtencion = "Consulta" },
            new Cita { Paciente = pac[1], Odontologo = mendoza, Servicio = S("restaur"), FechaHora = Lab(-10, 9, 0), Estado = "Realizada", TipoAtencion = "Tratamiento" },
            new Cita { Paciente = pac[2], Odontologo = rivas, Servicio = S("ortodoncia"), FechaHora = Lab(-9, 10, 0), Estado = "Realizada", TipoAtencion = "Consulta" },
            new Cita { Paciente = pac[3], Odontologo = mendoza, Servicio = S("tesis"), FechaHora = Lab(-7, 10, 30), Estado = "Realizada", TipoAtencion = "Consulta" },
            new Cita { Paciente = pac[4], Odontologo = rivas, Servicio = S("ortodoncia"), FechaHora = Lab(-4, 14, 0), Estado = "Realizada", TipoAtencion = "Control" },
            new Cita { Paciente = pac[5], Odontologo = mendoza, Servicio = S("limpieza"), FechaHora = Lab(-3, 11, 0), Estado = "Cancelada", TipoAtencion = "Consulta" },
            new Cita { Paciente = pac[0], Odontologo = rivas, Servicio = S("ortodoncia"), FechaHora = Lab(1, 8, 0), Estado = "Confirmada", TipoAtencion = "Consulta" },
            new Cita { Paciente = pac[6], Odontologo = mendoza, Servicio = S("limpieza"), FechaHora = Lab(1, 9, 30), Estado = "Confirmada", TipoAtencion = "Consulta" },
            new Cita { Paciente = pac[7], Odontologo = mendoza, Servicio = S("restaur"), FechaHora = Lab(2, 11, 30), Estado = "Confirmada", TipoAtencion = "Tratamiento" },
            new Cita { Paciente = pac[2], Odontologo = rivas, Servicio = S("ortodoncia"), FechaHora = Lab(3, 15, 0), Estado = "Confirmada", TipoAtencion = "Control" },
            new Cita { Paciente = pac[8], Odontologo = rivas, Servicio = S("limpieza"), FechaHora = Lab(4, 10, 30), Estado = "Solicitada", TipoAtencion = "Consulta" },
            new Cita { Paciente = pac[9], Odontologo = mendoza, Servicio = S("tesis"), FechaHora = Lab(5, 14, 30), Estado = "Solicitada", TipoAtencion = "Consulta" },
            new Cita { Paciente = pac[1], Odontologo = mendoza, Servicio = S("restaur"), FechaHora = Lab(8, 16, 0), Estado = "Confirmada", TipoAtencion = "Control" },
            new Cita { Paciente = pac[4], Odontologo = rivas, Servicio = S("ortodoncia"), FechaHora = Lab(10, 9, 0), Estado = "Confirmada", TipoAtencion = "Control" });

        // Historial clínico y tratamientos
        var h1 = new HistorialClinico { PacienteId = pac[1].IdPaciente, OdontologoId = mendoza.IdOdontologo, FechaCreacion = DateTime.Today.AddDays(-10), MotivoConsulta = "Dolor al masticar en el lado derecho.", Diagnostico = "Caries profunda en pieza 46.", Observaciones = "Sensibilidad al frío. Se recomienda restauración." };
        var h2 = new HistorialClinico { PacienteId = pac[2].IdPaciente, OdontologoId = rivas.IdOdontologo, FechaCreacion = DateTime.Today.AddDays(-9), MotivoConsulta = "Valoración para colocar brackets.", Diagnostico = "Apiñamiento dental moderado en arcada inferior.", Observaciones = "Se toman radiografías y modelos. Control mensual." };
        var h3 = new HistorialClinico { PacienteId = pac[3].IdPaciente, OdontologoId = mendoza.IdOdontologo, FechaCreacion = DateTime.Today.AddDays(-7), MotivoConsulta = "Pérdida de una muela hace meses.", Diagnostico = "Ausencia de pieza 36; espacio suficiente para puente fijo.", Observaciones = "Se explican opciones y costos." };
        var h4 = new HistorialClinico { PacienteId = pac[4].IdPaciente, OdontologoId = rivas.IdOdontologo, FechaCreacion = DateTime.Today.AddDays(-4), MotivoConsulta = "Control mensual de ortodoncia.", Diagnostico = "Alineación progresando según lo planificado.", Observaciones = "Se cambian ligas y se ajusta arco." };
        var h5 = new HistorialClinico { PacienteId = pac[0].IdPaciente, OdontologoId = mendoza.IdOdontologo, FechaCreacion = DateTime.Today.AddDays(-14), MotivoConsulta = "Limpieza dental de rutina.", Diagnostico = "Acumulación leve de sarro. Encías sanas.", Observaciones = "Se indica cepillado con técnica suave y uso de hilo dental." };
        db.Historiales.AddRange(h1, h2, h3, h4, h5);
        db.Tratamientos.AddRange(
            new Tratamiento { PacienteId = pac[1].IdPaciente, HistorialId = h1.Id, PiezaDental = "46", DescripcionProcedimiento = "Restauración (calza) en resina compuesta.", EstadoTratamiento = "Finalizado" },
            new Tratamiento { PacienteId = pac[2].IdPaciente, HistorialId = h2.Id, PiezaDental = "Arcadas", DescripcionProcedimiento = "Colocación de brackets metálicos y controles mensuales.", EstadoTratamiento = "En Proceso" },
            new Tratamiento { PacienteId = pac[3].IdPaciente, HistorialId = h3.Id, PiezaDental = "36", DescripcionProcedimiento = "Puente fijo de tres unidades (35-37).", EstadoTratamiento = "Planificado" },
            new Tratamiento { PacienteId = pac[4].IdPaciente, HistorialId = h4.Id, PiezaDental = "Arcadas", DescripcionProcedimiento = "Ortodoncia: ajuste de arco y cambio de ligas.", EstadoTratamiento = "En Proceso" },
            new Tratamiento { PacienteId = pac[0].IdPaciente, HistorialId = h5.Id, PiezaDental = "Todas", DescripcionProcedimiento = "Profilaxis y remoción de sarro.", EstadoTratamiento = "Finalizado" });
        db.SaveChanges();
    }
}
