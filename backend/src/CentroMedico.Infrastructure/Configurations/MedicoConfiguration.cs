using CentroMedico.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CentroMedico.Infrastructure.Configurations;

public class MedicoConfiguration : IEntityTypeConfiguration<Medico>
{
    public void Configure(EntityTypeBuilder<Medico> builder)
    {
        builder.ToTable("Medico", "dbo");

        builder.HasKey(m => m.IdMedico)
            .HasName("PK_Medico");

        builder.Property(m => m.IdMedico)
            .HasColumnName("idMedico")
            .UseIdentityColumn();

        builder.Property(m => m.Matricula)
            .HasColumnName("matricula")
            .HasMaxLength(30)
            .IsUnicode(false)
            .IsRequired();

        builder.HasIndex(m => m.Matricula)
            .IsUnique()
            .HasDatabaseName("UQ_Medico_Matricula");

        builder.Property(m => m.Dni)
            .HasColumnName("dni")
            .HasMaxLength(20)
            .IsUnicode(false)
            .IsRequired();

        builder.HasIndex(m => m.Dni)
            .IsUnique()
            .HasDatabaseName("UQ_Medico_Dni");

        builder.Property(m => m.Nombre)
            .HasColumnName("nombre")
            .HasMaxLength(50)
            .IsUnicode(false)
            .IsRequired();

        builder.Property(m => m.Apellido)
            .HasColumnName("apellido")
            .HasMaxLength(50)
            .IsUnicode(false)
            .IsRequired();

        builder.Property(m => m.Telefono)
            .HasColumnName("telefono")
            .HasMaxLength(25)
            .IsUnicode(false)
            .IsRequired(false);

        builder.Property(m => m.Email)
            .HasColumnName("email")
            .HasMaxLength(150)
            .IsUnicode(false)
            .IsRequired(false);

        builder.Property(m => m.Activo)
            .HasColumnName("activo")
            .HasDefaultValue(true)
            .IsRequired();

        builder.Property(m => m.FechaAlta)
            .HasColumnName("fechaAlta")
            .HasColumnType("datetime2(0)")
            .HasDefaultValueSql("SYSDATETIME()")
            .IsRequired();

        builder.Property(m => m.FechaModificacion)
            .HasColumnName("fechaModificacion")
            .HasColumnType("datetime2(0)")
            .IsRequired(false);

        builder.HasMany(m => m.Especialidades)
            .WithOne(me => me.Medico)
            .HasForeignKey(me => me.IdMedico)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
