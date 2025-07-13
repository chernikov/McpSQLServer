using System.ComponentModel;
using ModelContextProtocol.Server;
using Microsoft.Data.SqlClient;
using System.Text.Json;

[McpServerToolType]
public static class RowTool
{
    /// <summary>
    /// Updates a row in the specified table by key value.
    /// </summary>
    /// <param name="tableName">Name of the table to update</param>
    /// <param name="keyColumn">Name of the key column for the WHERE clause</param>
    /// <param name="keyValue">Value for the key column</param>
    /// <param name="columnValuesJson">JSON string representing column values to update, e.g. {"column1":"value1","column2":42}</param>
    /// <param name="connectionString">Optional connection string, uses default if not specified</param>
    /// <returns>Result message</returns>
    [McpServerTool, Description("Updates a row in the specified table. Accepts table name, key column, key value, column values as JSON, and an optional connection string.")]
    public static string UpdateRow(
        string tableName,
        string keyColumn,
        string keyValue,
        string columnValuesJson,
        string? connectionString = null)
    {
        if (string.IsNullOrWhiteSpace(tableName) || string.IsNullOrWhiteSpace(keyColumn) || string.IsNullOrWhiteSpace(columnValuesJson))
            return "Error: Table name, key column, and column values (as JSON) are required.";

        Dictionary<string, object?>? columnValues;
        try
        {
            columnValues = JsonSerializer.Deserialize<Dictionary<string, object?>>(columnValuesJson,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (columnValues == null || columnValues.Count == 0)
                return "Error: Column values JSON could not be parsed or is empty.";
        }
        catch (Exception ex)
        {
            return $"Error parsing JSON column values: {ex.Message}";
        }

        var connStr = string.IsNullOrWhiteSpace(connectionString)
            ? McpSQLServerApp.Helpers.ConfigHelper.GetDefaultConnectionString()
            : connectionString;
        try
        {
            using var connection = new SqlConnection(connStr);
            connection.Open();
            var setParts = new List<string>();
            foreach (var col in columnValues.Keys)
            {
                if (!string.Equals(col, keyColumn, StringComparison.OrdinalIgnoreCase))
                    setParts.Add($"[{col}] = @{col}");
            }
            string updateSql = $"UPDATE [{tableName}] SET {string.Join(", ", setParts)} WHERE [{keyColumn}] = @keyValue";
            using var updateCmd = new SqlCommand(updateSql, connection);
            updateCmd.Parameters.AddWithValue("@keyValue", keyValue);
            foreach (var kvp in columnValues)
            {
                if (!string.Equals(kvp.Key, keyColumn, StringComparison.OrdinalIgnoreCase))
                    updateCmd.Parameters.AddWithValue($"@{kvp.Key}", kvp.Value ?? DBNull.Value);
            }
            int rows = updateCmd.ExecuteNonQuery();
            return $"Updated {rows} row(s) in '{tableName}'.";
        }
        catch (Exception ex)
        {
            return $"Error updating row: {ex.Message}";
        }
    }
    /// <summary>
    /// Inserts a row into the specified table.
    /// </summary>
    /// <param name="tableName">Name of the table where to insert data</param>
    /// <param name="columnValuesJson">JSON string representing column values to insert, e.g. {"column1":"value1","column2":42}</param>
    /// <param name="connectionString">Optional connection string, uses default if not specified</param>
    /// <returns>Result message</returns>
    [McpServerTool, Description("Inserts a row into the specified table. Accepts table name, column values as JSON, and an optional connection string.")]
    public static string InsertRow(
        string tableName,
        string columnValuesJson,
        string? connectionString = null)
    {
        if (string.IsNullOrWhiteSpace(tableName) || string.IsNullOrWhiteSpace(columnValuesJson))
            return "Error: Table name and column values (as JSON) are required.";

        Dictionary<string, object?>? columnValues;
        try
        {
            columnValues = JsonSerializer.Deserialize<Dictionary<string, object?>>(columnValuesJson, 
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            
            if (columnValues == null || columnValues.Count == 0)
                return "Error: Column values JSON could not be parsed or is empty.";
        }
        catch (Exception ex)
        {
            return $"Error parsing JSON column values: {ex.Message}";
        }

        var connStr = string.IsNullOrWhiteSpace(connectionString)
            ? McpSQLServerApp.Helpers.ConfigHelper.GetDefaultConnectionString()
            : connectionString;
        try
        {
            using var connection = new SqlConnection(connStr);
            connection.Open();
            var cols = new List<string>(columnValues.Keys);
            var vals = new List<string>();
            foreach (var col in cols)
                vals.Add($"@{col}");
            string insertSql = $"INSERT INTO [{tableName}] ([{string.Join("] , [", cols)}]) VALUES ({string.Join(", ", vals)})";
            using var insertCmd = new SqlCommand(insertSql, connection);
            foreach (var kvp in columnValues)
                insertCmd.Parameters.AddWithValue($"@{kvp.Key}", kvp.Value ?? DBNull.Value);
            int rows = insertCmd.ExecuteNonQuery();
            return $"Inserted {rows} row(s) into '{tableName}'.";
        }
        catch (Exception ex)
        {
            return $"Error inserting row: {ex.Message}";
        }
    }
}