using ConfiDen.Web.Data;
using ConfiDen.Web.Models;
using Microsoft.EntityFrameworkCore;
namespace ConfiDen.Web.Services;

/// <summary>Registra quién accede al sistema y a la información clínica.</summary>
public class AuditoriaService(IDbContextFactory<AppDb> f)
{
    public async Task Registrar(string usuario, string accion, Guid? pacienteId = null)
    {
        try
        {
            await using var db = await f.CreateDbContextAsync();
            db.Accesos.Add(new AccesoAuditoria
            {
                Usuario = usuario.Length > 100 ? usuario[..100] : usuario,
                Accion = accion,
                PacienteId = pacienteId
            });
            await db.SaveChangesAsync();
        }
        catch { /* la auditoría nunca debe tumbar la operación principal */ }
    }
}
