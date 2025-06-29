using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace KufarParserApp.Telegram
{
    public class TelegramNotifier
    {
        private readonly string _apiUrl;
        private readonly string _chatId;
        private readonly HttpClient _httpClient;
        private readonly ILogger<TelegramNotifier> _logger;

        public TelegramNotifier(IConfiguration configuration, ILogger<TelegramNotifier> logger)
        {
            // Получаем настройки из конфигурации
            var telegramConfig = configuration.GetSection("Telegram");
            var botToken = telegramConfig["BotToken"] ??
                throw new ArgumentNullException("Telegram:BotToken не найден в конфигурации");
            _chatId = telegramConfig["ChatId"] ??
                throw new ArgumentNullException("Telegram:ChatId не найден в конфигурации");

            _apiUrl = $"https://api.telegram.org/bot{botToken}";
            _httpClient = new HttpClient();
            _logger = logger;
        }

        public async Task<JsonDocument?> SendMessageAsync(string text)
        {
            var url = $"{_apiUrl}/sendMessage";
            var payload = new Dictionary<string, string>
            {
                {"chat_id", _chatId},
                {"text", text}
            };

            try
            {
                using var response = await _httpClient.PostAsync(url, new FormUrlEncodedContent(payload));
                response.EnsureSuccessStatusCode();
                return await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка отправки сообщения");
                return null;
            }
        }

        //public async Task<JsonDocument?> SendPhotosAsync(List<string> photoUrls, string caption = "")
        //{
        //    if (!photoUrls?.Any() ?? true)
        //        return null;

        //    var url = $"{_apiUrl}/sendMediaGroup";
        //    var media = new List<Dictionary<string, string>>();
        //    var multipartContent = new MultipartFormDataContent();

        //    // Добавляем медиа-группу
        //    for (int i = 0; i < photoUrls.Count; i++)
        //    {
        //        try
        //        {
        //            var photoBytes = await _httpClient.GetByteArrayAsync(photoUrls[i]);
        //            var content = new ByteArrayContent(photoBytes);
        //            content.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");

        //            //var fieldName = $"photo_{i}";
        //            //multipartContent.Add(content, fieldName, $"photo_{i}.jpg");
        //            var attachName = $"photo_{i}";

        //            multipartContent.Add(content, attachName, attachName);


        //            var mediaItem = new Dictionary<string, string>
        //            {
        //                {"type", "photo"},
        //                {"media", $"attach://{attachName}"}
        //            };

        //            if (i == 0) mediaItem.Add("caption", caption);
        //            media.Add(mediaItem);
        //        }
        //        catch (Exception ex)
        //        {
        //            _logger.LogWarning(ex, "Не удалось загрузить фото {PhotoUrl}", photoUrls[i]);
        //        }
        //    }

        //    if (!media.Any())
        //        return null;

        //    multipartContent.Add(new StringContent(_chatId), "chat_id");
        //    multipartContent.Add(new StringContent(JsonSerializer.Serialize(media)), "media");

        //    try
        //    {
        //        using var response = await _httpClient.PostAsync(url, multipartContent);
        //        response.EnsureSuccessStatusCode();
        //        return await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Ошибка отправки медиа-группы");
        //        return null;
        //    }
        //}
        public async Task<JsonDocument?> SendPhotosAsync(List<string> photoUrls, string caption = "")
        {
            if (photoUrls == null || photoUrls.Count == 0)
                return null;

            var url = $"{_apiUrl}/sendMediaGroup";
            using var multipartContent = new MultipartFormDataContent();

            var mediaList = new List<Dictionary<string, object>>();

            for (int i = 0; i < Math.Min(photoUrls.Count, 10); i++)
            {
                try
                {
                    var photoBytes = await _httpClient.GetByteArrayAsync(photoUrls[i]);
                    var content = new ByteArrayContent(photoBytes);
                    content.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");

                    var attachName = $"photo_{i}";
                    multipartContent.Add(content, attachName, attachName);

                    var mediaObject = new Dictionary<string, object>
                    {
                        { "type", "photo" },
                        { "media", $"attach://{attachName}" }
                    };

                    if (i == 0 && !string.IsNullOrWhiteSpace(caption))
                    {
                        mediaObject.Add("caption", caption);
                    }

                    mediaList.Add(mediaObject);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Ошибка при загрузке фото: {Url}", photoUrls[i]);
                }
            }

            if (!mediaList.Any())
                return null;

            multipartContent.Add(new StringContent(_chatId), "chat_id");

            var mediaJson = JsonSerializer.Serialize(mediaList, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
            multipartContent.Add(new StringContent(mediaJson, Encoding.UTF8, "application/json"), "media");

            try
            {
                using var response = await _httpClient.PostAsync(url, multipartContent);
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


    }
}