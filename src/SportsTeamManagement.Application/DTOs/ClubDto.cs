namespace SportsTeamManagement.Application.DTOs;

public sealed class ClubDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? WebsiteUrl { get; set; }
    public string? LogoUrl { get; set; }
    public string? Location { get; set; }
    public string? TimeZone { get; set; }
    public bool BeltGradingEnabled { get; set; }
    public bool IsActive { get; set; }
}

public class CreateClubDto
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? WebsiteUrl { get; set; }
    public string? LogoUrl { get; set; }
    public string? Location { get; set; }
    public string? TimeZone { get; set; }
    public bool BeltGradingEnabled { get; set; }
}

public sealed class UpdateClubDto : CreateClubDto
{
    public bool IsActive { get; set; } = true;
}
