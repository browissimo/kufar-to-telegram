using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using KufarParserApp.Models;
using Microsoft.Extensions.Logging;

namespace KufarParserApp.Kufar
{
    public class KufarParser
    {
        private readonly KufarClient _client;
        private readonly ILogger<KufarParser> _logger;

        public KufarParser(KufarClient client, ILogger<KufarParser> logger)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<List<HomesModel>> ExtractAdsAsync()
        {
            var allAds = new List<HomesModel>();
            string cursor = string.Empty;
            int attempt = 0;
            const int maxAttempts = 3;

            while (attempt < maxAttempts)
            {
                attempt++;
                try
                {
                    using var response = await _client.FetchPageAsync(cursor);
                    if (response == null)
                    {
                        _logger.LogWarning("Не удалось получить страницу (попытка {Attempt}/{MaxAttempts})", attempt, maxAttempts);
                        continue;
                    }

                    var ads = ParseResponse(response);
                    allAds.AddRange(ads);

                    cursor = GetNextPageCursor(response);
                    if (string.IsNullOrEmpty(cursor))
                        break;

                    attempt = 0; 
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка при получении страницы (попытка {Attempt}/{MaxAttempts})", attempt, maxAttempts);
                    if (attempt >= maxAttempts)
                        throw new ApplicationException($"Не удалось получить данные после {maxAttempts} попыток", ex);
                }
            }

            return allAds;
        }

        private List<HomesModel> ParseResponse(JsonDocument doc)
        {
            try
            {
                if (!doc.RootElement.TryGetProperty("ads", out var ads) || ads.ValueKind != JsonValueKind.Array)
                {
                    _logger.LogWarning("Ответ не содержит массива объявлений");
                    return new List<HomesModel>();
                }

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    NumberHandling = JsonNumberHandling.AllowReadingFromString,
                };

                var result = new List<HomesModel>();

                foreach (var element in ads.EnumerateArray())
                {
                    try
                    {
                        var ad = JsonSerializer.Deserialize<HomesModel>(element.GetRawText(), options);
                        if (ad != null)
                            result.Add(ad);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Ошибка десериализации объявления");
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка обработки JSON-ответа");
                return new List<HomesModel>();
            }
        }

        private string GetNextPageCursor(JsonDocument response)
        {
            try
            {
                if (response.RootElement.TryGetProperty("pagination", out var pagination) &&
                    pagination.TryGetProperty("pages", out var pages) &&
                    pages.ValueKind == JsonValueKind.Array)
                {
                    foreach (var page in pages.EnumerateArray())
                    {
                        if (page.TryGetProperty("label", out var label) &&
                            label.GetString() == "next" &&
                            page.TryGetProperty("token", out var token))
                        {
                            return token.GetString() ?? string.Empty;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении курсора следующей страницы");
            }

            return string.Empty;
        }
    }
}
