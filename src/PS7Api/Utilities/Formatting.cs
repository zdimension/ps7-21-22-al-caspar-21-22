namespace PS7Api.Utilities;

public static class Formatting
{
    public static string Iso8601(this DateTime dateTime)
    {
        return dateTime.ToString("s", System.Globalization.CultureInfo.InvariantCulture);
    }
}