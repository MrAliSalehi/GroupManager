using GroupManager.Application.Contracts;
using GroupManager.DataLayer.Controller;
using GroupManager.DataLayer.Models;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using User = Telegram.Bot.Types.User;

namespace GroupManager.Application.Handlers;

public class CallBackHandler : HandlerBase
{
    private static readonly ChatPermissions UnMuteChatPermissions = new()
    {
        CanSendOtherMessages = true,
        CanSendMessages = true,
        CanSendMediaMessages = true,
        CanInviteUsers = true
    };
    public CallBackHandler(ITelegramBotClient client) : base(client)
    {
    }

    public async Task InitHandlerAsync(CallbackQuery callback, CancellationToken ct)
    {
        if (callback.Data is null or "" or ConstData.IgnoreMe)
            return;

        var dataArr = callback.Data.Split(':');
        switch (dataArr[0])
        {
            case nameof(InlineButtons.Admin):
                await AdminCallbacksAsync(callback, dataArr, ct);
                break;
            case nameof(InlineButtons.Member):
                await MemberCallbacksAsync(callback, dataArr, ct);
                break;
            default:
                break;
        }
        await Task.CompletedTask;
    }

    private async Task AdminCallbacksAsync(CallbackQuery callback, IReadOnlyList<string> data, CancellationToken ct)
    {
        if (!ManagerConfig.Admins.Contains(callback.From.Id))
        {
            await Client.AnswerCallbackQueryAsync(callback.Id, "You Are Not Admin of Bot!", cancellationToken: ct);
            return;
        }

        if (callback.Message is null)
            return;

        var group = await GroupController.GetGroupByIdAsync(callback.Message.Chat.Id, ct);
        if (group is null)
            return;

        switch (data[1])
        {
            case nameof(InlineButtons.Admin.SettingMenu):
                {
                    switch (data[2])
                    {
                        case nameof(InlineButtons.Admin.Warn):
                            await Client.EditMessageTextAsync(callback.Message.Chat.Id, callback.Message.MessageId, ConstData.MessageOfWarnMenu, replyMarkup: InlineButtons.Admin.Warn.GetMenu(group), cancellationToken: ct);
                            break;

                        case nameof(InlineButtons.Admin.Curse):
                            await Client.EditMessageTextAsync(callback.Message.Chat.Id, callback.Message.MessageId, ConstData.MessageOfCurseMenu, replyMarkup: InlineButtons.Admin.Curse.GetMenu(group), cancellationToken: ct);
                            break;

                        case nameof(InlineButtons.Admin.General):
                            await Client.EditMessageTextAsync(callback.Message.Chat.Id, callback.Message.MessageId, ConstData.MessageOfGeneralMenu, replyMarkup: InlineButtons.Admin.General.GetMenu(group), cancellationToken: ct);
                            break;

                        case ConstData.Close:
                            await Client.EditMessageTextAsync(callback.Message.Chat.Id, callback.Message.MessageId, "<i>Panel Closed</i>", ParseMode.Html,
                                cancellationToken: ct);
                            break;
                    }
                    break;
                }
            case nameof(InlineButtons.Admin.Warn):
                await AdminWarnMenuAsync(callback, data, group, ct);
                break;
            case nameof(InlineButtons.Admin.Curse):
                await AdminCurseMenuAsync(callback, data, ct);
                break;
            case nameof(InlineButtons.Admin.Curse.MuteTimeModify):
                await ModifyMuteTimeAsync(callback, data, group, ct);
                break;
            case nameof(InlineButtons.Admin.ConfirmChat):
                await GroupController.AddGroupAsync(callback.Message.Chat.Id, ct);
                await Client.EditMessageTextAsync(callback.Message.Chat.Id, callback.Message.MessageId, "<b>Bot is Now Active</b>", ParseMode.Html, cancellationToken: ct);

                break;
            case nameof(InlineButtons.Admin.General):
                await GeneralSettingsAsync(callback, data, group, ct);
                break;
            case nameof(InlineButtons.Admin.General.AntiLink):
                await AntiLinkSettingsAsync(callback, data, group, ct);
                break;

        }

    }

