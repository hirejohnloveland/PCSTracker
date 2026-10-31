using Microsoft.Extensions.Logging;

namespace PartsTrackerErp.GoldenPattern.Tests.Api.ErrorHandling;

internal sealed record LogEntry(LogLevel Level, Exception? Exception, string Message);

/// <summary>Records everything logged through any logger the provider creates.</summary>
internal sealed class CapturingLoggerProvider : ILoggerProvider
{
    public List<LogEntry> Entries { get; } = [];

    public ILogger CreateLogger(string categoryName) => new CapturingLogger(Entries);

    public void Dispose()
    {
    }

    private sealed class CapturingLogger(List<LogEntry> entries) : ILogger
    {
        public IDisposable? BeginScope<TState>(TState state)
            where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter) =>
            entries.Add(new LogEntry(logLevel, exception, formatter(state, exception)));
    }
}
