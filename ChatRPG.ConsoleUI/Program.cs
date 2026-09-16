using ChatRPG.Agents;
using ChatRPG.Application;
using ChatRPG.ConsoleUI.Configuration;
using ChatRPG.ConsoleUI.Demo;
using ChatRPG.ConsoleUI.Menus;
using ChatRPG.ConsoleUI.Session;
using ChatRPG.Infrastructure;
using ChatRPG.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Spectre.Console;

bool demo = args.Contains("--demo", StringComparer.OrdinalIgnoreCase);

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
builder.Configuration.AddUserSecrets<Program>(optional: true);

var settingsStore = new PlayerSettingsStore();
PlayerSettings settings = settingsStore.Load();

builder.Services.AddSingleton(settingsStore);
builder.Services.AddSingleton(settings);
builder.Services.AddSingleton<SettingsEditor>();
builder.Services.AddSingleton<MainMenu>();
builder.Services.AddSingleton<GameLoop>();

if (demo)
{
    builder.Services.AddSingleton<IGameBackend, DemoGameBackend>();
}
else
{
    // AddInfrastructure registers the DbContext without a provider, and AddDbContext uses TryAdd
    // for the options, so the provider has to be registered first to win.
    string connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                              ?? throw new InvalidOperationException(
                                  "Connection string 'DefaultConnection' not found in appsettings.json.");
    builder.Services.AddDbContext<ChatRpgDbContext>(options => options.UseNpgsql(connectionString));

    builder.Services.AddApplication();
    builder.Services.AddAgents();
    builder.Services.AddInfrastructure();

    builder.Services.AddSingleton<IGameBackend, LiveGameBackend>();
}

using IHost host = builder.Build();

using var cancellation = new CancellationTokenSource();

try
{
    await host.Services.GetRequiredService<GameLoop>().RunAsync(cancellation.Token);
}
catch (OperationCanceledException)
{
    // Quitting mid-turn is not an error.
}
catch (Exception ex)
{
    // A TUI crash scrolls away fast, so keep a copy next to the settings file.
    Directory.CreateDirectory(settingsStore.DirectoryPath);
    await File.WriteAllTextAsync(Path.Combine(settingsStore.DirectoryPath, "crash.log"), ex.ToString());

    AnsiConsole.WriteLine();
    AnsiConsole.WriteException(ex, ExceptionFormats.ShortenEverything);
    return 1;
}

AnsiConsole.MarkupLine("[grey54]farewell.[/]");
return 0;

/// <summary>Anchors user-secrets and the host builder to this assembly.</summary>
public partial class Program;
