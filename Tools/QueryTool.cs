using System.ComponentModel;
using ModelContextProtocol.Server;
using Microsoft.Data.SqlClient;
using System.Text.Json;

[McpServerToolType]
public static class QueryTool
{
    [McpServerTool, Description("Execute a custom SQL query and return the results as JSON. Optionally accepts a connection string.")]
    public static string ExecuteQuery(string sqlQuery, string? connectionString = null)
    {
        if (string.IsNullOrWhiteSpace(sqlQuery))
            return "Error: SQL query must not be empty.";

        var connStr = string.IsNullOrWhiteSpace(connectionString) 
            ? McpSQLServerApp.Helpers.ConfigHelper.GetDefaultConnectionString() 
            : connectionString;
        try
        {
            using var connection = new SqlConnection(connStr);
            connection.Open();
            using var command = new SqlCommand(sqlQuery, connection);
            
            // Для SELECT-запитів
            if (sqlQuery.TrimStart().StartsWith("SELECT", StringComparison.OrdinalIgnoreCase))
            {
                var results = new List<Dictionary<string, object?>>();
                using var reader = command.ExecuteReader();
                
                while (reader.Read())
                {
                    var row = new Dictionary<string, object?>();
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        string columnName = reader.GetName(i);
                        object? value = reader.IsDBNull(i) ? null : reader.GetValue(i);
                        row[columnName] = value;
                    }
                    results.Add(row);
                }
                
                return JsonSerializer.Serialize(results, new JsonSerializerOptions 
                { 
                    WriteIndented = true
                });
            }
            // Для запитів, які змінюють дані (INSERT, UPDATE, DELETE)
            else
            {
                int rowsAffected = command.ExecuteNonQuery();
                return $"Query executed successfully. Rows affected: {rowsAffected}";
            }
        }
        catch (Exception ex)
        {
            return $"Error executing query: {ex.Message}";
        }
    }
}