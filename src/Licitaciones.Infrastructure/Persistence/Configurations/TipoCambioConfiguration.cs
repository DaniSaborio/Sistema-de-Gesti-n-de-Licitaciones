using Licitaciones.Domain.TiposCambio;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Licitaciones.Infrastructure.Persistence.Configurations;

public sealed class TipoCambioConfiguration : IEntityTypeConfiguration<TipoCambio>
{
    public void Configure(EntityTypeBuilder<TipoCambio> builder)
    {
        builder.ToTable("tipos_cambio", t => t.HasCheckConstraint(
            "ck_tipos_cambio_valor_positivo", "crc_por_usd > 0"));

        builder.HasKey(tc => tc.Id);
        builder.Property<uint>("xmin").HasColumnName("xmin").IsRowVersion();

        builder.Property(tc => tc.CRCporUSD).HasColumnName("crc_por_usd").HasColumnType("numeric(18,6)").IsRequired();
        builder.Property(tc => tc.FechaVigencia).HasColumnName("fecha_vigencia").IsRequired();
        builder.Property(tc => tc.Activo).HasColumnName("activo").IsRequired();
        builder.Property(tc => tc.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(tc => tc.UpdatedAt).HasColumnName("updated_at").IsRequired();

        // Garantiza a nivel de PostgreSQL que solo exista un tipo de cambio activo (8.8).
        builder.HasIndex(tc => tc.Activo)
            .IsUnique()
            .HasDatabaseName("ux_tipos_cambio_unico_activo")
            .HasFilter("\"activo\" = true");

        // Dato semilla: tipo de cambio inicial activo para operar sin depender de una API externa (8.8).
        var semilla = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        builder.HasData(new
        {
            Id = Guid.Parse("22222222-2222-2222-2222-222222222201"),
            CRCporUSD = 520.00m,
            FechaVigencia = semilla,
            Activo = true,
            CreatedAt = semilla,
            UpdatedAt = semilla,
        });
    }
}
