using McpSQLServerApp.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using System.Reflection;
using System.ComponentModel;
using System.Linq;

var builder = Host.CreateApplicationBuilder(args);

builder.Logging.ClearProviders();

if (args.Contains("-l"))
{
    builder.Logging.AddConsole();

    var connectionString = ConfigHelper.GetDefaultConnectionString();
    Console.WriteLine($"Using connection string: {connectionString}");

    Console.WriteLine("Available tools:");
    var assembly = Assembly.GetExecutingAssembly();
    var toolTypes = assembly.GetTypes().Where(t => t.IsClass && t.GetCustomAttribute<McpServerToolTypeAttribute>() != null);

    foreach (var type in toolTypes)
    {
        var methods = type.GetMethods().Where(m => m.GetCustomAttribute<McpServerToolAttribute>() != null);
        foreach (var method in methods)
        {
            var description = method.GetCustomAttribute<DescriptionAttribute>()?.Description ?? "No description.";
            Console.WriteLine($"- {type.Name}.{method.Name}: {description}");
        }
    }
    Console.WriteLine();
}

builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly();

var app = builder.Build();

await app.RunAsync();
