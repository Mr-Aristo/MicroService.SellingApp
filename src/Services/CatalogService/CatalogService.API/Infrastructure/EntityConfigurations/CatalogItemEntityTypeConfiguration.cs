using CatalogService.API.Core.Domain;
using CatalogService.API.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CatalogService.API.Infrastructure.EntityConfigurations
{
    public class CatalogItemEntityTypeConfiguration : IEntityTypeConfiguration<CatalogItem>
    {
        public void Configure(EntityTypeBuilder<CatalogItem> builder)
        {
            builder.ToTable("CatalogItem", CatalogContext.DEFAULT_SCHEMA);
            builder.Property(ci => ci.Id)
                .UseHiLo("catalog_hilo")
                .IsRequired();

            builder.Property(ci => ci.Name)
                .IsRequired(true)
                .HasMaxLength(50);

            builder.Property(ci => ci.Description)
                .IsRequired(false)
                .HasMaxLength(250);

            builder.Property(ci => ci.Price)
                .IsRequired(true);
            builder.Property(ci=> ci.AvailableStock)
                .IsRequired(true);

            builder.Property(ci => ci.OnReOrder)
               .IsRequired(false);

            builder.Property(ci =>ci.PictureFileName)
                .IsRequired(false);

            builder.Ignore(ci => ci.PictureUrl);

            builder.HasOne(ci => ci.CatalogBrand)
                .WithMany()
                .HasForeignKey(ci => ci.CatalogBrandId);

            builder.HasOne(ci => ci.CatalogType)
                .WithMany()
                .HasForeignKey(ci => ci.CatalogTypeId);

        }
    }
}
