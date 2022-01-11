using System.Globalization;

namespace PS7Api.Utilities;

public static class Extensions
{
    public static string Iso8601(this DateTime dateTime)
    {
        return dateTime.ToString("s", CultureInfo.InvariantCulture);
    }
    
    public static bool IsBetween(this DateTime dateTime, DateTime start, DateTime end)
    {
        return dateTime >= start && dateTime <= end;
    }
}