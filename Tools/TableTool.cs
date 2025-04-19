using System.ComponentModel;
using ModelContextProtocol.Server;
using McpSQLServerApp.Helpers;
using Microsoft.Data.SqlClient;


[McpServerToolType]
public static class TableTool
{
    private static string GetConnectionString(string? connectionString)
    {
        return string.IsNullOrWhiteSpace(connectionString)
            ? ConfigHelper.GetDefaultConnectionString()
            : connectionString;
    }

    [McpServerTool, Description("Creates a new table in the specified database. Accepts a CREATE TABLE SQL statement and an optional connection string.")]
    public static string CreateTable(string createTableSql, string? connectionString = null)
    {
        if (string.IsNullOrWhiteSpace(createTableSql) || !createTableSql.TrimStart().StartsWith("CREATE TABLE", StringComparison.OrdinalIgnoreCase))
            return "Error: Please provide a valid CREATE TABLE SQL statement.";

        var connStr = string.IsNullOrWhiteSpace(connectionString)
                   ? ConfigHelper.GetDefaultConnectionString()
                   : connectionString;

        try
        {
            using var connection = new SqlConnection(connStr);
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = createTableSql;
            command.ExecuteNonQuery();
            return "Table created successfully.";
        }
        catch (Exception ex)
        {
            return $"Error creating table: {ex.Message}";
        }
    }

    [McpServerTool, Description("Lists all user tables in the current database. Returns a list of table names.")]
    public static List<string> ListAllTables(string? connectionString = null)
    {
        var connStr = GetConnectionString(connectionString);
        var tables = new List<string>();
        try
        {
            using SqlConnection connection = new SqlConnection(connStr);
            connection.Open();
            var schema = connection.GetSchema("Tables");
            foreach (System.Data.DataRow row in schema.Rows)
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
}