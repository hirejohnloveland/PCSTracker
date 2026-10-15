using System;
using System.Collections.Generic;
using System.Data;
using DbUp.Engine;
using DbUp.Engine.Output;
using DbUp.Engine.Transactions;

namespace Ziosk.DbUp.Hashed;

public class HashedSqlTableJournal : IJournal
{
    private readonly Func<IConnectionManager> _connectionManager;
    private readonly Func<IUpgradeLog> _log;
    private readonly string _schema;
    private readonly ISqlObjectParser _sqlObjectParser;
    private readonly string _table;

    private bool _versionTableExisted;

    public HashedSqlTableJournal(Func<IConnectionManager> connectionManager,
        Func<IUpgradeLog> logger,
        ISqlObjectParser sqlObjectParser,
        string schema = "dbo",
        string table = HashedSqlServerExtensions.DefaultVersionTableName)
    {
        _schema = schema;
        _table = table;

        _connectionManager = connectionManager;
        _sqlObjectParser = sqlObjectParser;
        _log = logger;
    }

    public string[] GetExecutedScripts()
    {
        // note: the HashedEmbeddedScriptsProvider implementation will deal with "already/should" run determination so we don't want any "executed scripts" in the pipeline
        return Array.Empty<string>();
    }

    public void StoreExecutedScript(SqlScript script, Func<IDbCommand> dbCommandFactory)
    {
        _connectionManager().ExecuteCommandsWithManagedConnection(EnsureTableExistsAndIsLatestVersion);
        if (!_versionTableExisted)
        {
            _log().WriteInformation($"Creating the {CreateTableName(_schema, _table)} table");

            _connectionManager().ExecuteCommandsWithManagedConnection(commandFactory =>
            {
                using var command = commandFactory();
                command.CommandText = CreateTableSql(_schema, _table);

                command.CommandType = CommandType.Text;
                command.ExecuteNonQuery();

                _log()
                    .WriteInformation($"The {CreateTableName(_schema, _table)} table has been created");
            });
        }

        EnsureHashColumnExists();

        _connectionManager().ExecuteCommandsWithManagedConnection(commandFactory =>
        {
            using var command = commandFactory();
            command.CommandText =
                $@"
        MERGE {CreateTableName(_schema, _table)} AS [Target] 
        USING(SELECT @scriptName as scriptName, @applied as applied, @scriptHash as scriptHash) AS [Source] 
        ON [Target].scriptName = [Source].scriptName 
        WHEN MATCHED THEN 
            UPDATE SET[Target].applied = [Source].applied, [Target].scriptHash = [Source].scriptHash 
        WHEN NOT MATCHED THEN 
            INSERT(ScriptName, Applied, ScriptHash) VALUES([Source].scriptName, [Source].applied, [Source].scriptHash); ";

            // todo: strip off the |HASH" from Name:
            var scriptNameParam = command.CreateParameter();
            scriptNameParam.ParameterName = "scriptName";
            scriptNameParam.Value = script.Name;
            command.Parameters.Add(scriptNameParam);

            var appliedParam = command.CreateParameter();
            appliedParam.ParameterName = "applied";
            appliedParam.Value = DateTime.Now;
            command.Parameters.Add(appliedParam);

            var hashParam = command.CreateParameter();
            hashParam.ParameterName = "scriptHash";
            hashParam.Value = Md5Utils.Md5EncodeString(script.Contents);
            command.Parameters.Add(hashParam);

            command.CommandType = CommandType.Text;
            command.ExecuteNonQuery();
        });
    }

    public void EnsureTableExistsAndIsLatestVersion(Func<IDbCommand> dbCommandFactory)
    {
        var command = dbCommandFactory.Invoke();
        command.CommandText = string.IsNullOrEmpty(_schema)
            ? $"select 1 from INFORMATION_SCHEMA.TABLES where TABLE_NAME = '{_table}'"
            : $"select 1 from INFORMATION_SCHEMA.TABLES where TABLE_NAME = '{_table}' and TABLE_SCHEMA = '{_schema}'";
        command.CommandType = CommandType.Text;
        var result = command.ExecuteScalar() as int?;
        _versionTableExisted = result == 1;
    }

    public Dictionary<string, string> GetExecutedScriptDictionary()
    {
        _log().WriteInformation("Fetching list of already executed scripts with their known hash.");
        _connectionManager().ExecuteCommandsWithManagedConnection(EnsureTableExistsAndIsLatestVersion);
        if (!_versionTableExisted)
        {
            _log()
                .WriteInformation(
                    $"The {CreateTableName(_schema, _table)} table could not be found. The database is assumed to be at version 0.");
            return new Dictionary<string, string>();
        }

        EnsureHashColumnExists();

        var scripts = new Dictionary<string, string>();
        _connectionManager().ExecuteCommandsWithManagedConnection(dbCommandFactory =>
        {
            using var command = dbCommandFactory();
            command.CommandText = GetExecutedScriptsSql(_schema, _table);
            command.CommandType = CommandType.Text;

            using var reader = command.ExecuteReader();
            while (reader.Read())
                scripts.Add((string)reader[0], reader[1] == DBNull.Value ? string.Empty : (string)reader[1]);
        });

        return scripts;
    }

    protected virtual string GetExecutedScriptsSql(string schema, string table)
    {
        return $"select [ScriptName], [ScriptHash] from {CreateTableName(schema, table)} order by [ScriptName]";
    }

    protected virtual string CreateTableSql(string schema, string table)
    {
        var tableName = CreateTableName(schema, table);
        var primaryKeyConstraintName = CreatePrimaryKeyName(table);

        return $@"create table {tableName} (
	[Id] int identity(1,1) not null constraint {primaryKeyConstraintName} primary key,
	[ScriptName] nvarchar(255) not null,
	[Applied] datetime not null,
    [ScriptHash] nvarchar(255) null
)";
    }

    protected virtual string CreateTableName(string schema, string table)
    {
        return string.IsNullOrEmpty(schema)
            ? _sqlObjectParser.QuoteIdentifier(table)
            : _sqlObjectParser.QuoteIdentifier(schema) + "." + _sqlObjectParser.QuoteIdentifier(table);
    }

    protected virtual string CreatePrimaryKeyName(string table)
    {
        return _sqlObjectParser.QuoteIdentifier("PK_" + table + "_Id");
    }

    protected void EnsureHashColumnExists()
    {
        _connectionManager().ExecuteCommandsWithManagedConnection(dbCommandFactory =>
        {
            using (var command = dbCommandFactory())
            {
                command.CommandText =
                    $"if not exists (select column_name from INFORMATION_SCHEMA.columns where table_name = '{_table}' and column_name = 'ScriptHash') alter table {_table} add ScriptHash nvarchar(255)";
                command.CommandType = CommandType.Text;
                command.ExecuteNonQuery();
            }
        });
    }
}