using Rocket.API;
using Rocket.Unturned.Chat;
using SDG.Unturned;
using UnityEngine;
using Logger = Rocket.Core.Logging.Logger;

namespace SeniorS.BarricadeRestrictions.Helpers;

public static class MessageHelper
{
    private static readonly BarricadeRestrictions Instance = BarricadeRestrictions.Instance;

    private static readonly Color DefaultMessageColor = HexToColor(Instance.Configuration.Instance.hexDefaultMessagesColor);

    private static readonly Color ErrorMessageColor = HexToColor(Instance.Configuration.Instance.hexErrorMessagesColor);

    private static readonly string Prefix = FormatMessage("prefix") + " ";

    public static void Say(SteamPlayer steamPlayer, string translationKey, bool error, params object[] values)
    {
        string message = Prefix + FormatMessage(translationKey, values);

        ChatManager.serverSendMessage(message, error ? ErrorMessageColor : DefaultMessageColor, null, steamPlayer,
            EChatMode.SAY, "", true);
    }

    public static void Say(IRocketPlayer caller, string translationKey, bool error, params object[] values)
    {
        string message = Prefix + FormatMessage(translationKey, values);

        UnturnedChat.Say(caller, $"{message}", error ? ErrorMessageColor : DefaultMessageColor, true);
    }

    public static void Broadcast(string translationKey, params object[] values)
    {
        string message = FormatMessage(translationKey, values);

        UnturnedChat.Say($"{message}", DefaultMessageColor, true);
    }

    public static void Hint(Player player, string translationKey, bool error, params object[] values)
    {
        string message = FormatMessage(translationKey, values);

        string hexColor = error ? Instance.Configuration.Instance.hexErrorMessagesColor : Instance.Configuration.Instance.hexDefaultMessagesColor;

        string finalMessage = $"<color={hexColor}>{message}</color>";

        player.ServerShowHint(finalMessage, 8f);
    }

    private static Color HexToColor(string hex)
    {
        if (ColorUtility.TryParseHtmlString(hex, out Color color)) return color;

        Logger.LogError($"Could not convert {hex} to a Color.");
        return Color.white;
    }

    // ReSharper disable once MemberCanBePrivate.Global
    public static string FormatMessage(string translationKey, params object[] values)
    {
        string baseMessage = Instance.Translate(translationKey, values);
        baseMessage = baseMessage.Replace("-=", "<").Replace("=-", ">");

        return baseMessage;
    }
}