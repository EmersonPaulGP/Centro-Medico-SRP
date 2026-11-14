using CentroMedico.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CentroMedico.Infrastructure.Configurations;

public class EspecialidadConfiguration : IEntityTypeConfiguration<Especialidad>
{
    public void Configure(EntityTypeBuilder<Especialidad> builder)
    {
        builder.ToTable("Especialidad", "dbo");

        builder.HasKey(e => e.IdEspecialidad)
            .HasName("PK_Especialidad");

        builder.Property(e => e.IdEspecialidad)
            .HasColumnName("idEspecialidad")
            .UseIdentityColumn();

        builder.Property(e => e.Nombre)
            .HasColumnName("nombre")
            .HasMaxLength(60)
            .IsUnicode(false)
            .IsRequired();

        builder.HasIndex(e => e.Nombre)
            .IsUnique()
            .HasDatabaseName("UQ_Especialidad_Nombre");

        builder.Property(e => e.Descripcion)
            .HasColumnName("descripcion")
            .HasMaxLength(200)
            .IsUnicode(false)
            .IsRequired(false);

        builder.Property(e => e.Activo)
            .HasColumnName("activo")
            .HasDefaultValue(true)
            .IsRequired();
    }
}
