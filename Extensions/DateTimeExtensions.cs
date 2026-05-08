namespace AisVacanciesAndResumes.Extensions;

public static class DateTimeExtensions
{
    private static readonly Lazy<TimeZoneInfo> KyivTimeZone = new(FindKyivTimeZone);

    public static DateTime ToKyivTime(this DateTime value)
    {
        var utcValue = value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
        };

        return TimeZoneInfo.ConvertTimeFromUtc(utcValue, KyivTimeZone.Value);
    }

    public static string ToKyivString(this DateTime value, string format = "dd.MM.yyyy HH:mm")
    {
        return value.ToKyivTime().ToString(format);
    }

    public static string ToKyivString(this DateTime? value, string format = "dd.MM.yyyy HH:mm", string emptyText = "Не вказано")
    {
        return value.HasValue ? value.Value.ToKyivString(format) : emptyText;
    }

    private static TimeZoneInfo FindKyivTimeZone()
    {
        foreach (var timeZoneId in new[] { "Europe/Kyiv", "Europe/Kiev", "FLE Standard Time" })
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            }
            catch (TimeZoneNotFoundException)
            {
            }
            catch (InvalidTimeZoneException)
            {
            }
        }

        return TimeZoneInfo.Local;
    }
}
