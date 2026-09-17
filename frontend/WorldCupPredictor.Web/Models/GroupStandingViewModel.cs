namespace WorldCupPredictor.Web.Models;

public sealed class GroupStandingViewModel
{
    public required string Letter { get; init; }
    public required string AccentCssVariable { get; init; }
    public required IReadOnlyList<TeamStandingViewModel> Teams { get; init; }
}

public sealed class TeamStandingViewModel
{
    public required string Name { get; init; }
    public required string FlagPath { get; init; }
    public required int Points { get; init; }
}