    private async Task AntiLinkSettingsAsync(CallbackQuery callback, IReadOnlyList<string> data, Group group, CancellationToken ct)
    {
        if (callback.Message is null)
            return;

        switch (data[2])
        {
            case ConstData.TelegramFilterLink:
            case ConstData.PublicFilterLink:
            case ConstData.FilterTag:
            case ConstData.FilterId:
                {
                    var updatedGroup = await GroupController.UpdateGroupAsync(p =>
                    {
                        if (data[2] is ConstData.TelegramFilterLink)
                            p.FilterTelLink = !p.FilterTelLink;

                        if (data[2] is ConstData.PublicFilterLink)
                            p.FilterPublicLink = !p.FilterPublicLink;

                        if (data[2] is ConstData.FilterTag)
                            p.FilterHashTag = !p.FilterHashTag;

                        if (data[2] is ConstData.FilterId)
                            p.FilterId = !p.FilterId;

                    }, callback.Message.Chat.Id, ct);
                    if (updatedGroup is null)
                        return;

                    var keyboard = InlineButtons.Admin.General.GetAntiLinkMenu(updatedGroup);
                    await Client.EditMessageReplyMarkupAsync(callback.Message.Chat.Id, callback.Message.MessageId,
                        keyboard, ct);
                }
                break;

            case ConstData.Back:
                {
                    var keyboard = InlineButtons.Admin.General.GetMenu(group);
                    await Client.EditMessageTextAsync(callback.Message.Chat.Id, callback.Message.MessageId, ConstData.MessageOfGeneralMenu, replyMarkup: keyboard, cancellationToken: ct);
                    break;
                }
        }
    }

    private async Task MemberCallbacksAsync(CallbackQuery callBack, IReadOnlyList<string> data, CancellationToken ct = default)
    {
        switch (data[1])
        {
            case "Force":
                await ForceJoinAsync(callBack, data, ct);
                break;
        }


    }

    private async Task ForceJoinAsync(CallbackQuery callback, IReadOnlyList<string> data, CancellationToken ct = default)
    {
        if (callback.Message is null)
            return;

        var canParse = long.TryParse(data[2], out var userId);
        if (!canParse)
            return;
        if (userId != callback.From.Id)
        {
            await Client.AnswerCallbackQueryAsync(callback.Id, "This Button Is Not Meant For You!", true, cancellationToken: ct);
            return;

        }
        var fullGroup = await GroupController.GetGroupByIdIncludeChannelAsync(callback.Message.Chat.Id, ct);
        if (fullGroup is null)
            return;

        var notJoinedList = await ChatMemberHandler.CheckUserChatMemberAsync(callback.From.Id, fullGroup.ForceJoinChannel, Client, ct);
        if (notJoinedList.Count == 0)
        {
            await Client.RestrictChatMemberAsync(callback.Message.Chat.Id, callback.From.Id, UnMuteChatPermissions,
                cancellationToken: ct);
            await Client.AnswerCallbackQueryAsync(callback.Id, "You Can Chat Now!", true, cancellationToken: ct);
            await Client.DeleteMessageAsync(callback.Message.Chat.Id, callback.Message.MessageId, ct);

        }
        else
        {
            await Client.AnswerCallbackQueryAsync(callback.Id, "You Are Not Joined In Channels!", true,
                cancellationToken: ct);
            return;
        }
    }

    private async Task ModifyMuteTimeAsync(CallbackQuery callback, IReadOnlyList<string> data, Group group, CancellationToken ct = default)
    {
        if (callback.Message is null)
            return;

        switch (data[2])
        {
            case ConstData.HourPlus:
                break;
            case ConstData.HourMinus:
                break;
            case ConstData.MinutePlus:
                break;
            case ConstData.MinuteMinus:
                break;

            case ConstData.Back:
                await Client.EditMessageTextAsync(callback.Message.Chat.Id, callback.Message.MessageId,
                    ConstData.MessageOfCurseMenu, replyMarkup: InlineButtons.Admin.Curse.GetMenu(group), cancellationToken: ct);
                break;
        }
    }

