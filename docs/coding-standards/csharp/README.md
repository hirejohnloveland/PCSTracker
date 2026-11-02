# C# Coding Standards

**Applies to:** the .NET solution in `src/api` and `tests/api`

## How the standards are enforced

[`.editorconfig`](../../../.editorconfig) defines the C# style. It starts from Microsoft's .NET template (`dotnet new editorconfig`), with this repository's rules on top. The rules the build enforces are listed together in its *Enforced rules* section. Editors read the same file.

[`Directory.Build.props`](../../../Directory.Build.props) runs the style checks during the build and treats warnings as errors, so a violation fails `dotnet build`. NuGet vulnerability warnings stay warnings.

To fix violations, run:

```powershell
dotnet format src/api/PartsTrackerErp.GoldenPattern.slnx
```

Run it at its default severity. Don't use `--severity info`: that applies every suggestion-level fix, and some of those rewrite conditions. A rule we want enforced is promoted to warning in `.editorconfig` instead. Some violations have no automatic fix; fix those by hand.

## Suppressions and rule changes

Don't suppress a diagnostic (`#pragma warning disable`, `[SuppressMessage]`, `<NoWarn>`) or change `.editorconfig` or `Directory.Build.props` without discussing it first. Fix the code instead.

## Conventions the tools can't fully check

Add a convention here only after it has been agreed.

## Related

- [Project structure](../../project-structure.md): where code belongs.
- [Architecture decision records](../../adr/README.md), including [ADR 0008](../../adr/0008-cancellation-tokens.md) on cancellation tokens and [ADR 0010](../../adr/0010-service-response.md), [ADR 0011](../../adr/0011-problem-details.md), and [ADR 0012](../../adr/0012-built-in-exception-handling.md) on service responses and error handling.
