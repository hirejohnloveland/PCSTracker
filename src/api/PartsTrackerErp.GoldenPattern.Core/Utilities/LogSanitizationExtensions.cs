namespace PartsTrackerErp.GoldenPattern.Core.Utilities;

public static class LogSanitizationExtensions
{
    /// <summary>
    /// Replaces line breaks and tabs with spaces so caller-supplied text cannot forge extra log lines.
    /// </summary>
    public static string SanitizeForLogging(this string? input)
    {
        return string.IsNullOrEmpty(input)
            ? string.Empty
            : input.ReplaceLineEndings(" ").Replace('\t', ' ');
    }
}
