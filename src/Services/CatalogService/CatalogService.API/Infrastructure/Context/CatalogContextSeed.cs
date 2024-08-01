using System.Collections;
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


        }
        private IEnumerable<CatalogBrand> GetCatalogBrandFromFile(string contentPath) { }
        private IEnumerable<CatalogType> GetCatalogTypeFromFile(string contentPath) { }
        private IEnumerable<CatalogItem> GetCatalogItemFromFile(string contentPath,CatalogContext context)
        {
            IEnumerable<CatalogItem> GetPreconfguratedItems()
            {


                return new List<CatalogItem>()
                 {
                    new CatalogItem(){CatalogTypeId =2, CatalogBrandId = 2,Description=".Net Test Text", Name= "TestName",Price= 64578,PictureFileName = "sda", PictureUrl="url" },
                    new CatalogItem(){CatalogTypeId =2, CatalogBrandId = 2,Description=".Net Test Text", Name= "TestName",Price= 64578,PictureFileName = "sda", PictureUrl="url" },
                    new CatalogItem(){CatalogTypeId =1, CatalogBrandId = 2,Description=".Net Test Text", Name= "TestName",Price= 64578,PictureFileName = "sda", PictureUrl="url" },
                    new CatalogItem(){CatalogTypeId =1, CatalogBrandId = 2,Description=".Net Test Text", Name= "TestName",Price= 64578,PictureFileName = "sda", PictureUrl="url" },
                    new CatalogItem(){CatalogTypeId =1, CatalogBrandId = 2,Description=".Net Test Text", Name= "TestName",Price= 64578,PictureFileName = "sda", PictureUrl="url" },
                    new CatalogItem(){CatalogTypeId =1, CatalogBrandId = 2,Description=".Net Test Text", Name= "TestName",Price= 64578,PictureFileName = "sda", PictureUrl="url" },
                    new CatalogItem(){CatalogTypeId =2, CatalogBrandId = 1,Description=".Net Test Text", Name= "TestName",Price= 64578,PictureFileName = "sda", PictureUrl="url" },
                    new CatalogItem(){CatalogTypeId =2, CatalogBrandId = 1,Description=".Net Test Text", Name= "TestName",Price= 64578,PictureFileName = "sda", PictureUrl="url" },
                    new CatalogItem(){CatalogTypeId =2, CatalogBrandId = 1,Description=".Net Test Text", Name= "TestName",Price= 64578,PictureFileName = "sda", PictureUrl="url" },
                    new CatalogItem(){CatalogTypeId =2, CatalogBrandId = 2,Description=".Net Test Text", Name= "TestName",Price= 64578,PictureFileName = "sda", PictureUrl="url" },
                    new CatalogItem(){CatalogTypeId =2, CatalogBrandId = 1,Description=".Net Test Text", Name= "TestName",Price= 64578,PictureFileName = "sda", PictureUrl="url" },
                    new CatalogItem(){CatalogTypeId =2, CatalogBrandId = 2,Description=".Net Test Text", Name= "TestName",Price= 64578,PictureFileName = "sda", PictureUrl="url" }
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
                     //Price = Decimal.Parse()
                 });
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
