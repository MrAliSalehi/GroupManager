using GroupManager.Application.Contracts;
using Telegram.Bot;

namespace GroupManager.Application.Services;

public class MessageService : Service
{
    public MessageService(ITelegramBotClient client) : base(client)
    {
    }
}