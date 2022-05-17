namespace GroupManager.Common.Extensions;

public static class GeneralHelpers
{
    internal static bool IsMoreThan(this TimeSpan first, TimeSpan second)
    {
        var isMinuteBigger = first.Minutes > second.Minutes;
        var isHourBigger = first.Hours >= second.Hours;
        var isSecondBigger = second.Seconds > first.Seconds;

        return isMinuteBigger && isHourBigger && isSecondBigger;
    }

}