using CentroMedico.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CentroMedico.Infrastructure.Configurations;

public class TurnoConfiguration : IEntityTypeConfiguration<Turno>
{
    public void Configure(EntityTypeBuilder<Turno> builder)
    {
        builder.ToTable("Turno", "dbo");

        builder.HasKey(t => t.IdTurno)
            .HasName("PK_Turno");

        builder.Property(t => t.IdTurno)
            .HasColumnName("idTurno")
            .UseIdentityColumn();

        builder.Property(t => t.IdPaciente)
            .HasColumnName("idPaciente")
            .IsRequired();

        builder.Property(t => t.IdMedico)
            .HasColumnName("idMedico")
            .IsRequired();

        builder.Property(t => t.FechaTurno)
            .HasColumnName("fechaTurno")
            .HasColumnType("datetime2(0)")
            .IsRequired();

        builder.Property(t => t.IdEstado)
            .HasColumnName("idEstado")
            .HasDefaultValue((short)0)
            .IsRequired();

        builder.Property(t => t.Observacion)
            .HasColumnName("observacion")
            .HasMaxLength(1000)
            .IsUnicode(false)
            .IsRequired(false);

        builder.Property(t => t.FechaAlta)
            .HasColumnName("fechaAlta")
            .HasColumnType("datetime2(0)")
            .HasDefaultValueSql("SYSDATETIME()")
            .IsRequired();

        builder.Property(t => t.FechaModificacion)
            .HasColumnName("fechaModificacion")
            .HasColumnType("datetime2(0)")
            .IsRequired(false);

        builder.HasIndex(t => new { t.IdMedico, t.FechaTurno })
            .HasDatabaseName("IX_Turno_Medico_FechaTurno");

        builder.HasOne(t => t.Paciente)
            .WithMany()
            .HasForeignKey(t => t.IdPaciente)
            .HasConstraintName("FK_Turno_Paciente")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.Medico)
            .WithMany()
            .HasForeignKey(t => t.IdMedico)
            .HasConstraintName("FK_Turno_Medico")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.TurnoEstado)
            .WithMany()
            .HasForeignKey(t => t.IdEstado)
            .HasConstraintName("FK_Turno_TurnoEstado")
            .OnDelete(DeleteBehavior.Restrict);
    }
}
