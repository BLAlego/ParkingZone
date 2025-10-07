using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using ParkingZone.Data;

var builder = WebApplication.CreateBuilder(args);

// DbContext ? SQL Server
builder.Services.AddDbContext<ParkingZoneContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("ParkingZone")));

// Usa memoria en vez de SQL para la sesión
builder.Services.AddDistributedMemoryCache(); // <-- cambia a memoria

builder.Services.AddSession(options =>
{
    options.Cookie.Name = builder.Configuration["Session:CookieName"] ?? ".ParkingZone.Session";
    options.IdleTimeout = TimeSpan.FromMinutes(
        int.TryParse(builder.Configuration["Session:IdleTimeoutMinutes"], out var min) ? min : 60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // recomendado
});

// Cookie Auth (igual que antes)
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

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseSession();       // sigue activo, ahora en memoria
app.UseAuthorization();

app.MapRazorPages();

app.Run();
