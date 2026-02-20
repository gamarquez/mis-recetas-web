using Microsoft.AspNetCore.Authentication.Cookies;
using MisRecetas.Web.Data;
using MisRecetas.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// ── MVC con Razor Views y Tag Helpers ─────────────────────────────────────
builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();

// ── Cookie Authentication ──────────────────────────────────────────────────
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccesoDenegado";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    });

// ── Authorization Policies ─────────────────────────────────────────────────
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("SoloAdmin", p => p.RequireRole("Administrador"));
    options.AddPolicy("AdminOAdministracion",
        p => p.RequireRole("Administrador", "Administracion"));
});

// ── TempData via Cookies ───────────────────────────────────────────────────
builder.Services.AddSession();

// ── Inyección de dependencias: DAL ─────────────────────────────────────────
builder.Services.AddScoped<DbConnectionFactory>();
builder.Services.AddScoped<UsuarioRepository>();
builder.Services.AddScoped<CatalogoRepository>();
builder.Services.AddScoped<MedicoRepository>();
builder.Services.AddScoped<PacienteRepository>();
builder.Services.AddScoped<RecetaRepository>();

// ── Inyección de dependencias: Services ───────────────────────────────────
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<RecetaService>();
builder.Services.AddScoped<MedicoService>();
builder.Services.AddScoped<PacienteService>();
builder.Services.AddScoped<UsuarioService>();
builder.Services.AddScoped<ReporteService>();

var app = builder.Build();

// ── Pipeline ───────────────────────────────────────────────────────────────
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();   // ANTES de Authorization
app.UseAuthorization();

app.UseSession();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dashboard}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
