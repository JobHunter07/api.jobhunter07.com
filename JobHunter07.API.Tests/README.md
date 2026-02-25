# JobHunter07.API.Tests — Test runner and large-data guidance

This document explains how to run the integration and large-data tests that target a real LocalDB instance. Heavy operations are guarded so they only run when explicitly enabled.

- Location: `JobHunter07.API.Tests` project root.

Quick commands (PowerShell)

- Generate 1,000,000 mock companies (uses the `tools/MockDataGenerator` project):
```powershell
cd tools\MockDataGenerator
dotnet run --project .\MockDataGenerator.csproj -- Mock-Companies-One-Million-Records.json 1000000
```

- Run the small verification / bulk-insert tests (guarded by env var):
```powershell
set RUN_LARGE_TESTS=1
dotnet test JobHunter07.API.Tests -c Debug --filter FullyQualifiedName~FileBasedBulkInsertTests
```

- Run the API-based batch-posting test (posts batches through the running test server):
```powershell
set RUN_LARGE_TESTS=1
dotnet test JobHunter07.API.Tests -c Debug --filter FullyQualifiedName~ApiFileInsertTests
```

- Run the direct SqlBulkCopy large test (uses in-memory generator if present):
```powershell
set RUN_LARGE_TESTS=1
dotnet test JobHunter07.API.Tests -c Debug --filter FullyQualifiedName~LargeDataTests
```

Notes and safety

- All large tests are gated by `RUN_LARGE_TESTS=1`. Do NOT set this in shared CI jobs unless you intend to run very long workloads.
- Tests and the generator use LocalDB `(localdb)\\MSSQLLocalDB`. Ensure LocalDB is installed and available on the runner.
- The JSON file produced by the generator is large — plan disk space accordingly (many GBs for 1,000,000 records depending on fields).
- These tests create a per-run ephemeral database named like `JobHunter07_Test_{GUID}` and the test factory will attempt to drop it on disposal. If a test run aborts, you may need to clean up the DB from SQL Server Management Studio or via `sqlcmd`.
- If you want a quicker profiling run, generate fewer records (e.g., 100,000) by passing a smaller number to the generator.

Troubleshooting

- If you see login failures when connecting to LocalDB, verify your account has access to `(localdb)\\MSSQLLocalDB` and that LocalDB instances are running.
- If the tests fail with unique constraint errors during API posting, the API maps those to handled conflict errors. For concurrency testing consider running the SqlBulkCopy path which bypasses API-level checks.


