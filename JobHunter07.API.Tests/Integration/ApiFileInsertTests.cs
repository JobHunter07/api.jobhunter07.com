using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;
using JobHunter07.API.Features.Crm.Companies.CreateCompany;

namespace JobHunter07.API.Tests.Integration;

public class ApiFileInsertTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public ApiFileInsertTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task InsertOneMillionViaApi_InBatches_MeasureTime()
    {
        if (!TestConfig.GetBool("RUN_LARGE_TESTS", false))
            return;

        var fileSetting = TestConfig.GetString("MOCK_DATA_FILE") ?? Path.Combine("tools", "MockDataGenerator", "Mock-Companies-One-Million-Records.json");
        var file = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "..", fileSetting));
        if (!File.Exists(file))
            throw new FileNotFoundException("Mock data file not found. Run the generator first.", file);

        using var client = _factory.CreateClient();

        using var fs = File.OpenRead(file);
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        var target = TestConfig.GetInt("RECORD_COUNT", int.MaxValue);
        var batchSize = TestConfig.GetInt("BATCH_SIZE_API", 1000);
        var batch = new List<CreateCompanyRequest>(batchSize);
        var total = 0;
        var sw = System.Diagnostics.Stopwatch.StartNew();

        await foreach (var item in JsonSerializer.DeserializeAsyncEnumerable<CreateCompanyRequest>(fs, options))
        {
            if (item is null) continue;
            batch.Add(item);
            if (batch.Count >= batchSize)
            {
                await PostBatch(client, batch);
                total += batch.Count;
                batch.Clear();
            }
            if (total + batch.Count >= target) break;
        }

        if (batch.Count > 0)
        {
            await PostBatch(client, batch);
            total += batch.Count;
        }

        sw.Stop();
        // Record time to the test output
        Console.WriteLine($"Posted {total} records via API in {sw.Elapsed}");

        Assert.True(total > 0, "No records posted");
    }

    private static async Task PostBatch(System.Net.Http.HttpClient client, List<CreateCompanyRequest> batch)
    {
        var tasks = new List<Task<System.Net.Http.HttpResponseMessage>>(batch.Count);
        foreach (var req in batch)
            tasks.Add(client.PostAsJsonAsync("/crm/companies", req));

        var results = await Task.WhenAll(tasks);
        // Ensure none returned 500
        foreach (var r in results)
            if (r.StatusCode == System.Net.HttpStatusCode.InternalServerError)
                throw new Exception("Server returned 500 during batch POST");
    }
}
