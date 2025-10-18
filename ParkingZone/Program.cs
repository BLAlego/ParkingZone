using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using ParkingZone.Data;
using ParkingZone.Services.Vision;   // Servicio OpenCV + Tesseract

var builder = WebApplication.CreateBuilder(args);

// DbContext → SQL Server
builder.Services.AddDbContext<ParkingZoneContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("ParkingZone")));

// Sesión en memoria
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

// Cookie Auth
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

// Servicio de visión (OpenCV + Tesseract)
builder.Services.AddSingleton<PlateRecognizer>();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseSession();
app.UseAuthorization();

app.MapRazorPages();

// API: recibe imagen (multipart/form-data "image") y devuelve placa + dígitos
app.MapPost("/api/plate", async (HttpRequest req, PlateRecognizer recognizer, CancellationToken ct) =>
{
    if (!req.HasFormContentType)
        return Results.BadRequest("Content-Type multipart/form-data requerido");

    var form = await req.ReadFormAsync(ct);
    var file = form.Files.GetFile("image");
    if (file is null || file.Length == 0)
        return Results.BadRequest("Falta 'image'");

    using var ms = new MemoryStream();
    await file.CopyToAsync(ms, ct);
    var bytes = ms.ToArray();

    // Usa la versión del Process que devuelve (plate, digits, confidence, rect)
    var (plate, digits, confidence, rect) = recognizer.Process(bytes);

    return Results.Ok(new
    {
        plate,        // Ej.: "ABC1234" o "1234ABC" (compacto, sin separadores)
        digits,       // Solo los 4 números (lo que pediste)
        confidence,   // 0.9 si patrón válido, 0.6 si fallback, 0 si nada
        box = rect is null ? null : new { rect.Value.X, rect.Value.Y, rect.Value.Width, rect.Value.Height }
    });
});

app.Run();