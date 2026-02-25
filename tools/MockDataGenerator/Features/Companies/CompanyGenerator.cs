using Bogus;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using Spectre.Console;

public class CompanyGenerator : ICompanyGenerator, IGenerator
{
    private readonly IOutputFormatter _formatter;
    private readonly IFileProvider _fileProvider;

    public CompanyGenerator(IOutputFormatter formatter, IFileProvider fileProvider)
    {
        _formatter = formatter;
        _fileProvider = fileProvider;
    }

    public string Id => "companies";
    public string DisplayName => "Companies";

    public void Generate(string outputPath, int count, bool compress, CancellationToken cancellationToken)
    {
        // Resolve final absolute path and ensure directory exists
        var resolvedPath = _fileProvider.ResolveOutputPath(outputPath);
        _fileProvider.EnsureDirectory(resolvedPath);

        // Create a temp file in the same directory to avoid partial output on cancel
        var dir = Path.GetDirectoryName(resolvedPath) ?? Environment.CurrentDirectory;
        var tempFile = Path.Combine(dir, Path.GetRandomFileName());

        var faker = new Faker<CompanyDto>("en")
            .RuleFor(c => c.CompanyId, f => f.Random.Guid())
            .RuleFor(c => c.Name, f => $"{f.Company.CompanyName()} {f.Random.Guid().ToString("N").Substring(0,2)}")
            .RuleFor(c => c.Domain, f => $"{f.Internet.DomainWord()}-{f.Random.AlphaNumeric(2)}.{f.Internet.DomainSuffix()}")
            .RuleFor(c => c.Description, f => $"{f.Company.CatchPhrase()} {f.Company.Bs()}")
            .RuleFor(c => c.Industry, f => f.Commerce.Department())
            .RuleFor(c => c.WebsiteUrl, f => f.Internet.Url())
            .RuleFor(c => c.LinkedInUrl, f =>
            {
                var name = f.Company.CompanyName();
                var slug = name.Replace(",", "-").Replace(" ", "-").ToLowerInvariant();
                slug = Regex.Replace(slug, "[^a-z0-9-]", string.Empty);
                slug = Regex.Replace(slug, "-{2,}", "-");
                return $"https://www.linkedin.com/company/{slug}";
            })
            .RuleFor(c => c.CreatedAt, f => DateTime.UtcNow)
            .RuleFor(c => c.UpdatedAt, f => DateTime.UtcNow)
            .RuleFor(c => c.IsActive, f => true);

        // Stream out JSON to temp file, optionally gzip
        try
        {
            if (compress)
            {
                // Create a .zip file containing a single JSON entry
                using (var fs = new FileStream(tempFile, FileMode.Create, FileAccess.Write, FileShare.None, 65536, FileOptions.SequentialScan))
                using (var zip = new ZipArchive(fs, ZipArchiveMode.Create, leaveOpen: false))
                {
                    var entryName = Path.GetFileNameWithoutExtension(resolvedPath) + ".json";
                    var entry = zip.CreateEntry(entryName, CompressionLevel.Optimal);
                    using var entryStream = entry.Open();
                    using var writer = new Utf8JsonWriter(entryStream, new JsonWriterOptions { Indented = false });

                    writer.WriteStartArray();

                    AnsiConsole.Progress().Start(ctx =>
                    {
                        var task = ctx.AddTask("Generating", maxValue: count);

                        for (int i = 0; i < count; i++)
                        {
                            cancellationToken.ThrowIfCancellationRequested();

                            var item = faker.Generate();

                            writer.WriteStartObject();
                            writer.WriteString("CompanyId", item.CompanyId.ToString());
                            writer.WriteString("Name", item.Name);
                            writer.WriteString("Domain", item.Domain);
                            writer.WriteString("Description", item.Description);
                            writer.WriteString("Industry", item.Industry);
                            writer.WriteString("WebsiteUrl", item.WebsiteUrl);
                            writer.WriteString("LinkedInUrl", item.LinkedInUrl);
                            writer.WriteString("CreatedAt", item.CreatedAt.ToString("o", CultureInfo.InvariantCulture));
                            writer.WriteString("UpdatedAt", item.UpdatedAt.ToString("o", CultureInfo.InvariantCulture));
                            writer.WriteBoolean("IsActive", item.IsActive);
                            writer.WriteEndObject();

                            task.Increment(1);

                            if (i % 1000 == 0)
                                writer.Flush();
                        }

                        writer.WriteEndArray();
                        writer.Flush();
                    });
                }
            }
            else
            {
                using (var fs = new FileStream(tempFile, FileMode.Create, FileAccess.Write, FileShare.None, 65536, FileOptions.SequentialScan))
                using (var writer = new Utf8JsonWriter(fs, new JsonWriterOptions { Indented = false }))
                {
                    writer.WriteStartArray();

                    AnsiConsole.Progress().Start(ctx =>
                    {
                        var task = ctx.AddTask("Generating", maxValue: count);

                        for (int i = 0; i < count; i++)
                        {
                            cancellationToken.ThrowIfCancellationRequested();

                            var item = faker.Generate();

                            writer.WriteStartObject();
                            writer.WriteString("CompanyId", item.CompanyId.ToString());
                            writer.WriteString("Name", item.Name);
                            writer.WriteString("Domain", item.Domain);
                            writer.WriteString("Description", item.Description);
                            writer.WriteString("Industry", item.Industry);
                            writer.WriteString("WebsiteUrl", item.WebsiteUrl);
                            writer.WriteString("LinkedInUrl", item.LinkedInUrl);
                            writer.WriteString("CreatedAt", item.CreatedAt.ToString("o", CultureInfo.InvariantCulture));
                            writer.WriteString("UpdatedAt", item.UpdatedAt.ToString("o", CultureInfo.InvariantCulture));
                            writer.WriteBoolean("IsActive", item.IsActive);
                            writer.WriteEndObject();

                            task.Increment(1);

                            if (i % 1000 == 0)
                                writer.Flush();
                        }

                        writer.WriteEndArray();
                        writer.Flush();
                    });
                }
            }

            // Move temp file to final path (overwrite if exists)
            if (File.Exists(resolvedPath))
                File.Delete(resolvedPath);
            File.Move(tempFile, resolvedPath);

            _formatter.Success($"Generated {count:N0} records to {resolvedPath}");
        }
        catch (OperationCanceledException)
        {
            // cleanup temp file
            try { if (File.Exists(tempFile)) File.Delete(tempFile); } catch { }
            _formatter.Info("Generation cancelled by user.");
            throw;
        }
    }
}
