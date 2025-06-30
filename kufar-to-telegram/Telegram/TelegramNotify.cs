using kufar_to_telegram.Telegram;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace KufarParserApp.Telegram;

public class TelegramNotify
{
    private const int MaxPhotosPerRequest = 10;
    private const string BaseUrl = "https://api.telegram.org";

    private readonly string _botToken;
    private readonly string _chatId;
    private readonly HttpClient _httpClient;
    private readonly ILogger<TelegramNotify> _logger;

    public TelegramNotify(IConfiguration configuration, ILogger<TelegramNotify> logger)
    {
        var telegramConfig = configuration.GetSection("Telegram");
        _botToken = GetRequiredConfigValue(telegramConfig, "BotToken");
        _chatId = GetRequiredConfigValue(telegramConfig, "ChatId");

        _httpClient = new HttpClient();
        _logger = logger;
    }

    public async Task<JsonDocument?> SendMessageAsync(string text)
    {
        var payload = new Dictionary<string, string>
        {
            ["chat_id"] = _chatId,
            ["text"] = text
        };

        try
        {
            using var content = new FormUrlEncodedContent(payload);
            using var response = await _httpClient.PostAsync(BuildApiUrl("sendMessage"), content);
            response.EnsureSuccessStatusCode();
            return await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка отправки сообщения");
            return null;
        }
    }

    public async Task<JsonDocument?> SendPhotosAsync(List<string> photoUrls, string caption = "")
    {
        if (photoUrls is not { Count: > 0 })
            return null;

        var builder = new MediaGroupBuilder(caption);

        for (int i = 0; i < Math.Min(photoUrls.Count, MaxPhotosPerRequest); i++)
        {
            var url = photoUrls[i];
            try
            {
                var photoBytes = await _httpClient.GetByteArrayAsync(url);
                builder.AddPhoto(photoBytes);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Ошибка при загрузке фото: {Url}", url);
            }
        }

        if (!builder.HasMedia)
            return null;

        var content = builder.Build(_chatId);
        return await SendMultipartToTelegram(content);
    }

    private async Task<JsonDocument?> SendMultipartToTelegram(MultipartFormDataContent content)
    {
        try
        {
            using var response = await _httpClient.PostAsync(BuildApiUrl("sendMediaGroup"), content);
            var body = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Telegram API Error: {Code} {Reason}\n{Body}", (int)response.StatusCode, response.ReasonPhrase, body);
                return null;
            }

            return await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка отправки медиа-группы");
            return null;
        }
    }

    private string BuildApiUrl(string methodName) => $"{BaseUrl}/bot{_botToken}/{methodName}";

    private static string GetRequiredConfigValue(IConfigurationSection section, string key)
    {
        return section[key] ?? throw new ArgumentNullException(
            $"Telegram:{key} is not configured");
    }
}
