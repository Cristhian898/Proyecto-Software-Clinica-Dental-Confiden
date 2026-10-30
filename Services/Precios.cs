using ConfiDen.Web.Models;
namespace ConfiDen.Web.Services;

public static class Precios
{
    /// <summary>Texto del precio. Las restauraciones (calzas) se cobran por cada calza.</summary>
    public static string Texto(Servicio s) =>
        $"C$ {s.Precio:N2}" + (s.NombreServicio.Contains("calza", StringComparison.OrdinalIgnoreCase) ? " por calza" : "");
}

public static class ServicioInfo
{
    static bool Tiene(Servicio s, params string[] claves) => claves.Any(c => s.NombreServicio.Contains(c, StringComparison.OrdinalIgnoreCase));

    /// <summary>Imagen del servicio (null si todavía no hay foto para ese servicio).</summary>
    public static string? Imagen(Servicio s) =>
        Tiene(s, "limpieza", "general") ? "img/servicios/limpieza.jpg" :
        Tiene(s, "ortodoncia") ? "img/servicios/ortodoncia.jpg" :
        Tiene(s, "prótesis", "protesis") ? "img/servicios/protesis.jpg" :
        Tiene(s, "restaur", "calza") ? "img/servicios/restauraciones.jpg" : null;

    /// <summary>Frase corta de la categoría del servicio.</summary>
    public static string? Frase(Servicio s) =>
        Tiene(s, "limpieza", "general") ? "La prevención es la base de una sonrisa duradera." :
        Tiene(s, "ortodoncia") ? "Alinear tus dientes es el primer paso hacia una mordida funcional y una estética impecable." :
        Tiene(s, "blanque") ? "Devuélvele el brillo natural a tus dientes de forma segura y profesional." :
        Tiene(s, "prótesis", "protesis", "restaur", "calza") ? "Recupera la funcionalidad de tu boca y la libertad de comer y sonreír sin temor." : null;
}
