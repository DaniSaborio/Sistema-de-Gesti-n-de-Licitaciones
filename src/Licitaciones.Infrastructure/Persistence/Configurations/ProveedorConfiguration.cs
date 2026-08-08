using Licitaciones.Domain.Proveedores;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Licitaciones.Infrastructure.Persistence.Configurations;

public sealed class ProveedorConfiguration : IEntityTypeConfiguration<Proveedor>
{
    public void Configure(EntityTypeBuilder<Proveedor> builder)
    {
        builder.ToTable("proveedores");
        builder.HasKey(p => p.Id);
        builder.Property<uint>("xmin").HasColumnName("xmin").IsRowVersion();

        builder.Property(p => p.Nombre).HasColumnName("nombre").HasMaxLength(200).IsRequired();
        builder.Property(p => p.NombreNormalizado).HasColumnName("nombre_normalizado").HasMaxLength(200).IsRequired();
        builder.Property(p => p.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(p => p.UpdatedAt).HasColumnName("updated_at").IsRequired();
        builder.Property(p => p.DeletedAt).HasColumnName("deleted_at");

        builder.HasIndex(p => p.NombreNormalizado).IsUnique().HasDatabaseName("ux_proveedores_nombre_normalizado");

        builder.HasQueryFilter(p => p.DeletedAt == null);
    }
}
