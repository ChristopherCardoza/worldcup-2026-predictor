using WorldCupPredictor.API.Models;

namespace WorldCupPredictor.API.Data;

public static class DbSeeder
{
    public static void Seed(AppDbContext db)
    {
        if (db.Tournaments.Any()) return; // already seeded

        var tournament = new Tournament
        {
            Name = "FIFA World Cup",
            Year = 2026
        };
        db.Tournaments.Add(tournament);
        db.SaveChanges();

        // Just 4 countries for now
        var argentina = new Country { Name = "Argentina", IsoCode = "ARG", FlagPath = "/flags/arg.svg", OppCode = 8 };
        var brazil = new Country { Name = "Brazil", IsoCode = "BRA", FlagPath = "/flags/bra.svg" };
        var canada = new Country { Name = "Canada", IsoCode = "CAN", FlagPath = "/flags/can.svg" };
        var mexico = new Country { Name = "Mexico", IsoCode = "MEX", FlagPath = "/flags/mex.svg" };

        db.Countries.AddRange(argentina, brazil, canada, mexico);
        db.SaveChanges();

        var groupA = new Group { TournamentId = tournament.Id, Name = "A" };
        db.Groups.Add(groupA);
        db.SaveChanges();

        db.GroupTeams.AddRange(
            new GroupTeam { GroupId = groupA.Id, CountryId = mexico.Id },
            new GroupTeam { GroupId = groupA.Id, CountryId = canada.Id },
            new GroupTeam { GroupId = groupA.Id, CountryId = argentina.Id },
            new GroupTeam { GroupId = groupA.Id, CountryId = brazil.Id }
        );

        // Example group fixtures (round robin for 4 teams)
        db.Fixtures.AddRange(
            new Fixture { TournamentId = tournament.Id, GroupId = groupA.Id, Matchday = 1, TeamAId = mexico.Id, TeamBId = canada.Id },
            new Fixture { TournamentId = tournament.Id, GroupId = groupA.Id, Matchday = 1, TeamAId = argentina.Id, TeamBId = brazil.Id },
            new Fixture { TournamentId = tournament.Id, GroupId = groupA.Id, Matchday = 2, TeamAId = mexico.Id, TeamBId = argentina.Id },
            new Fixture { TournamentId = tournament.Id, GroupId = groupA.Id, Matchday = 2, TeamAId = canada.Id, TeamBId = brazil.Id },
            new Fixture { TournamentId = tournament.Id, GroupId = groupA.Id, Matchday = 3, TeamAId = mexico.Id, TeamBId = brazil.Id },
            new Fixture { TournamentId = tournament.Id, GroupId = groupA.Id, Matchday = 3, TeamAId = canada.Id, TeamBId = argentina.Id }
        );

        db.SaveChanges();
    }
}