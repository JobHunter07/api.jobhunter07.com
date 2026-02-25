using System;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

// Composition root using Generic Host
using var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((_, services) =>
    {
        // common
        services.AddSingleton<IOutputFormatter, ConsoleOutputFormatter>();
        services.AddSingleton<IFileProvider, FileProvider>();
        // ConsoleMenu has primitive constructor params so create via factory to allow defaults
        services.AddSingleton<IMenu>(_ => new ConsoleMenu());

        // feature generators
        services.AddSingleton<IGenerator, CompanyGenerator>();

        // factory
        services.AddSingleton<IGeneratorFactory, GeneratorFactory>();
    })
    .Build();

var services = host.Services;
var menu = services.GetRequiredService<IMenu>();
var formatter = services.GetRequiredService<IOutputFormatter>();
var fileProvider = services.GetRequiredService<IFileProvider>();
var factory = services.GetRequiredService<IGeneratorFactory>();

var generators = factory.GetAll().ToList();

// Cancellation support (Ctrl+C)
using var cts = new System.Threading.CancellationTokenSource();
Console.CancelKeyPress += (_, e) =>
{
    e.Cancel = true; // prevent the process from terminating immediately
    cts.Cancel();
    Console.WriteLine("Cancellation requested. Finishing current work and exiting...");
};

// If args were provided, run a single non-interactive invocation and exit.
if (args.Length > 0)
{
    var (genId, requestedOutput, count, compress) = menu.GetSelection(args, generators);
    if (!string.IsNullOrWhiteSpace(genId) && factory.GetById(genId) is IGenerator g)
    {
        try
        {
            // Build final filename and place under /data/{generator-displayname}/ when relative
            var baseName = Path.GetFileNameWithoutExtension(requestedOutput);
            var rawGenName = g.DisplayName ?? g.Id ?? "generator";
            var genPart = rawGenName.ToLowerInvariant().Replace(" ", "-");
            genPart = Regex.Replace(genPart, "[^a-z0-9-]", string.Empty);
            genPart = Regex.Replace(genPart, "-{2,}", "-");
            var finalFileName = $"{baseName}-{genPart}-{count}-Records{(compress ? ".zip" : ".json")}".ToLowerInvariant();

            string finalRequested;
            if (Path.IsPathRooted(requestedOutput))
            {
                // preserve absolute path directory
                var dirPart = Path.GetDirectoryName(requestedOutput) ?? string.Empty;
                finalRequested = string.IsNullOrWhiteSpace(dirPart) ? finalFileName : Path.Combine(dirPart, finalFileName);
            }
            else
            {
                // place into project relative data folder by generator display name
                var relativeDir = Path.Combine("data", genPart);
                finalRequested = Path.Combine(relativeDir, finalFileName);
            }

            var resolved = fileProvider.ResolveOutputPath(finalRequested);
            formatter.Info($"Generating {count:N0} {g.DisplayName} to {resolved}...");
            g.Generate(finalRequested, count, compress, cts.Token);
        }
        catch (OperationCanceledException)
        {
            formatter.Info("Generation cancelled.");
        }
    }
    return;
}

// Interactive loop: allow multiple generations until user quits
while (true)
{
    var (genId, requestedOutput, count, compress) = menu.GetSelection(args, generators);
    if (genId == null)
        break; // user chose to quit

    var generator = factory.GetById(genId);
    if (generator == null)
    {
        Console.WriteLine("Selected generator not found. Returning to menu.");
        continue;
    }

    // Build final filename and place under /data/{generator-displayname}/ when relative
    var baseName = Path.GetFileNameWithoutExtension(requestedOutput);
    var rawGenName2 = generator.DisplayName ?? generator.Id ?? "generator";
    var genPart2 = rawGenName2.ToLowerInvariant().Replace(" ", "-");
    genPart2 = Regex.Replace(genPart2, "[^a-z0-9-]", string.Empty);
    genPart2 = Regex.Replace(genPart2, "-{2,}", "-");
    var finalFileName = $"{baseName}-{genPart2}-{count}-Records{(compress ? ".zip" : ".json")}".ToLowerInvariant();

    string finalRequested;
    if (Path.IsPathRooted(requestedOutput))
    {
        var dirPart = Path.GetDirectoryName(requestedOutput) ?? string.Empty;
        finalRequested = string.IsNullOrWhiteSpace(dirPart) ? finalFileName : Path.Combine(dirPart, finalFileName);
    }
    else
    {
        var relativeDir = Path.Combine("data", genPart2);
        finalRequested = Path.Combine(relativeDir, finalFileName);
    }

    var resolved = fileProvider.ResolveOutputPath(finalRequested);
    formatter.Info($"Generating {count:N0} {generator.DisplayName} to {resolved}...");
    try
    {
        generator.Generate(finalRequested, count, compress, cts.Token);
    }
    catch (OperationCanceledException)
    {
        formatter.Info("Generation cancelled.");
    }

    Console.WriteLine();
    Console.WriteLine("Press Enter to return to main menu or Q to quit.");
    var key = Console.ReadLine();
    if (!string.IsNullOrWhiteSpace(key) && key.Equals("Q", StringComparison.OrdinalIgnoreCase))
        break;
}
