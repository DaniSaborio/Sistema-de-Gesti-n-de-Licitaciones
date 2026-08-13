using Licitaciones.Api;
using Licitaciones.Application;
using Licitaciones.Infrastructure;
using Licitaciones.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApiModule();
builder.Services.AddHealthChecks()
    .AddDbContextCheck<LicitacionesDbContext>("postgresql");

var app = builder.Build();

// Migra la base de datos automáticamente al iniciar, para que
// `docker compose up --build` y el despliegue en Kubernetes queden
// reproducibles sin pasos manuales (secciones 13.1 y 17.2).
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<LicitacionesDbContext>();
    dbContext.Database.Migrate();
}

// El manejador de excepciones se activa siempre (no solo en producción) para
// que las rutas /api nunca filtren stack traces, incluso en Development.
app.UseExceptionHandler("/Home/Error");
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();
app.MapStaticAssets();

app.UseApiModule();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapHealthChecks("/health/live");
app.MapHealthChecks("/health/ready");

app.Run();

/// <summary>Punto de entrada visible para WebApplicationFactory en las pruebas de integración/funcionales.</summary>
public partial class Program;
