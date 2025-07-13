# McpSQLServerApp

This application provides a set of tools for interacting with a SQL Server database.

## Build and Publish

To build and publish the application to a local folder, use the following command:

```bash
dotnet publish -c Release -o C:\mcp-servers\McpSqlServer
```

## Running the Application

After publishing, you can run the application from the output folder:

```bash
cd C:\mcp-servers\McpSqlServer
.\McpSQLServerApp.exe
```

To enable console logging, use the `-l` flag:

```bash
.\McpSQLServerApp.exe -l
```
