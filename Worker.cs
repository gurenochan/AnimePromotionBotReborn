using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using System.Reflection;
using System.ComponentModel;

namespace AnimePromotionBotReborn
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;

        private readonly IServiceScopeFactory serviceScopeFactory;

        protected readonly TelegramBotClient botClient;

        private readonly List<Update> Updates = new List<Update>();

        private CancellationToken cancellation;

        private readonly List<Task> Tasks = new List<Task>();

        public Worker(ILogger<Worker> logger, IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            this.serviceScopeFactory = scopeFactory;

            IConfiguration configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

            botClient = new TelegramBotClient(configuration.GetSection("BotSettings")["Key"]);

            using (var scope=this.serviceScopeFactory.CreateScope())
            {
                foreach (Interfaces.BotModule botModule in scope.ServiceProvider.GetServices<Interfaces.BotModule>())
                {
                    try
                    { 
                        botModule.Init();
                    }
                    catch(Exception ex)
                    {
                        _logger.Log(LogLevel.Error, $"Initiating module of type {botModule.GetType()} has been failed.\n{ex.Message}\n{ex.StackTrace}");
                    }
                }
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {

            this.cancellation = stoppingToken;
            int offset = 0;
            while (!stoppingToken.IsCancellationRequested)
            {

                Update[] updates = await botClient.GetUpdatesAsync(cancellationToken: stoppingToken, offset: offset);
                foreach(Update update in updates)
                {
                    offset = update.Id + 1;
                    await PerformUpdate(update, stoppingToken);
                }
                Task wait = Task.Delay(1000);


                await wait;
            }
        }

        protected async Task PerformUpdate(Update update, CancellationToken cancellationToken=default)
        {
            switch(update.Type)
            {
                case Telegram.Bot.Types.Enums.UpdateType.Message:
                case Telegram.Bot.Types.Enums.UpdateType.ChannelPost:
                    if (update.Message.Type==Telegram.Bot.Types.Enums.MessageType.Text)
                    {
                        using(var scope= this.serviceScopeFactory.CreateScope())
                        {
                            IEnumerable<System.String> messageSplit = update.Message.Text.Split(" ");
                            System.String commandName = messageSplit.ElementAt(0).Split("@")[0].Replace("/", System.String.Empty);
                            if (update.Message.Chat.Type == Telegram.Bot.Types.Enums.ChatType.Group || update.Message.Chat.Type == Telegram.Bot.Types.Enums.ChatType.Supergroup && messageSplit.ElementAt(0).Contains("@"))
                            {
                                User botUser = await this.botClient.GetMeAsync(cancellationToken);
                                IEnumerable<System.String> splited = messageSplit.ElementAt(0).Split("@");
                                if (splited.Count()>1)
                                {
                                    if (splited.ElementAt(1) != botUser.Username) return;
                                }
                            }
                            IEnumerable<System.String> commandArgs = messageSplit.Skip(1);
                            foreach (Interfaces.BotModule botModule in scope.ServiceProvider.GetServices<Interfaces.BotModule>())
                            {
                                try
                                {

                                    var commandMethod = botModule.GetType().GetMethod(commandName);
                                    Task commandTask = (Task)commandMethod?.Invoke(botModule, new object[] { this.botClient, update.Message, commandArgs, cancellationToken });
                                    if (commandMethod != null) await commandTask.ConfigureAwait(false);

                                }
                                catch (AmbiguousMatchException)
                                {
                                    //_logger.Log(LogLevel.Information, "No method {0}.", new System.String[] { });
                                }
                                catch(Exception ex)
                                {
                                    _logger.Log(LogLevel.Error, ex.Message + Environment.NewLine + ex.StackTrace);
                                }
                            }
                        }
                    }
                    break;
            }
        }
    }
}
