using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using JobHunter07.API.Database;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;

namespace JobHunter07.API.Tests.Integration;

public sealed class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    public string ConnectionString { get; }

    private readonly string _databaseName;

    public CustomWebApplicationFactory()
    {
        _databaseName = $"JobHunter07_Test_{Guid.NewGuid():N}";
        ConnectionString = $"Server=(localdb)\\MSSQLLocalDB;Database={_databaseName};Integrated Security=True;MultipleActiveResultSets=true";
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureAppConfiguration((context, conf) =>
        {
            // Override connection string to point at a per-test LocalDB database
            var inMemory = new Dictionary<string, string>
            {
                ["ConnectionStrings:connection"] = ConnectionString
            };

            conf.AddInMemoryCollection(inMemory);
        });

        builder.ConfigureServices((context, services) =>
        {
            // Ensure database is created and migrations applied
            using var scope = services.BuildServiceProvider().CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            db.Database.Migrate();
        });
    }

    protected override void Dispose(bool disposing)
    {
        try
        {
            // Drop the test database
            var masterConn = new SqlConnection("Server=(localdb)\\MSSQLLocalDB;Database=master;Integrated Security=True");
            masterConn.Open();
            using var cmd = masterConn.CreateCommand();
            // Set single user and drop
            cmd.CommandText = $"IF DB_ID(N'{_databaseName}') IS NOT NULL BEGIN ALTER DATABASE [{_databaseName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [{_databaseName}]; END;";
            cmd.ExecuteNonQuery();
            masterConn.Close();
        }
        catch
        {
            // swallow - cleanup best-effort
        }

        base.Dispose(disposing);
    }
}
