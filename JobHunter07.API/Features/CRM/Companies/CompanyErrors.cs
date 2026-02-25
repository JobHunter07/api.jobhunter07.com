using JobHunter07.API.Abstractions.Errors;

namespace JobHunter07.API.Features.Crm.Companies;

public static class CompanyErrors
{
    public static Error NotFound(Guid id) =>
        Error.NotFound("Companies.NotFound", $"The Company with Id '{id}' was not found");

    public static Error NameConflict(string name) =>
        Error.Conflict("Companies.NameConflict", $"A company named '{name}' already exists.");

    public static Error DomainConflict(string domain) =>
        Error.Conflict("Companies.DomainConflict", $"A company with domain '{domain}' already exists.");
}
