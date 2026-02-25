using FluentValidation.TestHelper;
using JobHunter07.API.Features.Crm.Companies.CreateCompany;
using Xunit;

namespace JobHunter07.API.Tests.Validators;

public class CreateCompanyValidatorTests
{
    private readonly CreateCompanyValidator _validator = new CreateCompanyValidator();

    [Fact]
    public void Should_Have_Error_When_Name_Is_Null_Or_Empty()
    {
        var model = new CreateCompanyRequest(string.Empty, null, null, null, null, null);
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Should_Have_Error_When_Name_Too_Short()
    {
        var model = new CreateCompanyRequest("A", null, null, null, null, null);
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Should_Have_Error_When_WebsiteUrl_Invalid()
    {
        var model = new CreateCompanyRequest("Valid Name", null, null, null, "not-a-url", null);
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.WebsiteUrl);
    }

    [Fact]
    public void Should_Have_Error_When_LinkedInUrl_Invalid()
    {
        var model = new CreateCompanyRequest("Valid Name", null, null, null, null, "https://linkedin.com/in/person");
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.LinkedInUrl);
    }
}
