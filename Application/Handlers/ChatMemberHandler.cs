using GroupManager.Application.Contracts;
using GroupManager.DataLayer.Controller;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace GroupManager.Application.Handlers;

public class ChatMemberHandler : HandlerBase
{
    public ChatMemberHandler(ITelegramBotClient client) : base(client)
    {
    }

    public async Task InitHandlerAsync(IEnumerable<User> users, Chat chat, CancellationToken ct)
    {
        var group = await GroupController.GetGroupByIdAsync(chat.Id, ct);
        if (group is null or { SayWelcome: false })
            return;


        foreach (var user in users)
        {
            await Client.SendTextMessageAsync(chat.Id, $"User @{user.Username}\n{group.WelcomeMessage}", cancellationToken: ct);
        }

    }

}