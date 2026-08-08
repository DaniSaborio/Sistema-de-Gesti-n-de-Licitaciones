using Licitaciones.Domain.Licitaciones;
using Licitaciones.Domain.Ofertas;
using Licitaciones.Domain.Proveedores;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Licitaciones.Infrastructure.Persistence.Configurations;

public sealed class OfertaConfiguration : IEntityTypeConfiguration<Oferta>
{
    public void Configure(EntityTypeBuilder<Oferta> builder)
    {
        builder.ToTable("ofertas", t => t.HasCheckConstraint(
            "ck_ofertas_monto_positivo", "monto_ofertado_crc > 0"));

        builder.HasKey(o => o.Id);
        builder.Property<uint>("xmin").HasColumnName("xmin").IsRowVersion();

        builder.Property(o => o.LicitacionId).HasColumnName("licitacion_id").IsRequired();
        builder.Property(o => o.ProveedorId).HasColumnName("proveedor_id").IsRequired();
        builder.Property(o => o.MontoOfertadoCRC).HasColumnName("monto_ofertado_crc").HasColumnType("numeric(18,2)").IsRequired();
        builder.Property(o => o.FechaRegistro).HasColumnName("fecha_registro").IsRequired();
        builder.Property(o => o.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(o => o.UpdatedAt).HasColumnName("updated_at").IsRequired();

        builder.HasIndex(o => new { o.LicitacionId, o.ProveedorId })
            .IsUnique()
            .HasDatabaseName("ux_ofertas_licitacion_proveedor");

        builder.HasOne<Licitacion>().WithMany().HasForeignKey(o => o.LicitacionId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Proveedor>().WithMany().HasForeignKey(o => o.ProveedorId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
