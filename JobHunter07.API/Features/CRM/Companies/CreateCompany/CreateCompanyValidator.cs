using FluentValidation;

namespace JobHunter07.API.Features.Crm.Companies.CreateCompany;

public class CreateCompanyValidator : AbstractValidator<CreateCompanyRequest>
{
    public CreateCompanyValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MinimumLength(2).WithMessage("Name must be at least 2 characters")
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters");

        RuleFor(x => x.Domain)
            .MaximumLength(200).WithMessage("Domain must not exceed 200 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.Domain));

        RuleFor(x => x.WebsiteUrl)
            .Must(url => Uri.IsWellFormedUriString(url, UriKind.Absolute))
            .WithMessage("WebsiteUrl must be a valid absolute URL")
            .When(x => !string.IsNullOrWhiteSpace(x.WebsiteUrl));

        RuleFor(x => x.LinkedInUrl)
            .Must(url => url != null && url.StartsWith("https://www.linkedin.com/company/"))
            .WithMessage("LinkedInUrl must be a LinkedIn company URL (https://www.linkedin.com/company/...)")
            .When(x => !string.IsNullOrWhiteSpace(x.LinkedInUrl));
    }
}
