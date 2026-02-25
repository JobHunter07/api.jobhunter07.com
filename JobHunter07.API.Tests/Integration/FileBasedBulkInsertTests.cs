using System;
using System.Data;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Xunit;

namespace JobHunter07.API.Tests.Integration;

public class FileBasedBulkInsertTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public FileBasedBulkInsertTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task BulkInsert_FromJsonFile_IntoLocalDb()
    {
        if (!TestConfig.GetBool("RUN_LARGE_TESTS", false))
            return;

        var fileSetting = TestConfig.GetString("MOCK_DATA_FILE") ?? Path.Combine("tools", "MockDataGenerator", "Mock-Companies-One-Million-Records.json");
        var file = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "..", fileSetting));
        if (!File.Exists(file))
            throw new FileNotFoundException("Mock data file not found. Run the generator first.", file);

        // Ensure app factory runs migrations
        using var client = _factory.CreateClient();

        using var stream = File.OpenRead(file);
        var doc = await JsonDocument.ParseAsync(stream);

        var target = TestConfig.GetInt("RECORD_COUNT", int.MaxValue);
        var batchSize = TestConfig.GetInt("BATCH_SIZE_SQL", 50_000);
        var batchTable = CreateTableSchema();
        await using var conn = new SqlConnection(_factory.ConnectionString);
        await conn.OpenAsync();

        int added = 0;

        foreach (var el in doc.RootElement.EnumerateArray())
        {
            if (added >= target) break;
            var id = Guid.Parse(el.GetProperty("CompanyId").GetString());
            var name = el.GetProperty("Name").GetString();
            var domain = el.GetProperty("Domain").GetString();
            var desc = el.GetProperty("Description").GetString();
            var industry = el.GetProperty("Industry").GetString();
            var website = el.GetProperty("WebsiteUrl").GetString();
            var linkedin = el.GetProperty("LinkedInUrl").GetString();
            var created = el.GetProperty("CreatedAt").GetDateTime();
            var updated = el.GetProperty("UpdatedAt").GetDateTime();
            var isActive = el.GetProperty("IsActive").GetBoolean();

            batchTable.Rows.Add(id, created, desc, domain, industry, isActive, linkedin, name, updated, website);
            added++;

            if (batchTable.Rows.Count >= batchSize)
            {
                await WriteBatch(conn, batchTable);
                batchTable.Clear();
            }
        }

        if (batchTable.Rows.Count > 0)
            await WriteBatch(conn, batchTable);

        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT COUNT(*) FROM dbo.Companies WHERE IsActive = 1";
        var count = Convert.ToInt32(await cmd.ExecuteScalarAsync());

        Assert.True(count >= added, $"Inserted {added} rows, DB reports {count}");
    }

    private static DataTable CreateTableSchema()
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
        return table;
    }

    private static async Task WriteBatch(SqlConnection conn, DataTable table)
    {
        using var bulk = new SqlBulkCopy(conn)
        {
            DestinationTableName = "dbo.Companies",
            BatchSize = table.Rows.Count,
            BulkCopyTimeout = 0
        };

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
    }
}
