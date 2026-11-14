using CentroMedico.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CentroMedico.Infrastructure.Configurations;

public class ConceptoConfiguration : IEntityTypeConfiguration<Concepto>
{
    public void Configure(EntityTypeBuilder<Concepto> builder)
    {
        builder.ToTable("Concepto", "dbo");

        builder.HasKey(c => c.IdConcepto)
            .HasName("PK_Concepto");

        builder.Property(c => c.IdConcepto)
            .HasColumnName("idConcepto")
            .UseIdentityColumn();

        builder.Property(c => c.Descripcion)
            .HasColumnName("descripcion")
            .HasMaxLength(100)
            .IsUnicode(false)
            .IsRequired();
    }
}
