# AGENTS.md

Entry point for AI agents working in this repository.

Before adding or moving code, read [docs/project-structure.md](docs/project-structure.md).

Architecture decisions are recorded in [docs/adr/README.md](docs/adr/README.md). Read the ones that cover the area you are changing.

## C#

`dotnet build` treats warnings as errors, including style violations. The rules agents most often get wrong:

- Methods, constructors and local functions use block bodies. Use `=>` only for read-only properties, lambdas and switch-expression arms.
- Put braces on every statement block.
- Name private instance fields `_camelCase` and private static fields `s_camelCase`.
- Use explicit types. Use `var` only with `new`, a cast, or a static method that returns the type it belongs to, like `Vendor.Create(...)`.
- Use primary constructors and collection expressions where they apply.
- Pass the `CancellationToken` on to every call that accepts one.
- Remove unused `using` directives.

Fix style violations with `dotnet format src/api/PartsTrackerErp.GoldenPattern.slnx`. Never run it with `--severity info`.

Never suppress a diagnostic or change `.editorconfig` or `Directory.Build.props` without discussing it with the user first.

Before finishing C# work, review your changes against [docs/coding-standards/csharp/README.md](docs/coding-standards/csharp/README.md). When the work is a plan to-do, have the `standards-reviewer` subagent do that review before you mark the to-do complete, and tell it which files the to-do changed. Fix the findings you agree with, say why you disagree with the rest, and bring anything still disputed to the user.
