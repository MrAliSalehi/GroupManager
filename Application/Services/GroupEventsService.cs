using GroupManager.Application.Contracts;
using Telegram.Bot;

namespace GroupManager.Application.Services;

public class GroupEventsService : Service
{
    public GroupEventsService(ITelegramBotClient client) : base(client)
    {
    }
}