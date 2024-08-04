using System.Collections;
using System.Globalization;
using System.Runtime.CompilerServices;
using CatalogService.API.Core.Domain;
using Microsoft.Data.SqlClient;
using Polly;

namespace CatalogService.API.Infrastructure.Context
{
    public class CatalogContextSeed
    {
        public async Task SeedAsync(CatalogContext context, IWebHostEnvironment webHost, ILogger<CatalogContextSeed> logger)
        {
            var policy = Policy.Handle<SqlException>()
                .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: retry => TimeSpan.FromSeconds(50),
                onRetry: (exception, timeSpan, retry, ctx) =>
                {
                    logger.LogWarning(exception, "[{prefix}] Exception {ExceptionType} with message {Message} detected on attempt {retry} of {prefix}");
                });
            var setupDirPath = Path.Combine(webHost.ContentRootPath, "Infrastructure", "Setup", "SeedFiles");
            var picturePath = "Pics";

            await policy.ExecuteAsync(() => ProcessSeeding(context, setupDirPath, picturePath, logger));
        }

        private async Task ProcessSeeding(CatalogContext context, string setupDirPath, string picturePath, ILogger logger)
        {
            if (!context.CatalogBrands.Any())
            {
                await context.CatalogBrands.AddRangeAsync(GetCatalogBrandFromFile(setupDirPath));
                await context.SaveChangesAsync();
            }

            if (!context.CatalogTypes.Any())
            {
                await context.CatalogTypes.AddRangeAsync(GetCatalogTypeFromFile(setupDirPath));
                await context.SaveChangesAsync();

            }

            if (!context.CatalogItems.Any())
            {
                await context.CatalogItems.AddRangeAsync(GetCatalogItemFromFile(setupDirPath, context));
                await context.SaveChangesAsync();

            }

        }
        private IEnumerable<CatalogBrand> GetCatalogBrandFromFile(string contentPath)
        {
            IEnumerable<CatalogBrand> GetPreConfuguredCatalogBrands()
            {
                return new List<CatalogBrand>()
                {
                    new CatalogBrand() {Brand="Azure"},
                    new CatalogBrand() {Brand=".Net"},
                    new CatalogBrand() {Brand="VS 2000"},
                    new CatalogBrand() {Brand="Sqlss"},
                    new CatalogBrand() {Brand="other"}

                };
            }
            string fileName = Path.Combine(contentPath, "CatalogBrand.txt");
            if (!File.Exists(fileName))
            {
                return GetPreConfuguredCatalogBrands();
            }
            var fileContent = File.ReadAllLines(fileName);

            var list = fileContent.Select(x => new CatalogBrand()
            {
                Brand = x.Trim(),
            }).Where(x => x != null);
            return list ?? GetPreConfuguredCatalogBrands();
        }
        private IEnumerable<CatalogType> GetCatalogTypeFromFile(string contentPath)
        {
            IEnumerable<CatalogType> GetPreConfuguredCatalogTypes()
            {
                return new List<CatalogType>()
                {
                    new CatalogType() {Type = " Mug"},
                    new CatalogType() {Type="T-shirt"},
                    new CatalogType() {Type= " sheet"},
                    new CatalogType() {Type= "USB mem"}
                };
            }

            string fileName = Path.Combine(contentPath, "CatalogType.txt");

            if (!File.Exists(fileName))
            {
                return GetPreConfuguredCatalogTypes();
            }
            var fileContent = File.ReadAllLines(fileName);

            var list = fileContent.Select(x => new CatalogType()
            {
                Type = x.Trim(),
            }).Where(x => x != null);
            return list ?? GetPreConfuguredCatalogTypes();

        }
        private IEnumerable<CatalogItem> GetCatalogItemFromFile(string contentPath, CatalogContext context)
        {
            IEnumerable<CatalogItem> GetPreconfguratedItems()
            {


                return new List<CatalogItem>()
                 {
                    new CatalogItem(){CatalogTypeId =2, CatalogBrandId = 2,Description=".Net Test Text1", Name= "TestName",Price= 64578,PictureFileName = "sda", PictureUrl="url" },
                    new CatalogItem(){CatalogTypeId =2, CatalogBrandId = 2,Description=".Net Test Text2", Name= "TestName2",Price= 64578,PictureFileName = "sda", PictureUrl="url" },
                    new CatalogItem(){CatalogTypeId =1, CatalogBrandId = 2,Description=".Net Test Text3", Name= "TestName3",Price= 64578,PictureFileName = "sda", PictureUrl="url" },
                    new CatalogItem(){CatalogTypeId =1, CatalogBrandId = 2,Description=".Net Test Text4", Name= "TestName4",Price= 64578,PictureFileName = "sda", PictureUrl="url" },
                    new CatalogItem(){CatalogTypeId =1, CatalogBrandId = 2,Description=".Net Test Text22", Name= "TestName5",Price= 64578,PictureFileName = "sda", PictureUrl="url" },
                    new CatalogItem(){CatalogTypeId =1, CatalogBrandId = 2,Description=".Net Test Text533", Name= "TestName6",Price= 64578,PictureFileName = "sda", PictureUrl="url" },
                    new CatalogItem(){CatalogTypeId =2, CatalogBrandId = 1,Description=".Net Test Text2", Name= "TestName7",Price= 64578,PictureFileName = "sda", PictureUrl="url" },
                    new CatalogItem(){CatalogTypeId =2, CatalogBrandId = 1,Description=".Net Test Text222", Name= "TestName8",Price= 64578,PictureFileName = "sda", PictureUrl="url" },
                    new CatalogItem(){CatalogTypeId =2, CatalogBrandId = 1,Description=".Net Test Text43", Name= "TestName9",Price= 64578,PictureFileName = "sda", PictureUrl="url" },
                    new CatalogItem(){CatalogTypeId =2, CatalogBrandId = 2,Description=".Net Test Tex2t", Name= "TestName11",Price= 64578,PictureFileName = "sda", PictureUrl="url" },
                    new CatalogItem(){CatalogTypeId =2, CatalogBrandId = 1,Description=".Net Test Te2xt", Name= "TestName22",Price= 64578,PictureFileName = "sda", PictureUrl="url" },
                    new CatalogItem(){CatalogTypeId =2, CatalogBrandId = 2,Description=".Net Test Tex2t", Name= "TestName22",Price= 64578,PictureFileName = "sda", PictureUrl="url" }
                 };

            }

            string fileName = Path.Combine(contentPath, "CatalogItem.txt");

            if (!File.Exists(fileName))
            {
                return GetPreconfguratedItems();

            }
            var catalTypeIdLookup = context.CatalogTypes.ToDictionary(ct => ct.Type, ct => ct.Id);
            var catalBrandIdLookup = context.CatalogBrands.ToDictionary(ct => ct.Brand, ct => ct.Id);

            var fileContent = File.ReadAllLines(fileName)
                 .Skip(1)//skip Header row
                 .Select(i => i.Split(","))
                 .Select(i => new CatalogItem
                 {
                     CatalogTypeId = catalTypeIdLookup[i[0]],
                     CatalogBrandId = catalBrandIdLookup[i[1]],
                     Description = i[2].Trim('"').Trim(),
                     Name = i[3].Trim('"').Trim(),
                     Price = Decimal.Parse(i[4].Trim('"').Trim(), NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture),
                     PictureFileName = i[5].Trim('"').Trim(),
                     AvailableStock = string.IsNullOrEmpty(i[6]) ? 0 : int.Parse(i[6]),
                     OnReOrder = Convert.ToBoolean(i[7])
                 });
            return fileContent;
        }
        private void GetCatalogPictures(string contentPath, string picturePath)
        {
            picturePath ??= "pics";
            if (picturePath != null)
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(picturePath);
                foreach (var file in directoryInfo.GetFiles())
                {
                    file.Delete();

                }
                string zipFileCatalogItemPicture = Path.Combine(contentPath, "CataloItem.zip");
            }
        }

    }
}
