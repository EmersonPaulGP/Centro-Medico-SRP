using CentroMedico.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CentroMedico.Infrastructure.Configurations;

public class TurnoEstadoConfiguration : IEntityTypeConfiguration<TurnoEstado>
{
    public void Configure(EntityTypeBuilder<TurnoEstado> builder)
    {
        builder.ToTable("TurnoEstado", "dbo");

        builder.HasKey(t => t.IdEstado)
            .HasName("PK_TurnoEstado");

        builder.Property(t => t.IdEstado)
            .HasColumnName("idEstado")
            .ValueGeneratedNever();

        builder.Property(t => t.Descripcion)
            .HasColumnName("descripcion")
            .HasMaxLength(50)
            .IsUnicode(false)
            .IsRequired();
    }
}
