using CentroMedico.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CentroMedico.Infrastructure.Configurations;

public class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.ToTable("Usuario", "dbo");

        builder.HasKey(u => u.IdUsuario)
            .HasName("PK_Usuario");

        builder.Property(u => u.IdUsuario)
            .HasColumnName("idUsuario")
            .UseIdentityColumn();

        builder.Property(u => u.Email)
            .HasColumnName("email")
            .HasMaxLength(100)
            .IsUnicode(false)
            .IsRequired();

        builder.Property(u => u.PasswordHash)
            .HasColumnName("passwordHash")
            .HasMaxLength(255)
            .IsUnicode(false)
            .IsRequired();

        builder.Property(u => u.Nombre)
            .HasColumnName("nombre")
            .HasMaxLength(50)
            .IsUnicode(false)
            .IsRequired();

        builder.Property(u => u.Apellido)
            .HasColumnName("apellido")
            .HasMaxLength(50)
            .IsUnicode(false)
            .IsRequired();

        builder.Property(u => u.Rol)
            .HasColumnName("rol")
            .HasMaxLength(30)
            .IsUnicode(false)
            .IsRequired();

        builder.Property(u => u.Activo)
            .HasColumnName("activo")
            .HasDefaultValue(true)
            .IsRequired();

        builder.Property(u => u.FechaAlta)
            .HasColumnName("fechaAlta")
            .HasColumnType("datetime2(0)")
            .HasDefaultValueSql("SYSDATETIME()")
            .IsRequired();

        builder.HasIndex(u => u.Email)
            .IsUnique()
            .HasDatabaseName("IX_Usuario_Email");
    }
}
