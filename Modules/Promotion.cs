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
using AnimePromotionBotReborn.Models;
using Microsoft.EntityFrameworkCore;

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

        public async Task changechat(TelegramBotClient client, Message message, IEnumerable<System.String> args, CancellationToken cancellationToken=default)
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

        protected async Task<bool> isUserFromGroup(TelegramBotClient client, User user, CancellationToken cancellationToken=default)
        {
            try
            {
                ChatMember member = await client.GetChatMemberAsync(this.MainChatId, user.Id, cancellationToken);
                return member != null;
            }
            finally { }
            return false;
        }

        protected async Task<bool> isMainChat(TelegramBotClient client, Chat chat, CancellationToken cancellationToken= default)
        {
            if (chat.Id == this.MainChatId)
            {
                await client.SendTextMessageAsync(chat.Id, "You should execute this command <b>right in your channel</b>.", Telegram.Bot.Types.Enums.ParseMode.Html, cancellationToken: cancellationToken);
                return true;
            }
            else return false;
        }

        public async Task addchannel(TelegramBotClient client, Message message, IEnumerable<System.String> args, CancellationToken cancellationToken=default)
        {
            if (message.Chat.Type == Telegram.Bot.Types.Enums.ChatType.Private || message.Chat.Type==Telegram.Bot.Types.Enums.ChatType.Sender) return;
            if (await this.isMainChat(client, message.Chat, cancellationToken)) return;
            if (!(await this.isUserFromGroup(client, message.From, cancellationToken))) return;

            try
            {
                IQueryable<Models.HumanType> humanTypes = this._context.HumanTypes;
                HumanType humanType = null;
                if (args.Count()>0)
                {
                    humanType = humanTypes.Single(p => args.Any(o => o.ToLower() == p.Name.ToLower()));
                }
                else
                {
                    humanType = humanTypes.OrderBy(p => p.Id).First();
                }
                Models.Channel channel = new Models.Channel()
                {
                    Tid = message.Chat.Id,
                    EnablePromotion = true,
                    IsActive = true,
                    HumanTypeId = humanType.Id
                };
                this._context.Channels.Add(channel);
                await this._context.SaveChangesAsync(cancellationToken);

            }
            catch (Exception ex)
            {
                this._logger.Log(LogLevel.Error, ex.Message + Environment.NewLine + ex.StackTrace);
                await client.SendTextMessageAsync(message.Chat.Id, "Oops… Something went wrong…\nTry again.", cancellationToken: cancellationToken);
            }
        }

    }
}
