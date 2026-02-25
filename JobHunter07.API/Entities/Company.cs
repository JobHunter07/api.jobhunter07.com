using System;

namespace JobHunter07.API.Entities;

public sealed class Company
{
    public Guid CompanyId { get; set; }
    public required string Name { get; set; }
    public string? Domain { get; set; }
    public string? Description { get; set; }
    public string? Industry { get; set; }
    public string? WebsiteUrl { get; set; }
    public string? LinkedInUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsActive { get; set; } = true;
}
