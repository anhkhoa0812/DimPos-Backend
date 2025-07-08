namespace DimPos.Order.Infrastructure.Utils;

public class TimeUtil
{
    public static DateTime GetCurrentSEATime()
    {
        TimeZoneInfo tz = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
        DateTime localTime = DateTime.Now;
        DateTime utcTime = TimeZoneInfo.ConvertTime(localTime, TimeZoneInfo.Local, tz);
        return utcTime;
    }
}