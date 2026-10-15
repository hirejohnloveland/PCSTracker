using System.Reflection;
using Microsoft.Extensions.Configuration;
using Ziosk.DbUp;

string[] configurationVariables =
{
    "ConnectionString", "DbName", "DatabaseLocation", "LogLocation", "Env", "ChangeNumber"
};

IConfiguration config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .AddUserSecrets<Program>()
    .AddEnvironmentVariables()
    .Build();

var scriptVariables = configurationVariables.ToDictionary(s => s, s => config.GetRequiredSection("Migrations")[s]);
var env = EnvParser.Parse(scriptVariables["Env"]);
var shouldSeedData = env == Env.LOCAL;
var connectionString = scriptVariables["ConnectionString"];
var dbName = scriptVariables["DbName"];
var dbUpdater = new DbUpdater(Assembly.GetExecutingAssembly(), "Scripts", dbName, connectionString, scriptVariables,
    shouldSeedData, env);

var result = dbUpdater.Run() ? 0 : -1;

#if DEBUG
Console.WriteLine("Press any key to exit.");
Console.ReadKey();
#endif

return result;
