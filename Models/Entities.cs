using System.ComponentModel.DataAnnotations;
namespace ConfiDen.Web.Models;

public static class Estados
{
    public static readonly string[] Cita = { "Solicitada", "Confirmada", "Realizada", "Cancelada", "Reprogramada" };
    public static readonly string[] Trat = { "Planificado", "En Proceso", "Finalizado" };
}

public class Rol
{
    [Key, MaxLength(20)] public string IdRol { get; set; } = "";
    [Required, MaxLength(50)] public string NombreRol { get; set; } = "";
    [MaxLength(150)] public string? Descripcion { get; set; }
}

public class Usuario
{
    public Guid IdUsuario { get; set; } = Guid.NewGuid();
    [Required, MaxLength(100), RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", ErrorMessage = "Correo no válido.")]
    public string Correo { get; set; } = "";
    [MaxLength(60)] public string ContraseniaHash { get; set; } = "";
    public bool Activo { get; set; } = true;
    [MaxLength(20)] public string IdRol { get; set; } = "";
    public Rol? Rol { get; set; }
    public Guid? OdontologoId { get; set; }
}

public class Paciente
{
    public Guid IdPaciente { get; set; } = Guid.NewGuid();
    [Required(ErrorMessage = "Ingrese el nombre."), MaxLength(150), RegularExpression(@"^[A-Za-zÁÉÍÓÚÜÑáéíóúüñ ]+$", ErrorMessage = "Solo letras y espacios.")]
    public string NombreCompleto { get; set; } = "";
    [Required(ErrorMessage = "Ingrese la cédula."), MaxLength(16), RegularExpression(@"^[0-9]{3}-[0-9]{6}-[0-9]{4}[A-Z]$", ErrorMessage = "Formato 000-000000-0000X.")]
    public string Cedula { get; set; } = "";
    [Required(ErrorMessage = "Ingrese el teléfono."), MaxLength(8), RegularExpression(@"^[2578][0-9]{7}$", ErrorMessage = "8 dígitos, empezando con 2, 5, 7 u 8.")]
    public string Telefono { get; set; } = "";
    [Required(ErrorMessage = "Ingrese la dirección."), MaxLength(250)] public string Direccion { get; set; } = "";
    public DateTime FechaRegistro { get; set; } = DateTime.Today;
}

public class Odontologo
{
    public Guid IdOdontologo { get; set; } = Guid.NewGuid();
    [Required(ErrorMessage = "Ingrese el nombre."), MaxLength(150)] public string Nombre { get; set; } = "";
    [Required(ErrorMessage = "Ingrese la especialidad."), MaxLength(100)] public string Especialidad { get; set; } = "";
    [Required(ErrorMessage = "Ingrese el número de colegiado."), MaxLength(20)] public string NumeroColegiado { get; set; } = "";
    public bool Disponible { get; set; } = true;
}

public class Servicio
{
    public Guid IdServicio { get; set; } = Guid.NewGuid();
    [Required(ErrorMessage = "Ingrese el nombre."), MaxLength(100)] public string NombreServicio { get; set; } = "";
    [MaxLength(250)] public string? Descripcion { get; set; }
    [Range(0.01, 1000000, ErrorMessage = "El precio debe ser mayor que 0.")] public decimal Precio { get; set; } = 1;
}

public class Cita
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime FechaHora { get; set; }
    [MaxLength(30)] public string Estado { get; set; } = "Solicitada";
    [MaxLength(50)] public string TipoAtencion { get; set; } = "Consulta";
    public Guid PacienteId { get; set; }
    public Paciente? Paciente { get; set; }
    public Guid OdontologoId { get; set; }
    public Odontologo? Odontologo { get; set; }
    public Guid ServicioId { get; set; }
    public Servicio? Servicio { get; set; }
}

public class HistorialClinico
{
    public Guid Id { get; set; } = Guid.NewGuid();
    [Required(ErrorMessage = "Ingrese el motivo."), MaxLength(500)] public string MotivoConsulta { get; set; } = "";
    [Required(ErrorMessage = "Ingrese el diagnóstico."), MaxLength(1000)] public string Diagnostico { get; set; } = "";
    [MaxLength(500)] public string? Observaciones { get; set; }
    public DateTime FechaCreacion { get; set; } = DateTime.Today;
    public Guid PacienteId { get; set; }
    public Guid? OdontologoId { get; set; }
}

public class Tratamiento
{
    public Guid Id { get; set; } = Guid.NewGuid();
    [Required(ErrorMessage = "Ingrese la pieza."), MaxLength(10)] public string PiezaDental { get; set; } = "";
    [Required(ErrorMessage = "Ingrese el procedimiento."), MaxLength(300)] public string DescripcionProcedimiento { get; set; } = "";
    [MaxLength(30)] public string EstadoTratamiento { get; set; } = "Planificado";
    public Guid PacienteId { get; set; }
    public Guid HistorialId { get; set; }
}

public class AccesoAuditoria
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime Fecha { get; set; } = DateTime.Now;
    [MaxLength(100)] public string Usuario { get; set; } = "";
    [MaxLength(200)] public string Accion { get; set; } = "";
    public Guid? PacienteId { get; set; }
}
