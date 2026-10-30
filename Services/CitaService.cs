using ConfiDen.Web.Data;
using Microsoft.EntityFrameworkCore;
namespace ConfiDen.Web.Services;

public static class Horario
{
    // 8:00 a 16:30 cada 30 min (supuesto: lunes a viernes 8:00-17:00)
    public static bool EsLaborable(DateTime d) => d.DayOfWeek is not (DayOfWeek.Saturday or DayOfWeek.Sunday);
    public static readonly List<string> Horas = Enumerable.Range(0, 18).Select(i => TimeSpan.FromMinutes(480 + 30 * i).ToString(@"hh\:mm")).ToList();
}

public class CitaService(IDbContextFactory<AppDb> f)
{
    // RF-11: comprueba conflictos e ignora las citas canceladas
    public async Task<bool> EstaLibre(Guid odontologoId, DateTime fh, Guid? ignorar = null)
    {
        await using var db = await f.CreateDbContextAsync();
        return !await db.Citas.AnyAsync(c => c.OdontologoId == odontologoId && c.FechaHora == fh && c.Estado != "Cancelada" && c.Id != ignorar);
    }

    public async Task<Guid?> PrimerLibre(DateTime fh)
    {
        await using var db = await f.CreateDbContextAsync();
        var ids = await db.Odontologos.Where(o => o.Disponible).Select(o => o.IdOdontologo).ToListAsync();
        foreach (var id in ids) if (await EstaLibre(id, fh)) return id;
        return null;
    }
}
