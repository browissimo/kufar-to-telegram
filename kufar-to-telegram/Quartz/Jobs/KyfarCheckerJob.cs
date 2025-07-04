﻿using KufarParserApp.Kufar;
using KufarParserApp.Models;
using KufarParserApp.Telegram;
using Microsoft.Extensions.Logging;
using Quartz;

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

                var newAds = await _parser.ExtractAdsAsync();
                var newItems = newAds.Take(3).ToList();

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

        private async Task ProcessAdAsync(HomesModel ad)
        {
            try
            {
                var message = FormatAdMessage(ad);
                var imageUrls = ExtractImageUrls(ad);

                if (imageUrls.Any())
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

        private static List<string> ExtractImageUrls(HomesModel ad)
        {
            return ad.Images?
                .Where(i => !string.IsNullOrEmpty(i?.Path))
                .Select(i => $"https://rms.kufar.by/v1/list_thumbs_2x/{i.Path}")
                .ToList()
                ?? new List<string>();
        }

        private static string FormatAdMessage(HomesModel ad)
        {
            var parts = new List<string>();

            //if (!string.IsNullOrWhiteSpace(ad.Subject))
            //    parts.Add(ad.Subject);

            //if (!string.IsNullOrWhiteSpace(ad.BodyShort))
            //    parts.Add(ad.BodyShort);

            if(!string.IsNullOrWhiteSpace(ad.Subject))
                parts.Add(ad.BodyShort);
            else if (!string.IsNullOrWhiteSpace(ad.BodyShort))
                parts.Add(ad.Subject);

            if (!string.IsNullOrWhiteSpace(ad.PriceUsd))
                parts.Add($"Цена: {ad.PriceUsd[..^2]} USD");

            var squarePrice = ad.AdParameters?.FirstOrDefault(p => p.P == "square_meter")?.V;
            if (!string.IsNullOrWhiteSpace(squarePrice?.ToString()))
                parts.Add($"Цена за метр: {squarePrice}");

            var size = ad.AdParameters?.FirstOrDefault(p => p.P == "size")?.V;
            if (!string.IsNullOrWhiteSpace(size?.ToString()))
                parts.Add($"Общая площадь: {size}");

            var sizeLivingPlace = ad.AdParameters?.FirstOrDefault(p => p.P == "size_living_space")?.V;
            if (!string.IsNullOrWhiteSpace(sizeLivingPlace?.ToString()))
                parts.Add($"Жилая площадь: {sizeLivingPlace}");

            var rooms = ad.AdParameters?.FirstOrDefault(p => p.Pl == "Комнат")?.Vl;
            if (!string.IsNullOrWhiteSpace(rooms?.ToString()))
                parts.Add($"Комнат: {rooms}");

            if (!string.IsNullOrWhiteSpace(ad.AdLink))
                parts.Add($"Ссылка: {ad.AdLink}");

            return string.Join("\n", parts);
        }
    }
}
