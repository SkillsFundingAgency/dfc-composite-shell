using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading.Tasks;
using DFC.Composite.Shell.Utilities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;

namespace DFC.Composite.Shell.Services.AssetLocationAndVersion
{
    public class AssetLocationAndVersion : IAssetLocationAndVersion
    {
        private const string ContentMDS = "content-md5";
        private readonly HttpClient _httpClient;
        private readonly IAsyncHelper _asyncHelper;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly ILogger<AssetLocationAndVersion> _logger;

        public AssetLocationAndVersion(
            HttpClient httpClientService,
            IAsyncHelper asyncHelper,
            IHostingEnvironment hostingEnvironment,
            ILogger<AssetLocationAndVersion> logger)
        {
            _httpClient = httpClientService;
            _asyncHelper = asyncHelper;
            _hostingEnvironment = hostingEnvironment;
            _logger = logger; ;
        }

        public string GetCdnAssetFileAndVersion(string assetLocation)
        {
            var version = _asyncHelper.Synchronise(() => GetFileHashAsync(assetLocation));

            return $"{assetLocation}?{version}";
        }

        public string GetLocalAssetFileAndVersion(string assetLocation)
        {
            var physicalPath = Path.Combine(_hostingEnvironment.WebRootPath, assetLocation.Replace('/', Path.DirectorySeparatorChar));
            var version = GetFileHash(physicalPath);

            return $"/{assetLocation}?{version}";
        }

        private static string GetFileHash(string file)
        {
            if (File.Exists(file))
            {
                var md5 = MD5.Create();

                using (var stream = new FileStream(file, FileMode.Open, FileAccess.Read))
                {
                    return BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", string.Empty);
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
                var response = await _httpClient.GetAsync(assetLocation);

                if (response.IsSuccessStatusCode)
                {
                    var hashCode = response.Content.Headers.GetValues(ContentMDS).FirstOrDefault();

                    return !string.IsNullOrWhiteSpace(hashCode) ? hashCode.Replace("-", string.Empty) : DateTime.Now.ToString("yyyyMMddHH");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{nameof(GetFileHashAsync)}: Failed to get file hash");
            }

            //If we don't get a valid response use the current time to the nearest hour.
            return DateTime.Now.ToString("yyyyMMddHH");
        }
    }
}