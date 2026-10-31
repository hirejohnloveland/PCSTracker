using PartsTrackerErp.GoldenPattern.Core.Utilities;

namespace PartsTrackerErp.GoldenPattern.Tests.Core.Utilities;

[Trait(Constants.TestCategory, Constants.UnitTestCategory)]
public class Given_LogSanitizationExtensions
{
    [Theory]
    [InlineData("plain text", "plain text")]
    [InlineData("a\r\nb", "a b")]
    [InlineData("a\nb", "a b")]
    [InlineData("a\rb", "a b")]
    [InlineData("a\tb", "a b")]
    [InlineData("a\r\n\r\nb", "a  b")]
    [InlineData("", "")]
    [InlineData(null, "")]
    public void SanitizeForLogging_ReplacesLineBreaksAndTabsWithSpaces(string? input, string expected)
    {
        Assert.Equal(expected, input.SanitizeForLogging());
    }
}
