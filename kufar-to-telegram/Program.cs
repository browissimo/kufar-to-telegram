using KufarParserApp.Kufar;
using KufarParserApp.Telegram;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MPBot.Services.Quartz;
using System.Text.Json;
using Telegram.Bot;

//namespace KufarParserApp
//{
//    internal class Program
//    {
//        static async Task Main(string[] args)
//        {

//            var host = Host.CreateDefaultBuilder(args)
//                .ConfigureAppConfiguration((hostingContext, config) =>
//                {
//                    config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
//                })
//                .ConfigureServices(async (context, services) =>
//                {
//                    var configuration = context.Configuration;

//                    services.AddHttpClient("telegram_bot_client")
//                        .AddTypedClient<ITelegramBotClient>((httpClient, sp) =>
//                        {
//                            var botToken = configuration.GetValue<string>("Telegram:BotToken");
//                            var options = new TelegramBotClientOptions(botToken);
//                            return new TelegramBotClient(options, httpClient);
//                        });

//                    services
//                        .AddTransient<TelegramNotifier>()
//                        .AddTransient<KufarParser>()
//                        .AddTransient<KufarClient>()
//                        .AddLogging(configure => configure.AddConsole());

//                    services.QuartzRegistration();
//                    services.QuartzJobRegistration();
//                }).Build();

//            await host.RunAsync();


//            Console.WriteLine("нужно вывести это");
//        }
//    }
//}



internal class Program
{
    static async Task Main(string[] args)
    {
        var host = Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((hostingContext, config) =>
            {
                config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            })
            .ConfigureServices((context, services) =>
            {
                var configuration = context.Configuration;

                services.AddHttpClient("telegram_bot_client")
                    .AddTypedClient<ITelegramBotClient>((httpClient, sp) =>
                    {
                        var botToken = configuration.GetValue<string>("Telegram:BotToken");
                        var options = new TelegramBotClientOptions(botToken);
                        return new TelegramBotClient(options, httpClient);
                    });

                services
                    .AddTransient<TelegramNotifier>()
                    .AddTransient<KufarParser>()
                    .AddTransient<KufarClient>()
                    .AddLogging(configure => configure.AddConsole());

                //services.QuartzRegistration();
                //services.QuartzJobRegistration();
            })
            .Build();

        // 🔻 Получаем зависимости из DI
        var logger = host.Services.GetRequiredService<ILogger<Program>>();
        var parser = host.Services.GetRequiredService<KufarParser>();
        var notifier = host.Services.GetRequiredService<TelegramNotifier>();

        logger.LogInformation("🚀 Парсер запущен");

        var oldAds = await parser.ExtractAdsAsync();
        var newAds = await parser.ExtractAdsAsync();
        //var newItems = CompareAds(oldAds, newAds);
        var newItems = newAds.Take(3).ToList();

        if (newItems.Any())
        {
            logger.LogInformation("Найдено новых объявлений: {Count}", newItems.Count);
        }

        foreach (var ad in newItems)
        {
            await ProcessAdAsync(ad, notifier, logger);
        }

        await host.RunAsync();
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

    private static async Task ProcessAdAsync(
        Dictionary<string, object> ad,
        TelegramNotifier notifier,
        ILogger logger)
    {
        try
        {
            var message = FormatAdMessage(ad);
            if (ad.TryGetValue("image_urls", out var images) && images is List<string> imageUrls)
            {
                await notifier.SendPhotosAsync(imageUrls, message);
            }
            else
            {
                await notifier.SendMessageAsync(message);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка при обработке объявления");
        }
    }

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
