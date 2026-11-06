namespace PartsTrackerErp.GoldenPattern.Core.Domain.Entities.Settings;

/// <summary>
/// The shared rules for a settings document that holds a list of names.
/// </summary>
internal static class SettingsList
{
    /// <summary>
    /// Returns <paramref name="items"/> with each name trimmed. Throws an <see cref="ArgumentException"/> when the list is
    /// empty, a name is blank, or two names differ only by case. <paramref name="itemName"/> is the singular word used
    /// in the error messages, such as "role".
    /// </summary>
    internal static string[] Normalize(IEnumerable<string> items, string itemName, string parameterName)
    {
        string[] trimmed = [.. items.Select(item => item?.Trim() ?? string.Empty)];

        if (trimmed.Length == 0 || trimmed.Any(string.IsNullOrEmpty))
        {
            throw new ArgumentException(
                $"At least one {itemName} is required, and no {itemName} can be blank.",
                parameterName);
        }

        if (trimmed.Distinct(StringComparer.OrdinalIgnoreCase).Count() != trimmed.Length)
        {
            throw new ArgumentException($"Each {itemName} must be unique.", parameterName);
        }

        return trimmed;
    }
}
