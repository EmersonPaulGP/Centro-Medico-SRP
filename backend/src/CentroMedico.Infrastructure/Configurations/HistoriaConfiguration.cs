using CentroMedico.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CentroMedico.Infrastructure.Configurations;

public class HistoriaConfiguration : IEntityTypeConfiguration<Historia>
{
    public void Configure(EntityTypeBuilder<Historia> builder)
    {
        builder.ToTable("Historia", "dbo");

        builder.HasKey(h => h.IdHistoria)
            .HasName("PK_Historia");

        builder.Property(h => h.IdHistoria)
            .HasColumnName("idHistoria")
            .UseIdentityColumn();

        builder.Property(h => h.IdPaciente)
            .HasColumnName("idPaciente")
            .IsRequired();

        builder.Property(h => h.IdMedico)
            .HasColumnName("idMedico")
            .IsRequired();

        builder.Property(h => h.IdTurno)
            .HasColumnName("idTurno")
            .IsRequired(false);

        builder.Property(h => h.FechaHistoria)
            .HasColumnName("fechaHistoria")
            .HasColumnType("datetime2(0)")
            .HasDefaultValueSql("SYSDATETIME()")
            .IsRequired();

        builder.Property(h => h.Diagnostico)
            .HasColumnName("diagnostico")
            .HasMaxLength(250)
            .IsUnicode(false)
            .IsRequired(false);

        builder.Property(h => h.Observacion)
            .HasColumnName("observacion")
            .IsUnicode(false)
            .IsRequired();

        builder.Property(h => h.FechaAlta)
            .HasColumnName("fechaAlta")
            .HasColumnType("datetime2(0)")
            .HasDefaultValueSql("SYSDATETIME()")
            .IsRequired();

        builder.HasIndex(h => new { h.IdPaciente, h.FechaHistoria })
            .HasDatabaseName("IX_Historia_Paciente_Fecha");

        builder.HasOne(h => h.Paciente)
            .WithMany()
            .HasForeignKey(h => h.IdPaciente)
            .HasConstraintName("FK_Historia_Paciente")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(h => h.Medico)
            .WithMany()
            .HasForeignKey(h => h.IdMedico)
            .HasConstraintName("FK_Historia_Medico")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(h => h.Turno)
            .WithMany()
            .HasForeignKey(h => h.IdTurno)
            .HasConstraintName("FK_Historia_Turno")
            .OnDelete(DeleteBehavior.Restrict);
    }
}
