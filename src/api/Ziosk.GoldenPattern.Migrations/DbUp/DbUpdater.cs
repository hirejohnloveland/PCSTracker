using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;
using DbUp;
using DbUp.Builder;
using DbUp.Engine.Output;
using DbUp.Helpers;
using DbUp.SqlServer;
using Ziosk.DbUp.Hashed;

namespace Ziosk.DbUp;

public class DbUpdater
{
    private readonly Env _env;
    private readonly Assembly _migrationAssembly;
    private readonly string _namespacePrefix;
    private readonly bool _shouldSeedData;

    public DbUpdater(Assembly migrationAssembly,
        string folderName,
        string? databaseName,
        string? connectionString,
        Dictionary<string, string?> scriptVariables,
        bool shouldSeedData = false,
        Env env = Env.Undefined)
    {
        _namespacePrefix = migrationAssembly.GetName().Name + ".";
        _migrationAssembly = migrationAssembly;
        _shouldSeedData = shouldSeedData;
        _env = env;

        ConnectionString = connectionString;
        UseTransactions = false;
        DatabaseName = databaseName;
        FolderName = folderName;
        ScriptVariables = scriptVariables;
    }

    public string FolderName { get; }
    public string? DatabaseName { get; }
    public string? ConnectionString { get; }

    // TODO: need to review how latest DbUp deals with transactions as setting UseTransactions causes an error of nested transactions
    public bool UseTransactions { get; set; }
    public IDictionary<string, string?> ScriptVariables { get; }

    public bool Run()
    {
        try
        {
            if (IsNew())
            {
                Console.WriteLine("No '{0}' database found. One will be created.", DatabaseName);
                CreateDatabase();
            }

            Console.WriteLine("Performing beforeMigrations for '{0}' database.", DatabaseName);
            BeforeMigration();

            Console.WriteLine("Performing migrations for '{0}' database.", DatabaseName);
            MigrateDatabase();

            Console.WriteLine("Executing hashed scripts for '{0}' database.", DatabaseName);
            HashedScripts();

            Console.WriteLine("Executing always run scripts for '{0}' database.", DatabaseName);
            AlwaysRun();

            if (_shouldSeedData)
            {
                Console.WriteLine("Executing seed scripts for '{0}' database.", DatabaseName);
                SeedDatabase();
            }
        }
        catch (Exception e)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(e.Message);
            Console.ResetColor();

            return false;
        }

        return true;
    }

    private void HashedScripts()
    {
        ExecuteHashedDatabaseActions(
            _namespacePrefix + FolderName + ".Hash",
            ConnectionString,
            builder => UseTransactions ? builder.WithTransactionPerScript() : builder);
    }

    private void AlwaysRun()
    {
        ExecuteDatabaseActions(
            _namespacePrefix + FolderName + ".AlwaysRun",
            ConnectionString,
            builder => builder.JournalTo(new NullJournal()));
    }

    private void BeforeMigration()
    {
        ExecuteDatabaseActions(
            _namespacePrefix + FolderName + ".BeforeMigration",
            ConnectionString,
            builder => UseTransactions ? builder.WithTransactionPerScript() : builder);
    }

    private void MigrateDatabase()
    {
        ExecuteDatabaseActions(
            _namespacePrefix + FolderName + ".Migration",
            ConnectionString,
            builder => UseTransactions ? builder.WithTransactionPerScript() : builder);
    }

    private void SeedDatabase()
    {
        ExecuteDatabaseActions(
            _namespacePrefix + FolderName + ".Seed",
            ConnectionString,
            builder => UseTransactions ? builder.WithTransactionPerScript() : builder);
    }

    private void CreateDatabase()
    {
        var masterDbSqlConnection = new SqlConnectionStringBuilder(ConnectionString)
        {
            InitialCatalog = "master"
        };

        ExecuteDatabaseActions(
            _namespacePrefix + FolderName + ".FirstRun",
            masterDbSqlConnection.ConnectionString,
            builder => builder.JournalTo(new NullJournal()));
    }

    private void ExecuteDatabaseActions(string scriptPrefix, string? connectionString,
        Func<UpgradeEngineBuilder, UpgradeEngineBuilder> customBuilderTransform)
    {
        var builder =
            DeployChanges.To
                .SqlDatabase(connectionString)
                .WithExecutionTimeout(TimeSpan.FromSeconds(2147483647))
                .WithScriptsEmbeddedInAssembly(_migrationAssembly,
                    name => name.StartsWith(scriptPrefix))
                .WithVariables(ScriptVariables)
                .LogToConsole();

        PerformUpgrade(customBuilderTransform, builder);
    }

    private void PerformUpgrade(Func<UpgradeEngineBuilder, UpgradeEngineBuilder> customBuilderTransform,
        UpgradeEngineBuilder builder)
    {
        var upgrader = customBuilderTransform(builder).Build();
        var createResult = upgrader.PerformUpgrade();

        if (!createResult.Successful)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(createResult.Error);
            throw createResult.Error;
        }

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Success!");
        Console.ResetColor();
    }

    private void ExecuteHashedDatabaseActions(
        string scriptPrefix,
        string? connectionString,
        Func<UpgradeEngineBuilder, UpgradeEngineBuilder> customBuilderTransform)
    {
        var sqlConnectionManager = new SqlConnectionManager(connectionString);
        var log = new ConsoleUpgradeLog();
        var journal =
            new HashedSqlTableJournal(() => sqlConnectionManager, () => log, new SqlServerObjectParser());

        var builder =
            DeployChanges.To
                .HashedSqlDatabase(sqlConnectionManager)
                .WithExecutionTimeout(TimeSpan.FromSeconds(2147483647))
                .WithHashedScriptsEmbeddedInAssembly(_migrationAssembly, name => name.StartsWith(scriptPrefix), journal)
                .WithVariables(ScriptVariables)
                .LogToConsole();

        PerformUpgrade(customBuilderTransform, builder);
    }

    private bool IsNew()
    {
        var masterDbSqlConnection = new SqlConnectionStringBuilder(ConnectionString)
        {
            InitialCatalog = "master"
        };
        using var connection = new SqlConnection(masterDbSqlConnection.ConnectionString);
        using var command = connection.CreateCommand();
        command.CommandText =
            $"SELECT name FROM sys.databases WHERE name = N'{DatabaseName}'";
        command.CommandType = CommandType.Text;

        connection.Open();
        var db = command.ExecuteScalar();
        connection.Close();

        return db == null;
    }
}