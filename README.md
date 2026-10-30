# ConfiDen.Web (Fase 3 (v2: tipografía Poppins+Inter, menú móvil, buscadores, privacidad Ley 787))
Blazor Web App (.NET 10, Interactive Server) + EF Core + SQL Server + BCrypt.

1. Instalar .NET 10 SDK y SQL Server LocalDB (incluido con Visual Studio).
2. Ajustar `ConnectionStrings:Default` en appsettings.json si usa otro servidor.
3. F5 en Visual Studio (perfil https, puerto 7210) o `dotnet run`. La base se crea y se llena sola.
4. Usuarios: admin@confiden.com / Admin123, recepcion@confiden.com / Recep123, odonto@confiden.com / Odonto123.

Si cambia el esquema de la base, borre la base ConfiDenDb en LocalDB para que se regenere.
Precios, horario (8:00-17:00) y contacto son de ejemplo.


## Novedades v2
- Contacto real (teléfono/WhatsApp 8411 1405, correo, ubicación 4MXW+WHX) centralizado en `Services/DatosClinica.cs`.
- Botón flotante de WhatsApp, página /contacto con mapa, calendario visual en /disponibilidad, /mi-cita para consultar o cancelar.
- Modo oscuro y tamaño de letra (se guardan en el navegador).
- Seguridad: bloqueo de 5 minutos tras 5 intentos fallidos, registro de accesos (/auditoria, solo administrador) y aviso si quedan contraseñas de prueba.
- La tabla de auditoría se crea sola al iniciar; no hace falta borrar la base.
- Redes sociales: aún no hay; cuando existan, agréguelas en el pie de `MainLayout.razor` y en `DatosClinica.cs`.

## Datos de ejemplo (v9)
- Al iniciar, si no hay pacientes, se cargan 10 pacientes ficticios con citas (pasadas, próximas y una cancelada), 5 consultas clínicas y 5 tratamientos. Las fechas son relativas a "hoy", así que siempre se ven vigentes.
- Si la base ya existe, no hace falta borrarla: los datos de ejemplo se agregan solos mientras la tabla de pacientes esté vacía.
- Para producción, elimine la línea `SembrarDemo(db)` en `Data/AppDb.cs`.
