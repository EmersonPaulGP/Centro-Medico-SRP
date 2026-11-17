using CentroMedico.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CentroMedico.Infrastructure.Configurations;

public class PagoConfiguration : IEntityTypeConfiguration<Pago>
{
    public void Configure(EntityTypeBuilder<Pago> builder)
    {
        builder.ToTable("Pago", "dbo");

        builder.HasKey(p => p.IdPago)
            .HasName("PK_Pago");

        builder.Property(p => p.IdPago)
            .HasColumnName("idPago")
            .UseIdentityColumn();

        builder.Property(p => p.IdPaciente)
            .HasColumnName("idPaciente")
            .IsRequired();

        builder.Property(p => p.IdTurno)
            .HasColumnName("idTurno")
            .IsRequired(false);

        builder.Property(p => p.IdConcepto)
            .HasColumnName("idConcepto")
            .HasColumnType("tinyint")
            .IsRequired();

        builder.Property(p => p.Fecha)
            .HasColumnName("fecha")
            .HasColumnType("datetime2(0)")
            .HasDefaultValueSql("SYSDATETIME()")
            .IsRequired();

        builder.Property(p => p.Monto)
            .HasColumnName("monto")
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(p => p.IdEstado)
            .HasColumnName("idEstado")
            .HasColumnType("tinyint")
            .HasDefaultValue((byte)0)
            .IsRequired();

        builder.Property(p => p.MetodoPago)
            .HasColumnName("metodoPago")
            .HasMaxLength(50)
            .IsUnicode(false)
            .IsRequired(false);

        builder.Property(p => p.Observacion)
            .HasColumnName("observacion")
            .HasMaxLength(1000)
            .IsUnicode(false)
            .IsRequired(false);

        builder.Property(p => p.FechaAlta)
            .HasColumnName("fechaAlta")
            .HasColumnType("datetime2(0)")
            .HasDefaultValueSql("SYSDATETIME()")
            .IsRequired();

        builder.HasIndex(p => new { p.IdPaciente, p.Fecha })
            .HasDatabaseName("IX_Pago_Paciente_Fecha");

        builder.HasOne(p => p.Paciente)
            .WithMany()
            .HasForeignKey(p => p.IdPaciente)
            .HasConstraintName("FK_Pago_Paciente")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Turno)
            .WithMany()
            .HasForeignKey(p => p.IdTurno)
            .HasConstraintName("FK_Pago_Turno")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Concepto)
            .WithMany()
            .HasForeignKey(p => p.IdConcepto)
            .HasConstraintName("FK_Pago_Concepto")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.PagoEstado)
            .WithMany()
            .HasForeignKey(p => p.IdEstado)
            .HasConstraintName("FK_Pago_PagoEstado")
            .OnDelete(DeleteBehavior.Restrict);
    }
}
