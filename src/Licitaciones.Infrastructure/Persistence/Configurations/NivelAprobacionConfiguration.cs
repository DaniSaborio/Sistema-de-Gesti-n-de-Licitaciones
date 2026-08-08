using Licitaciones.Domain.NivelesAprobacion;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Licitaciones.Infrastructure.Persistence.Configurations;

public sealed class NivelAprobacionConfiguration : IEntityTypeConfiguration<NivelAprobacion>
{
    public void Configure(EntityTypeBuilder<NivelAprobacion> builder)
    {
        builder.ToTable("niveles_aprobacion", t => t.HasCheckConstraint(
            "ck_niveles_aprobacion_monto_minimo_positivo", "monto_minimo_crc > 0"));

        builder.HasKey(n => n.Id);
        builder.Property<uint>("xmin").HasColumnName("xmin").IsRowVersion();

        builder.Property(n => n.MontoMinimoCRC).HasColumnName("monto_minimo_crc").HasColumnType("numeric(18,2)").IsRequired();
        builder.Property(n => n.MontoMaximoCRC).HasColumnName("monto_maximo_crc").HasColumnType("numeric(18,2)");
        builder.Property(n => n.Aprobador).HasColumnName("aprobador").HasMaxLength(150).IsRequired();
        builder.Property(n => n.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(n => n.UpdatedAt).HasColumnName("updated_at").IsRequired();

        builder.HasIndex(n => n.MontoMinimoCRC).HasDatabaseName("ix_niveles_aprobacion_monto_minimo");

        // Datos semilla: rangos de ejemplo del enunciado (sección 8.7), sin traslape.
        var semilla = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        builder.HasData(
            new
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111101"),
                MontoMinimoCRC = 0.01m,
                MontoMaximoCRC = (decimal?)999_999.99m,
                Aprobador = "Encargado de área",
                CreatedAt = semilla,
                UpdatedAt = semilla,
            },
            new
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111102"),
                MontoMinimoCRC = 1_000_000.00m,
                MontoMaximoCRC = (decimal?)9_999_999.99m,
                Aprobador = "Gerencia",
                CreatedAt = semilla,
                UpdatedAt = semilla,
            },
            new
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111103"),
                MontoMinimoCRC = 10_000_000.00m,
                MontoMaximoCRC = (decimal?)null,
                Aprobador = "Junta Directiva",
                CreatedAt = semilla,
                UpdatedAt = semilla,
            });
    }
}
