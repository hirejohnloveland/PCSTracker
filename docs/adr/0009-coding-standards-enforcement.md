# ADR 0009: C# Coding Standards Enforcement

## Context

A standard that exists only as a document depends on whoever writes the code following it, and agents writing code in this repository have not reliably done so. Tools can check mechanical rules on every build. They can't fully check every convention. `dotnet format` can apply fixes automatically, but at info severity it applies every suggestion-level fix, and some of those rewrite conditions.

## Decision

- [`.editorconfig`](../../.editorconfig) is the single definition of C# style. It starts from Microsoft's .NET template, with this repository's rules on top. A rule we want enforced is promoted to warning there.
- [`Directory.Build.props`](../../Directory.Build.props) runs the style checks in the build and treats warnings as errors, so a violation fails `dotnet build`. NuGet vulnerability warnings stay warnings.
- `dotnet format` runs at its default severity, never `--severity info`.
- The [C# standards document](../coding-standards/csharp/README.md) holds the conventions the tools can't fully check. A convention is added only once it is agreed.
- [`AGENTS.md`](../../AGENTS.md) tells agents to review their C# changes against that document before finishing. When the work is a plan to-do, the [`standards-reviewer`](../../.agents/agents/standards-reviewer.md) agent reviews the files the to-do changed before the to-do is marked complete.
- Suppressing a diagnostic or changing `.editorconfig` or `Directory.Build.props` needs a discussion first. The reviewer always reports either.

## Consequences

- Mechanical style violations are caught by the build instead of in review.
- A rule left at suggestion severity is not enforced. The build fails only at warning severity or higher.
- The repository has no CI pipeline, Cursor hook or pre-commit check. The build runs only when a person or an agent runs it, and the to-do review and the discussion rule rest on `AGENTS.md` and the reviewer, not on a mechanical gate.
- This covers C# only. Ember and architecture tests are not covered.
