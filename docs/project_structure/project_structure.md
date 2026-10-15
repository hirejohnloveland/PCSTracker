# Project Structure

Solution: `src/api/Ziosk.GoldenPattern.slnx`

| Project | Purpose |
|---------|---------|
| `Ziosk.GoldenPattern.Api` | ASP.NET Core Web API |
| `Ziosk.GoldenPattern.Core` | Business logic class library, shared by all hosts |
| `Ziosk.GoldenPattern.Migrations` | Console app that applies DbUp database migrations |
| `Ziosk.GoldenPattern.Tests` | xUnit test project (`tests/api/Ziosk.GoldenPattern.Tests`) |
