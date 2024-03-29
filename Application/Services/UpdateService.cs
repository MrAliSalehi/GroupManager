﻿using GroupManager.Application.Handlers;
using GroupManager.DataLayer.Context;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace GroupManager.Application.Services
{
    public class UpdateService : BackgroundService
    {
        private readonly TelegramBotClient _client;
        private readonly MyChatMemberHandler _myChatMemberHandler;
        private readonly ChatMemberHandler _chatMemberHandler;
        private readonly MessageHandler _messageHandler;
        private readonly CallBackHandler _callBackHandler;


        public UpdateService()
        {
            var token = Globals.BotConfigs.Token;
            _client = new TelegramBotClient(token);
            _myChatMemberHandler = new MyChatMemberHandler(_client);
            _messageHandler = new MessageHandler(_client);
            _callBackHandler = new CallBackHandler(_client);
            _chatMemberHandler = new ChatMemberHandler(_client);
        }


        public static event EventHandler<Update> Update;
        private static void OnUpdated(Update e)
        {
            Update?.Invoke(null, e);
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                var me = await _client.GetMeAsync(cancellationToken);
                await _client.SendTextMessageAsync(ManagerConfig.Admins.First(), "Bot Has Been Started", cancellationToken: cancellationToken);
                ManagerConfig.BotUserName = me.Username ?? "-";

                _client.StartReceiving(OnUpdate, OnError, cancellationToken: cancellationToken);
                await base.StartAsync(cancellationToken);
            }
            catch (Exception e)
            {
                Log.Error(e, nameof(StartAsync));
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.CompletedTask;
        }

        private async Task OnUpdate(ITelegramBotClient client, Update update, CancellationToken ct)
        {
            try
            {
                OnUpdated(update);
                var updateHandler = update.Type switch
                {
                    UpdateType.Message when (update.Message is { NewChatMembers: null }) => _messageHandler.InitHandlerAsync(update.Message, ct),

                    UpdateType.Message when (update.Message?.NewChatMembers is not null) => _chatMemberHandler.UserJoinedChatAsync(update.Message.NewChatMembers.ToList(), update.Message.Chat, ct),

                    //UpdateType.ChatMember when (update.ChatMember is not null) => _chatMemberHandler.InitHandlerAsync(update.ChatMember, ct),

                    UpdateType.MyChatMember when (update.MyChatMember is not null) => _myChatMemberHandler.InitHandlerAsync(update.MyChatMember, ct),

                    UpdateType.CallbackQuery when (update.CallbackQuery is not null) => _callBackHandler.InitHandlerAsync(update.CallbackQuery, ct),

                    UpdateType.ChatJoinRequest => Task.CompletedTask,

                    UpdateType.Unknown => Task.CompletedTask,
                    _ => Task.CompletedTask
                };

                await updateHandler;
            }
            catch (Exception e)
            {
                Log.Error(e, "OnUpdate");
            }

        }

        private static async Task OnError(ITelegramBotClient client, Exception ex, CancellationToken ct)
        {
            Log.Error(ex, "OnError");
            await Task.CompletedTask;
        }


    }
}