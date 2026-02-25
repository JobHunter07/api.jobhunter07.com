using System.Net.Http.Json;
using System.Text.RegularExpressions;
using Xunit;
using JobHunter07.API.Features.Crm.Companies.CreateCompany;
using JobHunter07.API.Features.Crm.Companies.GetCompanyById;
using JobHunter07.API.Features.Crm.Companies.SearchCompanies;
using JobHunter07.API.Features.Crm.Companies.UpdateCompany;
using JobHunter07.API.Features.Crm.Companies.DeleteCompany;
using System.Linq;
using System.Threading.Tasks;

namespace JobHunter07.API.Tests.Integration;

public class CompanyIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public CompanyIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task CreateCompany_EndToEnd_ReturnsCreatedCompany()
    {
        using var client = _factory.CreateClient();

        var req = new CreateCompanyRequest("Acme Integration", "acme-int.com", "Desc", "Industry", "https://acme.example", "https://www.linkedin.com/company/acme-int");

        var resp = await client.PostAsJsonAsync("/crm/companies", req);
        resp.EnsureSuccessStatusCode();

        var created = await resp.Content.ReadFromJsonAsync<CreateCompanyResponse>();
        Assert.NotNull(created);
        Assert.Equal(req.Name, created!.Name);
        Assert.Equal(req.Domain, created.Domain);
    }

    [Fact]
    public async Task CreateThenGetCompany_ById_ReturnsCompany()
    {
        using var client = _factory.CreateClient();

        var req = new CreateCompanyRequest("Beta Integration", "beta-int.com", "Desc", "Industry", "https://beta.example", "https://www.linkedin.com/company/beta-int");
        var post = await client.PostAsJsonAsync("/crm/companies", req);
        post.EnsureSuccessStatusCode();
        var created = await post.Content.ReadFromJsonAsync<CreateCompanyResponse>();

        var get = await client.GetAsync($"/crm/companies/{created!.CompanyId}");
        get.EnsureSuccessStatusCode();
        var fetched = await get.Content.ReadFromJsonAsync<GetCompanyByIdResponse>();

        Assert.NotNull(fetched);
        Assert.Equal(created.CompanyId, fetched!.CompanyId);
        Assert.Equal(req.Name, fetched.Name);
    }

    [Fact]
    public async Task UpdateCompany_PartialUpdate_OnlyNameChanged()
    {
        using var client = _factory.CreateClient();

        var create = new CreateCompanyRequest("Upd Integration", "upd-int.com", "Desc", "Industry", "https://upd.example", "https://www.linkedin.com/company/upd-int");
        var post = await client.PostAsJsonAsync("/crm/companies", create);
        post.EnsureSuccessStatusCode();
        var created = await post.Content.ReadFromJsonAsync<CreateCompanyResponse>();

        var updateReq = new UpdateCompanyRequest(created!.CompanyId, "Upd Integration New", null, null, null, null, null);
        var put = await client.PutAsJsonAsync($"/crm/companies/{created.CompanyId}", updateReq);
        put.EnsureSuccessStatusCode();
        var updated = await put.Content.ReadFromJsonAsync<UpdateCompanyResponse>();

        Assert.NotNull(updated);
        Assert.Equal("Upd Integration New", updated!.Name);
        Assert.Equal(create.Domain, updated.Domain);
    }

    [Fact]
    public async Task DeleteCompany_SoftDelete_ThenGetReturnsNotFound()
    {
        using var client = _factory.CreateClient();

        var create = new CreateCompanyRequest("Del Integration", "del-int.com", "Desc", "Industry", "https://del.example", "https://www.linkedin.com/company/del-int");
        var post = await client.PostAsJsonAsync("/crm/companies", create);
        post.EnsureSuccessStatusCode();
        var created = await post.Content.ReadFromJsonAsync<CreateCompanyResponse>();

        var del = await client.DeleteAsync($"/crm/companies/{created!.CompanyId}");
        del.EnsureSuccessStatusCode();

        var get = await client.GetAsync($"/crm/companies/{created.CompanyId}");
        Assert.Equal(System.Net.HttpStatusCode.NotFound, get.StatusCode);
        var err = await get.Content.ReadFromJsonAsync<JobHunter07.API.Abstractions.Errors.Error>();
        Assert.NotNull(err);
        Assert.Equal(JobHunter07.API.Abstractions.Errors.ErrorType.NotFound, err!.Type);
    }

    [Fact]
    public async Task SearchCompanies_PaginationAndFilters_ExcludesSoftDeleted()
    {
        using var client = _factory.CreateClient();

        // create 5 companies
        for (int i = 1; i <= 5; i++)
        {
            var req = new CreateCompanyRequest($"SearchCo {i}", $"search{i}.example", null, null, null, null);
            var r = await client.PostAsJsonAsync("/crm/companies", req);
            r.EnsureSuccessStatusCode();
        }

        // soft-delete one
        var listResp = await client.GetFromJsonAsync<SearchCompaniesResponse>($"/crm/companies?page=1&pageSize=10");
        Assert.NotNull(listResp);
        var toDelete = listResp!.Companies.First().CompanyId;
        var del = await client.DeleteAsync($"/crm/companies/{toDelete}");
        del.EnsureSuccessStatusCode();

        // search page size 2
        var page1 = await client.GetFromJsonAsync<SearchCompaniesResponse>($"/crm/companies?page=1&pageSize=2");
        Assert.NotNull(page1);
        Assert.Equal(2, page1!.PageSize);
        Assert.Equal(1, page1.Page);
        Assert.Equal(4, page1.TotalCount); // one soft-deleted excluded
        Assert.Equal(2, page1.Companies.Count());
    }

    [Fact]
    public async Task CreateCompany_NameConflict_ReturnsConflictError()
    {
        using var client = _factory.CreateClient();

        var req1 = new CreateCompanyRequest("ConflictCo", "conflict.example", null, null, null, null);
        var r1 = await client.PostAsJsonAsync("/crm/companies", req1);
        r1.EnsureSuccessStatusCode();

        var req2 = new CreateCompanyRequest("conflictco", "other.example", null, null, null, null);
        var r2 = await client.PostAsJsonAsync("/crm/companies", req2);
        // endpoint maps failure to BadRequest; assert the error payload mentions the conflict code
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, r2.StatusCode);
        var body = await r2.Content.ReadAsStringAsync();
        // ensure the conflict code appears exactly once
        var occ = Regex.Matches(body ?? string.Empty, @"Companies\.NameConflict").Count;
        Assert.Equal(1, occ);
    }

    [Fact]
    public async Task CreateCompany_InvalidLinkedInUrl_ReturnsValidation()
    {
        using var client = _factory.CreateClient();

        var req = new CreateCompanyRequest("InvalidLiCo", "inv.example", null, null, null, "https://linkedin.com/in/person");
        var r = await client.PostAsJsonAsync("/crm/companies", req);
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, r.StatusCode);
        var body = await r.Content.ReadAsStringAsync();
        Assert.Contains("Validation", body);
    }
}
