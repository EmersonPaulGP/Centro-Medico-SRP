using CentroMedico.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CentroMedico.Infrastructure.Configurations;

public class PaisConfiguration : IEntityTypeConfiguration<Pais>
{
    public void Configure(EntityTypeBuilder<Pais> builder)
    {
        builder.ToTable("Pais", "dbo");

        builder.HasKey(p => p.IdPais)
            .HasName("PK_Pais");

        builder.Property(p => p.IdPais)
            .HasColumnName("idPais")
            .HasColumnType("char(3)")
            .IsFixedLength()
            .HasMaxLength(3)
            .IsRequired();

        builder.Property(p => p.Nombre)
            .HasColumnName("nombre")
            .HasMaxLength(50)
            .IsUnicode(false)
            .IsRequired();
    }
}
