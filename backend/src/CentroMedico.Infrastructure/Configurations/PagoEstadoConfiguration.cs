using CentroMedico.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CentroMedico.Infrastructure.Configurations;

public class PagoEstadoConfiguration : IEntityTypeConfiguration<PagoEstado>
{
    public void Configure(EntityTypeBuilder<PagoEstado> builder)
    {
        builder.ToTable("PagoEstado", "dbo");

        builder.HasKey(p => p.IdEstado)
            .HasName("PK_PagoEstado");

        builder.Property(p => p.IdEstado)
            .HasColumnName("idEstado")
            .ValueGeneratedNever();

        builder.Property(p => p.Descripcion)
            .HasColumnName("descripcion")
            .HasMaxLength(50)
            .IsUnicode(false)
            .IsRequired();
    }
}
