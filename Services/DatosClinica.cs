namespace ConfiDen.Web.Services;

/// <summary>Datos de contacto de la clínica (un solo lugar para cambiarlos).</summary>
public static class DatosClinica
{
    public const string TelefonoLocal = "84111405";
    public const string TelefonoVisible = "8411 1405";
    public const string TelefonoLink = "tel:+50584111405";
    public const string Correo = "cm405103@gmail.com";
    public const string CorreoLink = "mailto:cm405103@gmail.com";
    public const string PlusCode = "4MXW+WHX";
    public const string Mapa = "https://www.google.com/maps/search/?api=1&query=4MXW%2BWHX%20Managua%2C%20Nicaragua";
    public const string MapaEmbed = "https://www.google.com/maps?q=4MXW%2BWHX%20Managua%2C%20Nicaragua&output=embed";
    public const string WaBase = "https://wa.me/50584111405";
    public static readonly string WaInfo = WaBase + "?text=" + Uri.EscapeDataString("Hola, quisiera información sobre una cita en ConfiDent.");
    public static string WaClinica(string texto) => WaBase + "?text=" + Uri.EscapeDataString(texto);
    public static string WaPaciente(string telefono8, string texto) => "https://wa.me/505" + telefono8 + "?text=" + Uri.EscapeDataString(texto);
}
