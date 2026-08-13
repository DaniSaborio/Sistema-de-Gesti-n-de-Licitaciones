using Licitaciones.Domain.Common;
using Licitaciones.Domain.Licitaciones;
using Licitaciones.Domain.Ofertas;
using Licitaciones.Domain.Proveedores;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Licitaciones.IntegrationTests;

/// <summary>Reloj fijo para pruebas de integración deterministas.</summary>
internal sealed class ClockDePrueba(DateTimeOffset momento) : IClock
{
    public DateTimeOffset UtcNow { get; } = momento;
}

[Collection("Postgres")]
public class MigracionesYRestriccionesTests(PostgresContainerFixture fixture)
{
    private static readonly IClock Reloj = new ClockDePrueba(new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero));

    [Fact]
    public async Task Las_migraciones_dejan_las_tablas_esperadas()
    {
        await using var db = fixture.CrearDbContext();

        var tablas = await db.Database
            .SqlQueryRaw<string>("SELECT table_name AS \"Value\" FROM information_schema.tables WHERE table_schema = 'public'")
            .ToListAsync();

        Assert.Contains("licitaciones", tablas);
        Assert.Contains("proveedores", tablas);
        Assert.Contains("ofertas", tablas);
        Assert.Contains("niveles_aprobacion", tablas);
        Assert.Contains("tipos_cambio", tablas);
    }

    [Fact]
    public async Task El_indice_unico_de_proveedor_normalizado_rechaza_duplicados_en_base_de_datos()
    {
        await using var db = fixture.CrearDbContext();

        db.Proveedores.Add(Proveedor.Crear("Índice Único S.A.", Reloj));
        await db.SaveChangesAsync();

        // Segundo DbContext para simular otro proceso que no ve el estado en memoria del primero,
        // forzando a que la unicidad real sea la del índice de PostgreSQL, no el tracker de EF.
        await using var db2 = fixture.CrearDbContext();
        db2.Proveedores.Add(Proveedor.Crear("índice   único s.a.", Reloj));

        await Assert.ThrowsAsync<DbUpdateException>(() => db2.SaveChangesAsync());
    }

    [Fact]
    public async Task El_indice_unico_compuesto_de_ofertas_impide_dos_ofertas_del_mismo_proveedor()
    {
        await using var db = fixture.CrearDbContext();
        var proveedor = Proveedor.Crear("Proveedor Ofertas Únicas", Reloj);
        var licitacion = Licitacion.Crear("LIC-INT-001", "Título", Reloj.UtcNow.AddDays(10), 1_000_000m, Reloj);
        licitacion.Publicar(Reloj);
        db.Proveedores.Add(proveedor);
        db.Licitaciones.Add(licitacion);
        await db.SaveChangesAsync();

        var primeraOferta = RegistroOfertaService.Registrar(licitacion, proveedor.Id, 500_000m, [], Reloj);
        db.Ofertas.Add(primeraOferta);
        await db.SaveChangesAsync();

        // Simula una condición de carrera: un segundo proceso no ve todavía la oferta
        // recién comprometida (lista de existentes vacía) y el dominio la deja pasar;
        // el índice único compuesto de PostgreSQL es la última línea de defensa (8.3).
        await using var db2 = fixture.CrearDbContext();
        var ofertaDuplicadaEnBd = RegistroOfertaService.Registrar(licitacion, proveedor.Id, 400_000m, [], Reloj);
        db2.Ofertas.Add(ofertaDuplicadaEnBd);

        await Assert.ThrowsAsync<DbUpdateException>(() => db2.SaveChangesAsync());
    }

    [Fact]
    public async Task No_se_puede_eliminar_fisicamente_un_proveedor_con_ofertas_relacionadas()
    {
        await using var db = fixture.CrearDbContext();
        var proveedor = Proveedor.Crear("Proveedor Con Ofertas", Reloj);
        var licitacion = Licitacion.Crear("LIC-INT-002", "Título", Reloj.UtcNow.AddDays(10), 1_000_000m, Reloj);
        licitacion.Publicar(Reloj);
        db.Proveedores.Add(proveedor);
        db.Licitaciones.Add(licitacion);
        await db.SaveChangesAsync();

        var oferta = RegistroOfertaService.Registrar(licitacion, proveedor.Id, 500_000m, [], Reloj);
        db.Ofertas.Add(oferta);
        await db.SaveChangesAsync();

        db.Proveedores.Remove(proveedor);

        // La FK ofertas -> proveedores está configurada con DeleteBehavior.Restrict (8.9): PostgreSQL
        // rechaza el borrado físico mientras existan ofertas relacionadas.
        await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
    }

    [Fact]
    public async Task La_restriccion_check_rechaza_presupuesto_no_positivo_a_nivel_de_base_de_datos()
    {
        await using var db = fixture.CrearDbContext();

        // Se inserta evitando el constructor de dominio (que ya valida esto) para
        // comprobar que la restricción CHECK de PostgreSQL es una defensa real,
        // no solo una validación de aplicación.
        await Assert.ThrowsAnyAsync<Exception>(async () =>
        {
            await db.Database.ExecuteSqlRawAsync(
                "INSERT INTO licitaciones (\"Id\", codigo, codigo_normalizado, titulo, estado, fecha_cierre, presupuesto_estimado_crc, created_at, updated_at) " +
                "VALUES (gen_random_uuid(), 'LIC-CHK', 'LIC-CHK', 'Título', 'Borrador', now() + interval '1 day', -100, now(), now())");
        });
    }

    [Fact]
    public async Task La_concurrencia_optimista_lanza_excepcion_cuando_el_registro_cambio_entre_lectura_y_escritura()
    {
        await using var dbSemilla = fixture.CrearDbContext();
        var proveedor = Proveedor.Crear("Proveedor Concurrencia", Reloj);
        dbSemilla.Proveedores.Add(proveedor);
        await dbSemilla.SaveChangesAsync();

        await using var dbLector1 = fixture.CrearDbContext();
        await using var dbLector2 = fixture.CrearDbContext();

        var copia1 = await dbLector1.Proveedores.SingleAsync(p => p.Id == proveedor.Id);
        var copia2 = await dbLector2.Proveedores.SingleAsync(p => p.Id == proveedor.Id);

        copia1.ActualizarNombre("Primer cambio", Reloj);
        await dbLector1.SaveChangesAsync();

        copia2.ActualizarNombre("Segundo cambio en conflicto", Reloj);
        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => dbLector2.SaveChangesAsync());
    }
}
