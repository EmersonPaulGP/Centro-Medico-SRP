using CentroMedico.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CentroMedico.Infrastructure.Configurations;

public class PacienteConfiguration : IEntityTypeConfiguration<Paciente>
{
    public void Configure(EntityTypeBuilder<Paciente> builder)
    {
        builder.ToTable("Paciente", "dbo");

        builder.HasKey(p => p.IdPaciente)
            .HasName("PK_Paciente");

        builder.Property(p => p.IdPaciente)
            .HasColumnName("idPaciente")
            .UseIdentityColumn();

        builder.Property(p => p.Dni)
            .HasColumnName("dni")
            .HasMaxLength(20)
            .IsUnicode(false)
            .IsRequired();

        builder.HasIndex(p => p.Dni)
            .IsUnique()
            .HasDatabaseName("UQ_Paciente_Dni");

        builder.Property(p => p.Nombre)
            .HasColumnName("nombre")
            .HasMaxLength(50)
            .IsUnicode(false)
            .IsRequired();

        builder.Property(p => p.Apellido)
            .HasColumnName("apellido")
            .HasMaxLength(50)
            .IsUnicode(false)
            .IsRequired();

        builder.Property(p => p.FNacimiento)
            .HasColumnName("fNacimiento")
            .HasColumnType("date")
            .IsRequired();

        builder.Property(p => p.Domicilio)
            .HasColumnName("domicilio")
            .HasMaxLength(150)
            .IsUnicode(false)
            .IsRequired();

        builder.Property(p => p.IdPais)
            .HasColumnName("idPais")
            .HasColumnType("char(3)")
            .IsFixedLength()
            .HasMaxLength(3)
            .IsRequired();

        builder.Property(p => p.Telefono)
            .HasColumnName("telefono")
            .HasMaxLength(25)
            .IsUnicode(false)
            .IsRequired(false);

        builder.Property(p => p.Email)
            .HasColumnName("email")
            .HasMaxLength(150)
            .IsUnicode(false)
            .IsRequired();

        builder.Property(p => p.Observacion)
            .HasColumnName("observacion")
            .HasMaxLength(1000)
            .IsUnicode(false)
            .IsRequired(false);

        builder.Property(p => p.Activo)
            .HasColumnName("activo")
            .HasDefaultValue(true)
            .IsRequired();

        builder.Property(p => p.FechaAlta)
            .HasColumnName("fechaAlta")
            .HasColumnType("datetime2(0)")
            .HasDefaultValueSql("SYSDATETIME()")
            .IsRequired();

        builder.Property(p => p.FechaModificacion)
            .HasColumnName("fechaModificacion")
            .HasColumnType("datetime2(0)")
            .IsRequired(false);

        builder.HasOne(p => p.Pais)
            .WithMany()
            .HasForeignKey(p => p.IdPais)
            .HasConstraintName("FK_Paciente_Pais")
            .OnDelete(DeleteBehavior.Restrict);

        // Propiedad calculada en dominio, no persistida
        builder.Ignore(p => p.Edad);
    }
}
