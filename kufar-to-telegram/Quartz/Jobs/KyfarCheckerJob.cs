using KufarParserApp.Kufar;
using KufarParserApp.Telegram;
using Microsoft.Extensions.Logging;
using Quartz;
using System.Text.Json;

namespace TelegramTestProject.Jobs
{
    public class KyfarCheckerJob : IJob
    {
        private readonly ILogger<KyfarCheckerJob> _logger;
        private readonly KufarParser _parser;
        private readonly TelegramNotify _notifier;

        public KyfarCheckerJob(
            ILogger<KyfarCheckerJob> logger,
            KufarParser parser,
            TelegramNotify notifier)
        {
            _logger = logger;
            _parser = parser;
            _notifier = notifier;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                _logger.LogInformation("🚀 Парсер запущен");

                var oldAds = await _parser.ExtractAdsAsync();
                var newAds = await _parser.ExtractAdsAsync();
                var newItems = newAds.Take(3).ToList();

                //var oldAds = await _parser.ExtractAdsAsync();
                //var newAds = await _parser.ExtractAdsAsync();
                //var newItems = CompareAds(oldAds, newAds);

                if (newItems.Any())
                {
                    _logger.LogInformation("Найдено новых объявлений: {Count}", newItems.Count);
                }

                foreach (var ad in newItems)
                {
                    await ProcessAdAsync(ad);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Критическая ошибка при выполнении задачи KyfarCheckerJob");
            }
        }

        private async Task ProcessAdAsync(Dictionary<string, object> ad)
        {
            try
            {
                var message = FormatAdMessage(ad);
                if (ad.TryGetValue("image_urls", out var images) && images is List<string> imageUrls)
                {
                    await _notifier.SendPhotosAsync(imageUrls, message);
                }
                else
                {
                    await _notifier.SendMessageAsync(message);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обработке объявления");
            }
        }

        private static List<Dictionary<string, object>> CompareAds(
            List<Dictionary<string, object>> oldAds,
            List<Dictionary<string, object>> newAds)
        {
            var oldSet = oldAds.Select(DictToHashable).ToHashSet();
            var newSet = newAds.Select(DictToHashable).ToHashSet();
            return newSet.Except(oldSet)
                .Select(x => JsonSerializer.Deserialize<Dictionary<string, object>>(x)!)
                .ToList();
        }

        private static string DictToHashable(Dictionary<string, object> d)
        {
            return JsonSerializer.Serialize(d.OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value));
        }

        //private static string FormatAdMessage(Dictionary<string, object> ad)
        //{
        //    var parts = new List<string> { $"{ad["subject"]}", $"Цена: {ad["price_byn"]} BYN" };

        //    AddPartIfExists(ad, "ad_processor", "Проц: {0}", parts);
        //    AddPartIfExists(ad, "ad_ram", "ОЗУ: {0}", parts);
        //    AddPartIfExists(ad, "ad_display", "Диагональ: {0}", parts);
        //    AddPartIfExists(ad, "ad_disk_type", "Диск: {0}", parts);
        //    AddPartIfExists(ad, "ad_disk_volume", "Объем: {0}", parts);
        //    AddPartIfExists(ad, "ad_battery", "АКБ: {0}", parts);

        //    parts.Add($"Ссылка: {ad["ad_link"]}");
        //    return string.Join("\n", parts);
        //}

        private static string FormatAdMessage(Dictionary<string, object> ad)
        {
            var parts = new List<string> { $"{ad["subject"]}", $"Цена: {ad["price_byn"]} BYN" };

            //AddPartIfExists(ad, "ad_processor", "Проц: {0}", parts);
            //AddPartIfExists(ad, "ad_ram", "ОЗУ: {0}", parts);
            //AddPartIfExists(ad, "ad_display", "Диагональ: {0}", parts);
            //AddPartIfExists(ad, "ad_disk_type", "Диск: {0}", parts);
            //AddPartIfExists(ad, "ad_disk_volume", "Объем: {0}", parts);
            //AddPartIfExists(ad, "ad_battery", "АКБ: {0}", parts);
            AddPartIfExists(ad, "ad_rooms", "Комнат: {0}", parts);

            parts.Add($"Ссылка: {ad["ad_link"]}");
            return string.Join("\n", parts);
        }

        private static void AddPartIfExists(
            Dictionary<string, object> ad,
            string key,
            string format,
            List<string> parts)
        {
            if (ad.TryGetValue(key, out var value) && value != null)
            {
                parts.Add(string.Format(format, value));
            }
        }
    }
}