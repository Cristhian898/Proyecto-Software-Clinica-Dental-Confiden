using System.Security.Claims;
using ConfiDen.Web.Components;
using ConfiDen.Web.Data;
using ConfiDen.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var b = WebApplication.CreateBuilder(args);
b.Services.AddRazorComponents().AddInteractiveServerComponents();
b.Services.AddDbContextFactory<AppDb>(o => o.UseSqlServer(b.Configuration.GetConnectionString("Default")));
b.Services.AddScoped<CitaService>();
b.Services.AddScoped<AuditoriaService>();
b.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(o =>
{
    o.LoginPath = "/login";
    o.AccessDeniedPath = "/login";
    o.ExpireTimeSpan = TimeSpan.FromMinutes(15); // RNF-04: expira sesiones inactivas
    o.SlidingExpiration = true;
    o.Cookie.HttpOnly = true;
});
b.Services.AddAuthorization();
b.Services.AddCascadingAuthenticationState();

var app = b.Build();
using (var s = app.Services.CreateScope())
{
    var f = s.ServiceProvider.GetRequiredService<IDbContextFactory<AppDb>>();
    using var db = f.CreateDbContext();
    Seed.Run(db);
}
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();
app.MapPost("/auth/login", async (HttpContext c, IDbContextFactory<AppDb> f, AuditoriaService aud) =>
{
    var form = await c.Request.ReadFormAsync();
    var correo = form["correo"].ToString().Trim().ToLower();
    var clave = form["clave"].ToString();
    if (Intentos.Bloqueado(correo)) { await aud.Registrar(correo, "Acceso bloqueado por intentos fallidos"); return Results.Redirect("/login?e=3"); }
    await using var db = await f.CreateDbContextAsync();
    var u = await db.Usuarios.Include(x => x.Rol).FirstOrDefaultAsync(x => x.Correo == correo);
    if (u == null || !BCrypt.Net.BCrypt.Verify(clave, u.ContraseniaHash))
    {
        Intentos.Fallo(correo, 5, TimeSpan.FromMinutes(5)); // 5 fallos = 5 minutos de bloqueo
        await aud.Registrar(correo, "Inicio de sesión fallido");
        return Results.Redirect("/login?e=1");
    }
    Intentos.Ok(correo);
    await aud.Registrar(correo, "Inició sesión");
    if (!u.Activo) return Results.Redirect("/login?e=2"); // RN-08
    var claims = new List<Claim> { new(ClaimTypes.Name, u.Correo), new(ClaimTypes.Role, u.Rol!.NombreRol) };
    if (u.OdontologoId != null) claims.Add(new Claim("odId", u.OdontologoId.ToString()!));
    await c.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
        new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme)));
    return Results.Redirect("/panel");
}).DisableAntiforgery();
app.MapGet("/logout", async (HttpContext c) => { await c.SignOutAsync(); return Results.Redirect("/"); });
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();
app.Run();
