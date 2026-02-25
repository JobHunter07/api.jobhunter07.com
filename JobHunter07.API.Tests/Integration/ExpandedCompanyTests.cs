using System.Net.Http.Json;
using Xunit;
using JobHunter07.API.Features.Crm.Companies.CreateCompany;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace JobHunter07.API.Tests.Integration;

public class ExpandedCompanyTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public ExpandedCompanyTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Concurrency_CreateSameName_OnlyOneSucceeds()
    {
        using var client = _factory.CreateClient();

        var name = "ConcurrentCo" + System.Guid.NewGuid();
        var tasks = new List<Task<System.Net.Http.HttpResponseMessage>>();

        for (int i = 0; i < 10; i++)
        {
            var req = new CreateCompanyRequest(name, $"con{i}.example", null, null, null, null);
            tasks.Add(client.PostAsJsonAsync("/crm/companies", req));
        }

        var results = await Task.WhenAll(tasks);

        var successCount = results.Count(r => r.IsSuccessStatusCode);
        var badRequestCount = results.Count(r => r.StatusCode == System.Net.HttpStatusCode.BadRequest);

        Assert.Equal(1, successCount);
        Assert.True(badRequestCount >= 1, "the other concurrent attempts should be rejected as conflicts");
    }

    [Fact]
    public async Task Fuzz_InvalidInputs_DoNotCrash_ProduceValidationOrBadRequest()
    {
        using var client = _factory.CreateClient();

        // Very long name
        var longName = new string('A', 5000);
        var longReq = new CreateCompanyRequest(longName, "long.example", null, null, null, null);
        var longResp = await client.PostAsJsonAsync("/crm/companies", longReq);
        Assert.NotEqual(System.Net.HttpStatusCode.InternalServerError, longResp.StatusCode);

        // SQL injection attempt in name
        var sqlName = "Robert'); DROP TABLE Companies; --" + System.Guid.NewGuid();
        var sqlReq = new CreateCompanyRequest(sqlName, "sqlinj.example", null, null, null, null);
        var sqlResp = await client.PostAsJsonAsync("/crm/companies", sqlReq);
        Assert.NotEqual(System.Net.HttpStatusCode.InternalServerError, sqlResp.StatusCode);

        // Invalid LinkedIn URL
        var badLiReq = new CreateCompanyRequest("BadLiCo" + System.Guid.NewGuid(), "badli.example", null, null, null, "https://linkedin.com/in/person");
        var badLiResp = await client.PostAsJsonAsync("/crm/companies", badLiReq);
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, badLiResp.StatusCode);

        // Confirm DB still responds (simple smoke GET)
        var page = await client.GetAsync($"/crm/companies?page=1&pageSize=1");
        Assert.Equal(System.Net.HttpStatusCode.OK, page.StatusCode);
    }

    [Fact]
    public async Task Load_ParallelCreates_QuickSmoke()
    {
        using var client = _factory.CreateClient();

        var tasks = new List<Task<System.Net.Http.HttpResponseMessage>>();
        int total = 30;

        for (int i = 0; i < total; i++)
        {
            var req = new CreateCompanyRequest($"LoadCo-{i}-{System.Guid.NewGuid()}", $"load{i}.example", null, null, null, null);
            tasks.Add(client.PostAsJsonAsync("/crm/companies", req));
        }

        var results = await Task.WhenAll(tasks);
        var successCount = results.Count(r => r.IsSuccessStatusCode);

        Assert.Equal(total, successCount);
    }
}
