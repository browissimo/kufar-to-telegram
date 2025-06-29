using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace KufarParserApp.Kufar
{
    public class KufarClient
    {
        private const string BaseUrl = "https://searchapi.kufar.by/v1/search/rendered-paginated";
        private readonly string _countUrl;
        private readonly HttpClient _httpClient;
        private readonly ILogger<KufarClient> _logger;
        private readonly Dictionary<string, List<string>> _params;

        public KufarClient(IConfiguration configuration, ILogger<KufarClient> logger)
        {
            _params = configuration.GetSection("SearchParams")
                .Get<Dictionary<string, List<string>>>() ?? throw new ArgumentNullException("SearchParams configuration is missing");
            _httpClient = new HttpClient();
            _logger = logger;
            _countUrl = BaseUrl.Replace("rendered-paginated", "count");
            _httpClient.Timeout = TimeSpan.FromSeconds(10);
        }

        public async Task<JsonDocument?> FetchPageAsync(string? cursor = null)
        {
            try
            {
                var queryParams = new Dictionary<string, List<string>>(_params);
                if (!string.IsNullOrEmpty(cursor))
                {
                    queryParams["cursor"] = new List<string> { cursor };
                }

                var uri = BuildUriWithQuery(BaseUrl, queryParams);
                var response = await _httpClient.GetAsync(uri);
                response.EnsureSuccessStatusCode();

                return await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка запроса страницы");
                return null;
            }
        }

        public async Task<int?> FetchCountAsync()
        {
            try
            {
                var uri = BuildUriWithQuery(_countUrl, _params);
                var response = await _httpClient.GetAsync(uri);
                response.EnsureSuccessStatusCode();

                using var doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
                return doc.RootElement.TryGetProperty("count", out var countProp)
                    ? countProp.GetInt32()
                    : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка получения количества объявлений");
                return null;
            }
        }

        private static Uri BuildUriWithQuery(string baseUrl, Dictionary<string, List<string>> queryParams)
        {
            var uriBuilder = new UriBuilder(baseUrl);
            var query = new List<string>();

            foreach (var param in queryParams)
            {
                foreach (var value in param.Value)
                {
                    query.Add($"{Uri.EscapeDataString(param.Key)}={Uri.EscapeDataString(value)}");
                }
            }

            uriBuilder.Query = string.Join("&", query);
            return uriBuilder.Uri;
        }
    }
}