using Licitaciones.Domain.Licitaciones;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Licitaciones.Infrastructure.Persistence.Configurations;

public sealed class LicitacionConfiguration : IEntityTypeConfiguration<Licitacion>
{
    public void Configure(EntityTypeBuilder<Licitacion> builder)
    {
        builder.ToTable("licitaciones", t => t.HasCheckConstraint(
            "ck_licitaciones_presupuesto_positivo", "presupuesto_estimado_crc > 0"));

        builder.HasKey(l => l.Id);
        builder.Property<uint>("xmin").HasColumnName("xmin").IsRowVersion();

        builder.Property(l => l.Codigo).HasColumnName("codigo").HasMaxLength(50).IsRequired();
        builder.Property(l => l.CodigoNormalizado).HasColumnName("codigo_normalizado").HasMaxLength(50).IsRequired();
        builder.Property(l => l.Titulo).HasColumnName("titulo").HasMaxLength(200).IsRequired();
        builder.Property(l => l.Estado).HasColumnName("estado").HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(l => l.FechaCierre).HasColumnName("fecha_cierre").IsRequired();
        builder.Property(l => l.PresupuestoEstimadoCRC).HasColumnName("presupuesto_estimado_crc").HasColumnType("numeric(18,2)").IsRequired();
        builder.Property(l => l.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(l => l.UpdatedAt).HasColumnName("updated_at").IsRequired();
        builder.Property(l => l.DeletedAt).HasColumnName("deleted_at");

        builder.HasIndex(l => l.CodigoNormalizado).IsUnique().HasDatabaseName("ux_licitaciones_codigo_normalizado");
        builder.HasIndex(l => l.Estado).HasDatabaseName("ix_licitaciones_estado");
        builder.HasIndex(l => l.FechaCierre).HasDatabaseName("ix_licitaciones_fecha_cierre");

        builder.HasQueryFilter(l => l.DeletedAt == null);
    }
}
