using System.Net.Http.Json;
using FluentAssertions;
using Xunit;
using JobHunter07.API.Features.Crm.Companies.CreateCompany;
using JobHunter07.API.Features.Crm.Companies.GetCompanyById;
using JobHunter07.API.Features.Crm.Companies.SearchCompanies;
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
        created.Should().NotBeNull();
        created!.Name.Should().Be(req.Name);
        created.Domain.Should().Be(req.Domain);
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

        fetched.Should().NotBeNull();
        fetched!.CompanyId.Should().Be(created.CompanyId);
        fetched.Name.Should().Be(req.Name);
    }
}
