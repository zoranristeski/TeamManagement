namespace SportsTeamManagement.Mobile.Models;

public sealed class ClubDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool BeltGradingEnabled { get; set; }
}
