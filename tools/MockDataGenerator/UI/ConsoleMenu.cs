using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Spectre.Console;

public class ConsoleMenu : IMenu
{
    private readonly string _defaultFilename;
    private readonly int _defaultCount;

    public ConsoleMenu(string defaultFilename = "Mock-Companies-One-Hundred-Records.json", int defaultCount = 100)
    {
        _defaultFilename = defaultFilename;
        _defaultCount = defaultCount;
    }

    public (string? generatorId, string requestedOutput, int count, bool compress) GetSelection(string[] args, IEnumerable<IGenerator> generators)
    {
        var gens = generators?.ToList() ?? new List<IGenerator>();

        // If args provided try to interpret them. If first arg matches a generator id use it,
        // otherwise treat first arg as filename and default to companies generator.
        if (args.Length > 0)
        {
            string genId;
            string requested;
            int c;

            var first = args[0];
            var match = gens.FirstOrDefault(g => string.Equals(g.Id, first, StringComparison.OrdinalIgnoreCase));
            if (match != null)
            {
                genId = match.Id;
                requested = args.Length > 1 ? args[1] : _defaultFilename;
                c = args.Length > 2 && int.TryParse(args[2], out var parsed) ? parsed : _defaultCount;
            }
            else
            {
                genId = gens.FirstOrDefault(g => g.Id.Equals("companies", StringComparison.OrdinalIgnoreCase))?.Id
                        ?? gens.FirstOrDefault()?.Id ?? string.Empty;
                requested = first;
                c = args.Length > 1 && int.TryParse(args[1], out var parsed2) ? parsed2 : _defaultCount;
            }

            if (string.IsNullOrWhiteSpace(Path.GetExtension(requested)))
                requested = requested + ".json";

            return (genId, requested, c, false);
        }

        // Interactive selection using Spectre.Console
        while (true)
        {
            ConsoleUI.Header("Mock Data Generator");

            if (!gens.Any())
            {
                ConsoleUI.Error("No generators registered.");
                return (null, string.Empty, 0, false);
            }

            var choices = gens.Select(g => $"{g.DisplayName} ({g.Id})").ToList();
            choices.Add("Quit");

            var selection = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Select generator:")
                    .PageSize(10)
                    .AddChoices(choices));

            if (selection.Equals("Quit", StringComparison.OrdinalIgnoreCase))
                return (null, string.Empty, 0, false);

            // map back to generator
            var chosen = gens.FirstOrDefault(g => selection.StartsWith(g.DisplayName, StringComparison.OrdinalIgnoreCase));
            if (chosen == null)
            {
                ConsoleUI.Error("Invalid selection. Try again.");
                continue;
            }

            // Filename prompt
            var filePrompt = new TextPrompt<string>("Filename:")
                .DefaultValue(_defaultFilename)
                .Validate(fn => string.IsNullOrWhiteSpace(fn) ? ValidationResult.Error("Filename cannot be empty") : ValidationResult.Success());

            var requestedOutput = AnsiConsole.Prompt(filePrompt);
            if (string.IsNullOrWhiteSpace(Path.GetExtension(requestedOutput)))
                requestedOutput = requestedOutput + ".json";

            // Count prompt
            var countPrompt = new TextPrompt<int>("How many records to generate:")
                .DefaultValue(_defaultCount)
                .Validate(n => n <= 0 ? ValidationResult.Error("Count must be > 0") : ValidationResult.Success());

            var countResult = AnsiConsole.Prompt(countPrompt);

            // Ask whether to compress
            var compressPrompt = new SelectionPrompt<string>()
                .Title("Compress output?")
                .AddChoices(new[] { "No", "Yes (gzip)" });
            var compressChoice = AnsiConsole.Prompt(compressPrompt);
            var compress = compressChoice?.StartsWith("Yes", StringComparison.OrdinalIgnoreCase) ?? false;

            return (chosen.Id, requestedOutput, countResult, compress);
        }
    }
}
