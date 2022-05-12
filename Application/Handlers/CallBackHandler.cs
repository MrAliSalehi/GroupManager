﻿using GroupManager.Application.Contracts;
using GroupManager.DataLayer.Controller;
using GroupManager.DataLayer.Models;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace GroupManager.Application.Handlers;

public class CallBackHandler : HandlerBase
{
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
            case "Admin":
                await AdminCallbacksAsync(callback, dataArr, ct);
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
                        case ConstData.Close:
                            await Client.EditMessageTextAsync(callback.Message.Chat.Id, callback.Message.MessageId, "<i>Panel Closed</i>", ParseMode.Html, cancellationToken: ct);
                            break;
                    }
                    break;
                }
            case nameof(InlineButtons.Admin.Warn):
                await AdminWarnMenuAsync(callback, data, group, ct);
                break;
            case nameof(InlineButtons.Admin.Curse):
                await AdminCurseMenuAsync(callback, data, group, ct);
                break;
            case nameof(InlineButtons.Admin.ConfirmChat):
                await GroupController.AddGroupAsync(callback.Message.Chat.Id, ct);
                await Client.EditMessageTextAsync(callback.Message.Chat.Id, callback.Message.MessageId, "<b>Bot is Now Active</b>", ParseMode.Html, cancellationToken: ct);

                break;
        }

    }

    private async Task AdminCurseMenuAsync(CallbackQuery callback, IReadOnlyList<string> data, Group group, CancellationToken ct = default)
    {
        if (callback.Message?.ReplyMarkup is null)
            return;
        switch (data[2])
        {
            case ConstData.Ban:
            case ConstData.Mute:
            case ConstData.Warn:
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

                group = updatedGroup;
                var prop = group.GetType().GetProperties().SingleOrDefault(p => p.Name == updatedProperty);
                if (prop is null)
                    return;
                var value = (bool)prop.GetValue(group)!;

                InlineButtons.ChangeButtonValue(buttonName, callback.Message.ReplyMarkup, button =>
                {
                    button.Text = $"{buttonName} " + (value ? ConstData.TrueEmoji : ConstData.FalseEmoji);
                });

                await Client.EditMessageReplyMarkupAsync(callback.Message.Chat.Id, callback.Message.MessageId,
                    callback.Message.ReplyMarkup, ct);
                break;

            case nameof(InlineButtons.Admin.Curse.MuteTimeModify):
                await Client.EditMessageReplyMarkupAsync(callback.Message.Chat.Id, callback.Message.MessageId, InlineButtons.Admin.Curse.MuteTimeModify, ct);
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

}