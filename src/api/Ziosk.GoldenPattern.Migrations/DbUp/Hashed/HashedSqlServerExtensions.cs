using System;
using System.Reflection;
using System.Text;
using DbUp.Builder;
using DbUp.Engine;
using DbUp.SqlServer;

namespace Ziosk.DbUp.Hashed;

public static class HashedSqlServerExtensions
{
    public const string DefaultVersionTableName = "SchemaVersions";
    public const string DefaultSchema = "dbo";

    public static UpgradeEngineBuilder HashedSqlDatabase(
        this SupportedDatabases supported,
        SqlConnectionManager connectionManager,
        string schema = DefaultSchema,
        string migrationTableName = DefaultVersionTableName)
    {
        var builder = new UpgradeEngineBuilder();
        builder.Configure(c => c.ConnectionManager = connectionManager);
        builder.Configure(c => c.ScriptExecutor = new SqlScriptExecutor(() => c.ConnectionManager, () => c.Log, schema,
            () => c.VariablesEnabled, c.ScriptPreprocessors, () => c.Journal));
        builder.Configure(c =>
            c.Journal = new HashedSqlTableJournal(() => c.ConnectionManager, () => c.Log, new SqlServerObjectParser(),
                schema, migrationTableName));
        return builder;
    }

    public static UpgradeEngineBuilder WithHashedScriptsEmbeddedInAssembly(
        this UpgradeEngineBuilder builder,
        Assembly assembly,
        Func<string, bool> filter,
        IJournal journal)
    {
        return WithScripts(builder, new HashedEmbeddedScriptsProvider(assembly, filter, Encoding.Default, journal));
    }

    public static UpgradeEngineBuilder WithScripts(
        this UpgradeEngineBuilder builder,
        IScriptProvider scriptProvider)
    {
        builder.Configure(c => c.ScriptProviders.Add(scriptProvider));
        return builder;
    }
}