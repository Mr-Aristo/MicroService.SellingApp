using CatalogService.API.Core.Domain;
using CatalogService.API.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CatalogService.API.Infrastructure.EntityConfigurations
{
    public class CatalogBrandEntityTypeConfiguration : IEntityTypeConfiguration<CatalogBrand>
    {
        public void Configure(EntityTypeBuilder<CatalogBrand> builder)
        {
            builder.ToTable("CatalogBrand", CatalogContext.DEFAULT_SCHEMA);
            builder.HasKey(ci => ci.Id);
            builder.Property(ci=>ci.Id)
                .UseHiLo("catalog_vrand_hilo")
                .IsRequired();
           
            builder.Property(cp=> cp.Brand)
                .IsRequired()
                .HasMaxLength(100);
        }
    }
}
