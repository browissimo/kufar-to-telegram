using KufarParserApp.Kufar;
using KufarParserApp.Telegram;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MPBot.Services.Quartz;
using Telegram.Bot;

namespace KufarParserApp
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            try
            {
                var host = BuildHost(args);
                await host.RunAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Application failed: {ex.Message}");
                Environment.Exit(1);
            }
        }

        private static IHost BuildHost(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration(ConfigureAppConfiguration)
                .ConfigureServices(ConfigureServices)
                .Build();
        }

        private static void ConfigureAppConfiguration(HostBuilderContext hostBuilderContext, IConfigurationBuilder configurationBuilder)
        {
            configurationBuilder.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            configurationBuilder.AddEnvironmentVariables();
        }

        private static void ConfigureServices(HostBuilderContext hostBuilderContext, IServiceCollection services)
        {
            var configuration = hostBuilderContext.Configuration;

            ConfigureHttpClients(services, configuration);
            RegisterApplicationServices(services);
            ConfigureLogging(services);

            services.QuartzRegistration();
            services.QuartzJobRegistration();
        }

        private static void ConfigureHttpClients(IServiceCollection services, IConfiguration configuration)
        {
            services.AddHttpClient("telegram_bot_client")
                .AddTypedClient<ITelegramBotClient>((httpClient, _) =>
                {
                    var botToken = configuration.GetValue<string>("Telegram:BotToken")
                        ?? throw new InvalidOperationException("Telegram BotToken is not configured");

                    var options = new TelegramBotClientOptions(botToken);
                    return new TelegramBotClient(options, httpClient);
                });
        }

        private static void RegisterApplicationServices(IServiceCollection services)
        {
            services
                .AddTransient<TelegramNotify>()
                .AddTransient<KufarParser>()
                .AddTransient<KufarClient>();
        }

        private static void ConfigureLogging(IServiceCollection services)
        {
            services.AddLogging(configure =>
                configure
                    .AddConsole()
                    .AddDebug() 
                    .SetMinimumLevel(LogLevel.Debug));
        }
    }
}