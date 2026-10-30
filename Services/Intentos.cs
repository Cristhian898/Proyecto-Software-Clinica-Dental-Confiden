using System.Collections.Concurrent;
namespace ConfiDen.Web.Services;

/// <summary>Bloqueo temporal por intentos fallidos (login del personal y consulta de citas).</summary>
public static class Intentos
{
    static readonly ConcurrentDictionary<string, (int n, DateTime hasta)> d = new();
    public static bool Bloqueado(string k) => d.TryGetValue(k, out var v) && v.hasta > DateTime.UtcNow;
    public static void Fallo(string k, int max, TimeSpan bloqueo) =>
        d.AddOrUpdate(k, _ => (1, DateTime.MinValue), (_, v) =>
        {
            var n = v.n + 1;
            return n >= max ? (0, DateTime.UtcNow + bloqueo) : (n, v.hasta);
        });
    public static void Ok(string k) => d.TryRemove(k, out _);
}
