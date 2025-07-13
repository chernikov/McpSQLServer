using System.ComponentModel;
using ModelContextProtocol.Server;
using Microsoft.Data.SqlClient;
using McpSQLServerApp.Helpers;
using System.Data;

[McpServerToolType]
public static class DatabaseTool
{
    [McpServerTool, Description("Lists all user tables in the specified database. Optionally accepts a connection string.")]
    public static List<string> ListTables(string? connectionString = null)
    {
        var connStr = string.IsNullOrWhiteSpace(connectionString)
                   ? ConfigHelper.GetDefaultConnectionString()
                   : connectionString;
        var tables = new List<string>();
        try
        {
            using var connection = new SqlConnection(connStr);
            connection.Open();
            var schema = connection.GetSchema("Tables");
            foreach (DataRow row in schema.Rows)
            {
                if (row[3]?.ToString() == "BASE TABLE")
                    tables.Add(row[2]?.ToString() ?? "");
            }
            return tables;
        }
        catch (Exception ex)
        {
            return new List<string> { $"Error: {ex.Message}" };
        }
    }

    [McpServerTool, Description("Lists all indexes for a given table. Accepts table name and optional connection string.")]
    public static List<string> ListIndexes(string tableName, string? connectionString = null)
    {
        var connStr = string.IsNullOrWhiteSpace(connectionString)
            ? ConfigHelper.GetDefaultConnectionString()
            : connectionString;

        var indexes = new List<string>();
        try
        {
            using var connection = new SqlConnection(connStr);
            connection.Open();
            string sql = @"SELECT i.name AS IndexName, i.type_desc, c.name AS ColumnName
                           FROM sys.indexes i
                           INNER JOIN sys.index_columns ic ON i.object_id = ic.object_id AND i.index_id = ic.index_id
                           INNER JOIN sys.columns c ON ic.object_id = c.object_id AND ic.column_id = c.column_id
                           INNER JOIN sys.tables t ON i.object_id = t.object_id
                           WHERE t.name = @tableName";
            using var cmd = new SqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@tableName", tableName);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                indexes.Add($"Index: {reader["IndexName"]}, Type: {reader["type_desc"]}, Column: {reader["ColumnName"]}");
            }
            return indexes;
        }
        catch (Exception ex)
        {
            return new List<string> { $"Error: {ex.Message}" };
        }
    }

    [McpServerTool, Description("Gets statistics info for a given table. Accepts table name and optional connection string.")]
    public static List<string> GetTableStatistics(string tableName, string? connectionString = null)
    {
        var connStr = string.IsNullOrWhiteSpace(connectionString)
            ? ConfigHelper.GetDefaultConnectionString()
            : connectionString;

        var stats = new List<string>();
        try
        {
            using var connection = new SqlConnection(connStr);
            connection.Open();
            string sql = @"SELECT s.name AS StatName, s.auto_created, s.user_created, sp.last_updated
                           FROM sys.stats s
                           INNER JOIN sys.tables t ON s.object_id = t.object_id
                           OUTER APPLY sys.dm_db_stats_properties(s.object_id, s.stats_id) sp
                           WHERE t.name = @tableName";
            using var cmd = new SqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@tableName", tableName);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                stats.Add($"Stat: {reader["StatName"]}, Auto: {reader["auto_created"]}, User: {reader["user_created"]}, LastUpdated: {reader["last_updated"]}");
            }
            return stats;
        }
        catch (Exception ex)
        {
            return new List<string> { $"Error: {ex.Message}" };
        }
    }

    [McpServerTool, Description("Executes a SELECT query and returns the results as a list of dictionaries. Accepts query and optional connection string.")]
    public static List<Dictionary<string, object?>> QueryData(string selectQuery, string? connectionString = null)
    {
        var connStr = string.IsNullOrWhiteSpace(connectionString)
              ? ConfigHelper.GetDefaultConnectionString()
              : connectionString;

        var results = new List<Dictionary<string, object?>>();
        try
        {
            using var connection = new SqlConnection(connStr);
            connection.Open();
            using var cmd = new SqlCommand(selectQuery, connection);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var row = new Dictionary<string, object?>();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    row[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i) as object;
                }
                results.Add(row);
            }
            return results;
        }
        catch (Exception ex)
        {
            return new List<Dictionary<string, object?>> { new Dictionary<string, object?> { { "Error", ex.Message } } };
        }
    }

    [McpServerTool, Description("Lists all databases on the server. Optionally accepts a connection string.")]
    public static List<string> ListDatabases(string? connectionString = null)
    {
        var connStr = string.IsNullOrWhiteSpace(connectionString)
                   ? ConfigHelper.GetDefaultConnectionString()
                   : connectionString;
        var databases = new List<string>();
        try
        {
            using var connection = new SqlConnection(connStr);
            connection.Open();
            var schema = connection.GetSchema("Databases");
            foreach (DataRow row in schema.Rows)
            {
                databases.Add(row["database_name"].ToString() ?? "");
            }
            return databases;
        }
        catch (Exception ex)
        {
            return new List<string> { $"Error: {ex.Message}" };
        }
    }

    [McpServerTool, Description("Creates a new SQL Server database with the specified name. Optionally accepts a connection string (to master DB).")]
    public static string CreateDatabase(string dbName, string? connectionString = null)
    {
        if (string.IsNullOrWhiteSpace(dbName))
            return "Error: Database name must not be empty.";

        var connStr = string.IsNullOrWhiteSpace(connectionString)
           ? ConfigHelper.GetDefaultConnectionString()
           : connectionString;

        try
        {
            using var connection = new SqlConnection(connStr);
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = $"CREATE DATABASE [" + dbName.Replace("]", "]]") + "]";
            command.ExecuteNonQuery();
            return $"Database '{dbName}' created successfully.";
        }
        catch (Exception ex)
        {
            return $"Error creating database: {ex.Message}";
        }
    }
}
