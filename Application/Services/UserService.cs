using GroupManager.Application.Contracts;
using Telegram.Bot;

namespace GroupManager.Application.Services;

public class UserService : Service
{
    public UserService(ITelegramBotClient client) : base(client)
    {
    }
}