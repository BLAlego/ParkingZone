using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using ParkingZone.Data;
using ParkingZone.Models;
using System.Globalization;
using System.IO;

var builder = WebApplication.CreateBuilder(args);

// -------------------- Servicios --------------------
builder.Services.AddDbContext<ParkingZoneContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("ParkingZone")));

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.Cookie.Name = builder.Configuration["Session:CookieName"] ?? ".ParkingZone.Session";
    options.IdleTimeout = TimeSpan.FromMinutes(
        int.TryParse(builder.Configuration["Session:IdleTimeoutMinutes"], out var min) ? min : 60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = builder.Configuration["Authentication:CookieName"] ?? ".ParkingZone.Auth";
        options.LoginPath = builder.Configuration["Authentication:LoginPath"] ?? "/Account/Login";
        options.LogoutPath = builder.Configuration["Authentication:LogoutPath"] ?? "/Account/Logout";
        options.AccessDeniedPath = builder.Configuration["Authentication:AccessDeniedPath"] ?? "/Account/Denied";
        options.SlidingExpiration = true;
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    });

builder.Services.AddRazorPages();

var app = builder.Build();

// -------------------- Middleware --------------------
app.UseHttpsRedirection();
app.UseStaticFiles();      // sirve /wwwroot (necesario para /uploads/scans/*)
app.UseRouting();

app.UseAuthentication();
app.UseSession();
app.UseAuthorization();

app.MapRazorPages();

// -------------------- API: guarda escaneo --------------------
// Recibe: multipart/form-data con "image" (obligatorio) y "plate", "digits", "confidence" (opcionales).
// Guarda la imagen en /wwwroot/uploads/scans y crea un registro en dbo.PlateScans.
app.MapPost("/api/plate", async (
    HttpRequest req,
    IWebHostEnvironment env,
    ParkingZoneContext db,
    CancellationToken ct) =>
{
    if (!req.HasFormContentType)
        return Results.BadRequest("Content-Type multipart/form-data requerido");

    var form = await req.ReadFormAsync(ct);

    // 1) Imagen
    var file = form.Files.GetFile("image");
    if (file is null || file.Length == 0)
        return Results.BadRequest("Falta 'image'");

    // 2) Datos OCR enviados por el navegador (opcionales)
    var plate = form["plate"].ToString();
    var digits = form["digits"].ToString();

    float confidence = 0f;
    if (float.TryParse(form["confidence"].ToString(),
        NumberStyles.Float, CultureInfo.InvariantCulture, out var c))
        confidence = c;

    // 3) Guardar imagen en wwwroot/uploads/scans
    string? relPath = null;
    try
    {
        var folder = Path.Combine(env.WebRootPath, "uploads", "scans");
        Directory.CreateDirectory(folder);
        var fileName = $"scan_{DateTime.UtcNow:yyyyMMdd_HHmmssfff}.png";
        var fullPath = Path.Combine(folder, fileName);

        using var ms = new MemoryStream();
        await file.CopyToAsync(ms, ct);
        await File.WriteAllBytesAsync(fullPath, ms.ToArray(), ct);

        relPath = $"/uploads/scans/{fileName}";
    }
    catch
    {
        // si falla guardado de imagen, seguimos sin ImagePath
    }

    // 4) Guardar fila en BD
    var entity = new PlateScan
    {
        Plate = string.IsNullOrWhiteSpace(plate) ? null : plate,
        Digits = string.IsNullOrWhiteSpace(digits) ? null : digits,
        Confidence = confidence,
        ImagePath = relPath
        // BoxX/BoxY/BoxW/BoxH quedan null (no se envían desde el front en esta opción)
    };

    db.PlateScans.Add(entity);
    await db.SaveChangesAsync(ct);

    // 5) Respuesta
    return Results.Ok(new
    {
        id = entity.Id,
        plate = entity.Plate,
        digits = entity.Digits,
        confidence = entity.Confidence,
        image = relPath
    });
});

app.Run();
