using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using AnimePromotionBotReborn.Interfaces;
using Microsoft.Extensions.Logging;

namespace AnimePromotionBotReborn.Modules
{
    public class Promotion:BotModule
    {
        protected readonly DBContext _context;
        private readonly ILogger<Promotion> _logger;

        protected long MainChatId
        {
            get
            {
                long id = 0;
                try
                {
                    id = Convert.ToInt64(System.IO.File.ReadAllText("ChatId.txt"));
                }
                catch
                { }
                return id;
            }
            set
            {
                try
                { 
                    System.IO.File.WriteAllText("ChatId.txt", value.ToString());
                }
                catch { }
            }
        }

        public Promotion(DBContext context, ILogger<Promotion> logger)
        {
            this._context = context;
        }

        public override void Init() => this._context.Database.EnsureCreated();

        public async Task changechat(TelegramBotClient client, Message message, IEnumerable<System.String> args, CancellationToken cancellationToken)
        {
            if (!(message.Chat.Type == Telegram.Bot.Types.Enums.ChatType.Group || message.Chat.Type == Telegram.Bot.Types.Enums.ChatType.Supergroup)) return;
            try
            {

                ChatMember[] chatAdmins = await client.GetChatAdministratorsAsync(message.Chat.Id, cancellationToken);
                if (!chatAdmins.Any(p => p.User.Id == message.From.Id)) return;

                this.MainChatId = message.Chat.Id;

                await client.SendTextMessageAsync(message.Chat.Id, "Chat changed successfully.");
            }
            catch(Exception ex)
            {
                _logger.Log(LogLevel.Error, "Changing chat failed.\n{0}\n{1}", new object[] { ex.Message, ex.StackTrace });
                await client.SendTextMessageAsync(message.Chat.Id, "Oops… Something went wrong…");
            }
        }

    }
}
