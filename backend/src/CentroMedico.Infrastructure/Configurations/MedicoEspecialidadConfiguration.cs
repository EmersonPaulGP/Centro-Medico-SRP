using CentroMedico.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CentroMedico.Infrastructure.Configurations;

public class MedicoEspecialidadConfiguration : IEntityTypeConfiguration<MedicoEspecialidad>
{
    public void Configure(EntityTypeBuilder<MedicoEspecialidad> builder)
    {
        builder.ToTable("MedicoEspecialidad", "dbo");

        builder.HasKey(me => new { me.IdMedico, me.IdEspecialidad })
            .HasName("PK_MedicoEspecialidad");

        builder.Property(me => me.IdMedico)
            .HasColumnName("idMedico")
            .IsRequired();

        builder.Property(me => me.IdEspecialidad)
            .HasColumnName("idEspecialidad")
            .IsRequired();

        builder.Property(me => me.Descripcion)
            .HasColumnName("descripcion")
            .HasMaxLength(100)
            .IsUnicode(false)
            .IsRequired(false);

        builder.HasOne(me => me.Medico)
            .WithMany(m => m.Especialidades)
            .HasForeignKey(me => me.IdMedico)
            .HasConstraintName("FK_MedicoEspecialidad_Medico")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(me => me.Especialidad)
            .WithMany(e => e.MedicosEspecialidades)
            .HasForeignKey(me => me.IdEspecialidad)
            .HasConstraintName("FK_MedicoEspecialidad_Especialidad")
            .OnDelete(DeleteBehavior.Restrict);
    }
}
