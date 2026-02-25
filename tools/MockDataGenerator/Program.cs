using Bogus;
using System.Globalization;
using System.Text.Json;

// Streams a JSON array of company objects to a file. Use arguments: <outputPath> <count>
var output = args.Length > 0 ? args[0] : "Mock-Companies-One-Hundred-Records.json";
var count = args.Length > 1 && int.TryParse(args[1], out var c) ? c : 100;

Console.WriteLine($"Generating {count:N0} mock companies to {output}...");

// https://github.com/bchavez/Bogus

//Company

//    CompanySuffix - Get a company suffix. "Inc" and "LLC" etc.
//    CompanyName - Get a company name.
//    CatchPhrase - Get a company catch phrase.
//    Bs - Get a company BS phrase.

//Internet

//    Avatar - Generates a legit Internet URL avatar from twitter accounts.
//    Email - Generates an email address.
//    ExampleEmail - Generates an example email with @example.com.
//    UserName - Generates user names.
//    UserNameUnicode - Generates a user name preserving Unicode characters.
//    DomainName - Generates a random domain name.
//    DomainWord - Generates a domain word used for domain names.
//    DomainSuffix - Generates a domain name suffix like .com, .net, .org
//    Color - Gets a random aesthetically pleasing color near the base RGB. See here.
//    Url - Generates a random URL.

var faker = new Faker<CompanyDto>("en")
    .RuleFor(c => c.CompanyId, f => f.Random.Guid())
    .RuleFor(c => c.Name, f => $"{f.Company.CompanyName()} {f.Random.Guid().ToString("N").Substring(0,2)}")
    .RuleFor(c => c.Domain, f => $"{f.Internet.DomainWord()}-{f.Random.AlphaNumeric(2)}.{f.Internet.DomainSuffix()}")
    .RuleFor(c => c.Description, f => $"{f.Company.CatchPhrase()} {f.Company.Bs()}")
    .RuleFor(c => c.Industry, f => f.Commerce.Department())
    .RuleFor(c => c.WebsiteUrl, f => f.Internet.Url())
    .RuleFor(c => c.LinkedInUrl, f => $"https://www.linkedin.com/company/{f.Company.CompanyName().Replace(" ", "-").ToLowerInvariant()}")
    .RuleFor(c => c.CreatedAt, f => DateTime.UtcNow)
    .RuleFor(c => c.UpdatedAt, f => DateTime.UtcNow)
    .RuleFor(c => c.IsActive, f => true);

using var fs = File.Create(output);
using var writer = new Utf8JsonWriter(fs, new JsonWriterOptions { Indented = false });

writer.WriteStartArray();

for (int i = 0; i < count; i++)
{
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

    if ((i + 1) % 100 == 0)
        Console.WriteLine($"Generated {i + 1:N0}");
}

writer.WriteEndArray();
writer.Flush();

Console.WriteLine("Generation complete.");