    private async Task AdminCurseMenuAsync(CallbackQuery callback, IReadOnlyList<string> data, CancellationToken ct = default)
    {
        if (callback.Message?.ReplyMarkup is null)
            return;
        switch (data[2])
        {
            case ConstData.Ban:
            case ConstData.Mute:
            case ConstData.Warn:
                {
                    var updatedProperty = "";
                    var buttonName = "";
                    var updatedGroup = await GroupController.UpdateGroupAsync(gp =>
                    {
                        if (data[2] is ConstData.Ban)
                        {
                            updatedProperty = nameof(gp.BanOnCurse);
                            buttonName = "Ban ";
                            gp.BanOnCurse = !gp.BanOnCurse;
                        }

                        if (data[2] is ConstData.Mute)
                        {
                            buttonName = "Mute ";
                            updatedProperty = nameof(gp.MuteOnCurse);
                            gp.MuteOnCurse = !gp.MuteOnCurse;
                        }

                        if (data[2] is ConstData.Warn)
                        {
                            buttonName = "Warn ";
                            updatedProperty = nameof(gp.WarnOnCurse);
                            gp.WarnOnCurse = !gp.WarnOnCurse;
                        }

                    }, callback.Message.Chat.Id, ct);
                    if (updatedGroup is null)
                        return;

                    var prop = updatedGroup.GetType().GetProperties().SingleOrDefault(p => p.Name == updatedProperty);
                    if (prop is null)
                        return;
                    var value = (bool)prop.GetValue(updatedGroup)!;

                    InlineButtons.ChangeButtonValue(buttonName, callback.Message.ReplyMarkup, button =>
                    {
                        button.Text = $"{buttonName} " + (value ? ConstData.TrueEmoji : ConstData.FalseEmoji);
                    });

                    await Client.EditMessageReplyMarkupAsync(callback.Message.Chat.Id, callback.Message.MessageId,
                        callback.Message.ReplyMarkup, ct);
                    break;
                }

            case nameof(InlineButtons.Admin.Curse.MuteTimeModify):
                await Client.EditMessageTextAsync(callback.Message.Chat.Id, callback.Message.MessageId, ConstData.MessageOfModifyMuteTimeMenu,
                    replyMarkup: InlineButtons.Admin.Curse.MuteTimeModify, cancellationToken: ct);
                break;

            case ConstData.Back:
                await Client.EditMessageTextAsync(callback.Message.Chat.Id, callback.Message.MessageId, ConstData.MessageOfMainMenu,
                    replyMarkup: InlineButtons.Admin.SettingMenu, cancellationToken: ct);
                break;
        }
    }

