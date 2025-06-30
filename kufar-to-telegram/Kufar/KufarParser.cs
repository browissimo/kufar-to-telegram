using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace KufarParserApp.Kufar
{
    public class KufarParser
    {
        private readonly KufarClient _client;
        private readonly ILogger<KufarParser> _logger;

        private static readonly Dictionary<string, string> _keyMapping = new()
        {
            ["Комнат"] = "ad_rooms"
        };

        public KufarParser(KufarClient client, ILogger<KufarParser> logger)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<List<Dictionary<string, object>>> ExtractAdsAsync()
        {
            var allAds = new List<Dictionary<string, object>>();
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

                    var ads = ProcessAds(response);
                    allAds.AddRange(ads);

                    cursor = GetNextPageCursor(response);
                    if (string.IsNullOrEmpty(cursor))
                        break;

                    attempt = 0; // Сброс счетчика после успешного запроса
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка при получении страницы объявлений (попытка {Attempt}/{MaxAttempts})", attempt, maxAttempts);
                    if (attempt >= maxAttempts)
                        throw new ApplicationException($"Не удалось получить данные после {maxAttempts} попыток", ex);
                }
            }

            return allAds;
        }

        private List<Dictionary<string, object>> ProcessAds(JsonDocument response)
        {
            var result = new List<Dictionary<string, object>>();

            if (!response.RootElement.TryGetProperty("ads", out var adsElement) ||
                adsElement.ValueKind != JsonValueKind.Array)
            {
                _logger.LogWarning("Ответ не содержит массива объявлений");
                return result;
            }

            foreach (var adElement in adsElement.EnumerateArray())
            {
                try
                {
                    var parsedAd = ParseAd(adElement);
                    if (parsedAd != null)
                    {
                        result.Add(parsedAd);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка обработки объявления");
                }
            }

            return result;
        }

        private Dictionary<string, object>? ParseAd(JsonElement adElement)
        {
            try
            {
                var result = new Dictionary<string, object>
                {
                    ["subject"] = adElement.GetProperty("subject").GetString() ?? string.Empty,
                    ["price_byn"] = (adElement.GetProperty("price_byn").GetString() ?? "")
                        .Replace("р.", "").Trim(),
                    ["ad_link"] = adElement.GetProperty("ad_link").GetString() ?? string.Empty,
                    ["list_time"] = adElement.GetProperty("list_time").GetString() ?? string.Empty
                };

                ProcessImages(adElement, result);
                ProcessParameters(adElement, result);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка парсинга объявления");
                return null;
            }
        }

        private void ProcessImages(JsonElement adElement, Dictionary<string, object> result)
        {
            if (adElement.TryGetProperty("images", out var imagesElement) &&
                imagesElement.ValueKind == JsonValueKind.Array)
            {
                var imageUrls = new List<string>();
                foreach (var img in imagesElement.EnumerateArray())
                {
                    if (img.TryGetProperty("path", out var path) && path.ValueKind == JsonValueKind.String)
                    {
                        imageUrls.Add($"https://rms.kufar.by/v1/list_thumbs_2x/{path.GetString()}");
                    }
                }
                result["image_urls"] = imageUrls;
            }
            else
            {
                result["image_urls"] = new List<string>();
            }
        }

        private void ProcessParameters(JsonElement adElement, Dictionary<string, object> result)
        {
            if (!adElement.TryGetProperty("ad_parameters", out var paramsElement) ||
                paramsElement.ValueKind != JsonValueKind.Array)
                return;

            foreach (var param in paramsElement.EnumerateArray())
            {
                if (!param.TryGetProperty("pl", out var plElement)
                    || !_keyMapping.TryGetValue(plElement.GetString() ?? string.Empty, out var mappedKey))
                    continue;

                if (param.TryGetProperty("vl", out var vlElement))
                {
                    result[mappedKey] = vlElement.ValueKind switch
                    {
                        JsonValueKind.String => vlElement.GetString()!,
                        JsonValueKind.Number => vlElement.GetInt32(),
                        _ => vlElement.ToString()
                    };
                }
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
                            label.ValueKind == JsonValueKind.String &&
                            label.GetString() == "next" &&
                            page.TryGetProperty("token", out var token) &&
                            token.ValueKind == JsonValueKind.String)
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