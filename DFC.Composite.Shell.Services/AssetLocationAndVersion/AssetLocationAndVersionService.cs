using DFC.Composite.Shell.Services.Utilities;
using DFC.Composite.Shell.Utilities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace DFC.Composite.Shell.Services.AssetLocationAndVersion
{
    public class AssetLocationAndVersionService : IAssetLocationAndVersionService
    {
        private const string ContentMDS = "content-md5";
        private readonly HttpClient httpClient;
        private readonly IAsyncHelper asyncHelper;
        private readonly IHostingEnvironment hostingEnvironment;
        private readonly ILogger<AssetLocationAndVersionService> logger;
        private readonly IFileInfoHelper fileInfoHelper;

        public AssetLocationAndVersionService(
            HttpClient httpClientService,
            IAsyncHelper asyncHelper,
            IHostingEnvironment hostingEnvironment,
            ILogger<AssetLocationAndVersionService> logger,
            IFileInfoHelper fileInfoHelper)
        {
            httpClient = httpClientService;
            this.asyncHelper = asyncHelper;
            this.hostingEnvironment = hostingEnvironment;
            this.logger = logger;
            this.fileInfoHelper = fileInfoHelper;
        }

        public string GetCdnAssetFileAndVersion(string assetLocation)
        {
            var version = asyncHelper.Synchronise(() => GetFileHashAsync(assetLocation));

            return $"{assetLocation}?{version}";
        }

        public string GetLocalAssetFileAndVersion(string assetLocation)
        {
            var physicalPath = Path.Combine(hostingEnvironment.WebRootPath, assetLocation?.Replace('/', Path.DirectorySeparatorChar));
            var version = GetFileHash(physicalPath);

            return $"/{assetLocation}?{version}";
        }

        private string GetFileHash(string file)
        {
            if (fileInfoHelper.FileExists(file))
            {
                using (var md5 = MD5.Create())
                {
                    using (var stream = fileInfoHelper.GetStream(file))
                    {
                        return BitConverter.ToString(md5.ComputeHash(stream))
                            .Replace("-", string.Empty, StringComparison.OrdinalIgnoreCase);
                    }
                }
            }
            else
            {
                return string.Empty;
            }
        }

        private async Task<string> GetFileHashAsync(string assetLocation)
        {
            try
            {
                var response = await httpClient.GetAsync(assetLocation).ConfigureAwait(false);

                if (response.IsSuccessStatusCode)
                {
                    var hashCode = response.Content.Headers.GetValues(ContentMDS).FirstOrDefault();

                    return !string.IsNullOrWhiteSpace(hashCode)
                        ? hashCode.Replace("-", string.Empty, StringComparison.OrdinalIgnoreCase)
                        : DateTime.Now.ToString("yyyyMMddHH", CultureInfo.InvariantCulture);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"{nameof(GetFileHashAsync)}: Failed to get file hash");
            }

            //If we don't get a valid response use the current time to the nearest hour.
            return DateTime.Now.ToString("yyyyMMddHH", CultureInfo.InvariantCulture);
        }
    }
}