    private async Task AdminWarnMenuAsync(CallbackQuery callback, IReadOnlyList<string> data, Group group, CancellationToken ct)
    {
        if (callback.Message?.ReplyMarkup is null)
            return;

        Group? updatedGroup;
        switch (data[2])
        {
            case ConstData.Minus:
            case ConstData.Plus:

                var op = data[2] is ConstData.Plus ? group.MaxWarns + 1 : group.MaxWarns - 1;
                var updatedKeyboard = InlineButtons.Admin.Warn.GetMenu(group);
                updatedKeyboard.InlineKeyboard.First().First().Text = $"Max Warns {op}";

                await GroupController.UpdateGroupAsync(p =>
                {
                    if (data[2] is ConstData.Plus)
                        p.MaxWarns++;
                    if (data[2] is ConstData.Minus)
                        p.MaxWarns--;

                }, group.GroupId, ct);

                await Client.EditMessageReplyMarkupAsync(callback.Message!.Chat.Id, callback.Message.MessageId,
                    updatedKeyboard, ct);

                break;

            case ConstData.Ban:
                updatedGroup = await GroupController.UpdateGroupAsync(p =>
                {
                    p.BanOnMaxWarn = !p.BanOnMaxWarn;
                }, group.GroupId, ct);
                if (updatedGroup is null)
                    return;
                group = updatedGroup;

                InlineButtons.ChangeButtonValue("Ban User", callback.Message.ReplyMarkup, button =>
                 {
                     button.Text = "Ban User " + (group.BanOnMaxWarn ? ConstData.TrueEmoji : ConstData.FalseEmoji);
                 });

                await Client.EditMessageReplyMarkupAsync(callback.Message!.Chat.Id, callback.Message.MessageId,
                    callback.Message.ReplyMarkup, ct);
                break;

            case ConstData.Mute:
                updatedGroup = await GroupController.UpdateGroupAsync(p =>
                {
                    p.MuteOnMaxWarn = !p.MuteOnMaxWarn;
                }, group.GroupId, ct);
                if (updatedGroup is null)
                    return;
                group = updatedGroup;

                InlineButtons.ChangeButtonValue("Mute User", callback.Message.ReplyMarkup, button =>
                {
                    button.Text = "Mute User " + (group.MuteOnMaxWarn ? ConstData.TrueEmoji : ConstData.FalseEmoji);
                });

                await Client.EditMessageReplyMarkupAsync(callback.Message!.Chat.Id, callback.Message.MessageId,
                    callback.Message.ReplyMarkup, ct);
                break;

            case ConstData.Back:
                await Client.EditMessageTextAsync(callback.Message!.Chat.Id, callback.Message.MessageId,
                    ConstData.MessageOfMainMenu, replyMarkup: InlineButtons.Admin.SettingMenu, cancellationToken: ct);
                break;

        }
    }

    private async Task GeneralSettingsAsync(CallbackQuery callback, IReadOnlyList<string> data, Group group, CancellationToken ct = default)
    {
        if (callback.Message is null)
            return;
        Group? updatedGroup;
        InlineKeyboardMarkup keyboard;
        switch (data[2])
        {
            case nameof(InlineButtons.Admin.General.AntiLink):
                keyboard = InlineButtons.Admin.General.GetAntiLinkMenu(group);
                await Client.EditMessageTextAsync(callback.Message.Chat.Id, callback.Message.MessageId, ConstData.MessageOfAntiLinkMenu, replyMarkup: keyboard, cancellationToken: ct);
                break;

            case ConstData.MessageLimitPerDay:
                {
                    updatedGroup = await GroupController.UpdateGroupAsync(p =>
                    {
                        p.EnableMessageLimitPerUser = !p.EnableMessageLimitPerUser;
                    }, group.GroupId, ct);
                    if (updatedGroup is null)
                        return;

                    keyboard = InlineButtons.Admin.General.GetMenu(updatedGroup);
                    await Client.EditMessageReplyMarkupAsync(callback.Message.Chat.Id, callback.Message.MessageId, keyboard, ct);

                    break;
                }

            case ConstData.AntiJoin:
            case ConstData.AntiBot:
            case ConstData.AntiForward:
                {
                    updatedGroup = await GroupController.UpdateGroupAsync(p =>
                    {
                        switch (data[2])
                        {
                            case ConstData.AntiBot:
                                {
                                    p.AntiBot = !p.AntiBot;
                                    break;
                                }
                            case ConstData.AntiJoin:
                                {
                                    p.AntiJoin = !p.AntiJoin;
                                    break;
                                }
                            case ConstData.AntiForward:
                                {
                                    p.AntiForward = !p.AntiForward;
                                    break;
                                }
                        }

                    }, group.GroupId, ct);
                    if (updatedGroup is null)
                        return;
                    keyboard = InlineButtons.Admin.General.GetMenu(updatedGroup);
                    await Client.EditMessageReplyMarkupAsync(callback.Message.Chat.Id, callback.Message.MessageId, keyboard, ct);

                    break;
                }

            case ConstData.Back:
                {
                    await Client.EditMessageReplyMarkupAsync(callback.Message.Chat.Id, callback.Message.MessageId,
                        InlineButtons.Admin.SettingMenu, ct);
                    break;
                }
        }
    }
}