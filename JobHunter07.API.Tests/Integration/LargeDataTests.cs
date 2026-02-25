using System;
using System.Data;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Xunit;

namespace JobHunter07.API.Tests.Integration;

// Long-running test that seeds a real LocalDB database with 1,000,000 Company rows
// This test is guarded by the environment variable RUN_LARGE_TESTS=1 to avoid accidental execution.
public class LargeDataTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public LargeDataTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task InsertAndVerifyOneMillionRecords_LocalDb_Bulk()
    {
        if (!TestConfig.GetBool("RUN_LARGE_TESTS", false))
        {
            // Skip by returning early; test will be treated as passing unless the env var is set.
            return;
        }

        var total = TestConfig.GetInt("RECORD_COUNT", 1_000_000);
        var batchSize = TestConfig.GetInt("BATCH_SIZE_SQL", 50_000);

        var sw = Stopwatch.StartNew();

        // Ensure the application factory starts and migrations are applied
        using var client = _factory.CreateClient();

        using var conn = new SqlConnection(_factory.ConnectionString);
        await conn.OpenAsync();

        var inserted = 0;
        var now = DateTime.UtcNow;

        var batches = (total + batchSize - 1) / batchSize;
        for (int batch = 0; batch < batches; batch++)
        {
            var table = new DataTable();
            table.Columns.Add("CompanyId", typeof(Guid));
            table.Columns.Add("CreatedAt", typeof(DateTime));
            table.Columns.Add("Description", typeof(string));
            table.Columns.Add("Domain", typeof(string));
            table.Columns.Add("Industry", typeof(string));
            table.Columns.Add("IsActive", typeof(bool));
            table.Columns.Add("LinkedInUrl", typeof(string));
            table.Columns.Add("Name", typeof(string));
            table.Columns.Add("UpdatedAt", typeof(DateTime));
            table.Columns.Add("WebsiteUrl", typeof(string));

            for (int i = 0; i < batchSize; i++)
            {
                var idx = batch * batchSize + i;
                if (idx >= total) break;
                var id = Guid.NewGuid();
                var name = $"BulkCo-{batch}-{i}-{id:D}";
                var domain = $"bulk{batch}-{i}.example";
                table.Rows.Add(id, now, null, domain, "TestIndustry", true, null, name, now, null);
            }

            using var bulk = new SqlBulkCopy(conn)
            {
                DestinationTableName = "dbo.Companies",
                BulkCopyTimeout = 0,
                BatchSize = batchSize
            };

            // Map columns by name
            bulk.ColumnMappings.Add("CompanyId", "CompanyId");
            bulk.ColumnMappings.Add("CreatedAt", "CreatedAt");
            bulk.ColumnMappings.Add("Description", "Description");
            bulk.ColumnMappings.Add("Domain", "Domain");
            bulk.ColumnMappings.Add("Industry", "Industry");
            bulk.ColumnMappings.Add("IsActive", "IsActive");
            bulk.ColumnMappings.Add("LinkedInUrl", "LinkedInUrl");
            bulk.ColumnMappings.Add("Name", "Name");
            bulk.ColumnMappings.Add("UpdatedAt", "UpdatedAt");
            bulk.ColumnMappings.Add("WebsiteUrl", "WebsiteUrl");

            await bulk.WriteToServerAsync(table);
            inserted += table.Rows.Count;
        }

        // Verify count
        using var countCmd = conn.CreateCommand();
        countCmd.CommandText = "SELECT COUNT(*) FROM dbo.Companies WHERE IsActive = 1";
        var countObj = await countCmd.ExecuteScalarAsync();
        var count = Convert.ToInt32(countObj);

        sw.Stop();

        // Simple assertion: expect at least the requested number (there may be pre-existing rows in DB depending on setup)
        Assert.True(count >= total, $"Expected at least {total} active companies but found {count}. Time: {sw.Elapsed}");
    }
}
