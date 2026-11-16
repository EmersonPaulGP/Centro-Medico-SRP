using CentroMedico.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CentroMedico.Infrastructure.Configurations;

public class PacienteInfoConfiguration : IEntityTypeConfiguration<PacienteInfo>
{
    public void Configure(EntityTypeBuilder<PacienteInfo> builder)
    {
        builder.ToTable("PacienteInfo", "dbo");

        builder.HasKey(pi => pi.IdPaciente)
            .HasName("PK_PacienteInfo");

        builder.Property(pi => pi.IdPaciente)
            .HasColumnName("idPaciente")
            .ValueGeneratedNever();

        builder.Property(pi => pi.Diabetico)
            .HasColumnName("diabetico")
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(pi => pi.Implantes)
            .HasColumnName("implantes")
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(pi => pi.Alergias)
            .HasColumnName("alergias")
            .HasMaxLength(500)
            .IsUnicode(false)
            .IsRequired(false);

        builder.Property(pi => pi.GrupoSanguineo)
            .HasColumnName("grupoSanguineo")
            .HasMaxLength(5)
            .IsUnicode(false)
            .IsRequired(false);

        builder.Property(pi => pi.FechaModificacion)
            .HasColumnName("fechaModificacion")
            .HasColumnType("datetime2(0)")
            .IsRequired(false);

        builder.HasOne(pi => pi.Paciente)
            .WithOne(p => p.PacienteInfo)
            .HasForeignKey<PacienteInfo>(pi => pi.IdPaciente)
            .HasConstraintName("FK_PacienteInfo_Paciente")
            .OnDelete(DeleteBehavior.Cascade);
    }
}
