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

        // Names match the ml-service's team_mapping.csv
        var countries = new Dictionary<string, Country>
        {
            // Group A
            ["Mexico"] = new() { Name = "Mexico", IsoCode = "MEX", FlagPath = "/flags/mex.svg", OppCode = 136 },
            ["South Africa"] = new() { Name = "South Africa", IsoCode = "RSA", FlagPath = "/flags/rsa.svg", OppCode = 201 },
            ["South Korea"] = new() { Name = "South Korea", IsoCode = "KOR", FlagPath = "/flags/kor.svg", OppCode = 202 },
            ["Czechia"] = new() { Name = "Czechia", IsoCode = "CZE", FlagPath = "/flags/cze.svg", OppCode = 53 },
            // Group B
            ["Switzerland"] = new() { Name = "Switzerland", IsoCode = "SUI", FlagPath = "/flags/sui.svg", OppCode = 210 },
            ["Canada"] = new() { Name = "Canada", IsoCode = "CAN", FlagPath = "/flags/can.svg", OppCode = 36 },
            ["Bosnia and Herzegovina"] = new() { Name = "Bosnia and Herzegovina", IsoCode = "BIH", FlagPath = "/flags/bih.svg", OppCode = 26 },
            ["Qatar"] = new() { Name = "Qatar", IsoCode = "QAT", FlagPath = "/flags/qat.svg", OppCode = 174 },
            // Group C
            ["Brazil"] = new() { Name = "Brazil", IsoCode = "BRA", FlagPath = "/flags/bra.svg", OppCode = 28 },
            ["Morocco"] = new() { Name = "Morocco", IsoCode = "MAR", FlagPath = "/flags/mar.svg", OppCode = 142 },
            ["Scotland"] = new() { Name = "Scotland", IsoCode = "SCO", FlagPath = "/flags/sco.svg", OppCode = 189 },
            ["Haiti"] = new() { Name = "Haiti", IsoCode = "HAI", FlagPath = "/flags/hai.svg", OppCode = 92 },
            // Group D
            ["United States"] = new() { Name = "United States", IsoCode = "USA", FlagPath = "/flags/usa.svg", OppCode = 232 },
            ["Australia"] = new() { Name = "Australia", IsoCode = "AUS", FlagPath = "/flags/aus.svg", OppCode = 11 },
            ["Paraguay"] = new() { Name = "Paraguay", IsoCode = "PAR", FlagPath = "/flags/par.svg", OppCode = 168 },
            ["Turkey"] = new() { Name = "Turkey", IsoCode = "TUR", FlagPath = "/flags/tur.svg", OppCode = 224 },
            // Group E
            ["Germany"] = new() { Name = "Germany", IsoCode = "GER", FlagPath = "/flags/ger.svg", OppCode = 80 },
            ["Ivory Coast"] = new() { Name = "Ivory Coast", IsoCode = "CIV", FlagPath = "/flags/civ.svg", OppCode = 104 },
            ["Ecuador"] = new() { Name = "Ecuador", IsoCode = "ECU", FlagPath = "/flags/ecu.svg", OppCode = 61 },
            ["Curaçao"] = new() { Name = "Curaçao", IsoCode = "CUW", FlagPath = "/flags/cuw.svg", OppCode = 51 },
            // Group F
            ["Netherlands"] = new() { Name = "Netherlands", IsoCode = "NED", FlagPath = "/flags/ned.svg", OppCode = 147 },
            ["Japan"] = new() { Name = "Japan", IsoCode = "JPN", FlagPath = "/flags/jpn.svg", OppCode = 106 },
            ["Sweden"] = new() { Name = "Sweden", IsoCode = "SWE", FlagPath = "/flags/swe.svg", OppCode = 209 },
            ["Tunisia"] = new() { Name = "Tunisia", IsoCode = "TUN", FlagPath = "/flags/tun.svg", OppCode = 223 },
            // Group G
            ["Belgium"] = new() { Name = "Belgium", IsoCode = "BEL", FlagPath = "/flags/bel.svg", OppCode = 19 },
            ["Egypt"] = new() { Name = "Egypt", IsoCode = "EGY", FlagPath = "/flags/egy.svg", OppCode = 62 },
            ["Iran"] = new() { Name = "Iran", IsoCode = "IRN", FlagPath = "/flags/irn.svg", OppCode = 99 },
            ["New Zealand"] = new() { Name = "New Zealand", IsoCode = "NZL", FlagPath = "/flags/nzl.svg", OppCode = 149 },
            // Group H
            ["Spain"] = new() { Name = "Spain", IsoCode = "ESP", FlagPath = "/flags/esp.svg", OppCode = 204 },
            ["Cape Verde"] = new() { Name = "Cape Verde", IsoCode = "CPV", FlagPath = "/flags/cpv.svg", OppCode = 37 },
            ["Uruguay"] = new() { Name = "Uruguay", IsoCode = "URU", FlagPath = "/flags/uru.svg", OppCode = 233 },
            ["Saudi Arabia"] = new() { Name = "Saudi Arabia", IsoCode = "KSA", FlagPath = "/flags/ksa.svg", OppCode = 188 },
            // Group I
            ["France"] = new() { Name = "France", IsoCode = "FRA", FlagPath = "/flags/fra.svg", OppCode = 75 },
            ["Norway"] = new() { Name = "Norway", IsoCode = "NOR", FlagPath = "/flags/nor.svg", OppCode = 161 },
            ["Senegal"] = new() { Name = "Senegal", IsoCode = "SEN", FlagPath = "/flags/sen.svg", OppCode = 190 },
            ["Iraq"] = new() { Name = "Iraq", IsoCode = "IRQ", FlagPath = "/flags/irq.svg", OppCode = 100 },
            // Group J
            ["Argentina"] = new() { Name = "Argentina", IsoCode = "ARG", FlagPath = "/flags/arg.svg", OppCode = 8 },
            ["Austria"] = new() { Name = "Austria", IsoCode = "AUT", FlagPath = "/flags/aut.svg", OppCode = 12 },
            ["Algeria"] = new() { Name = "Algeria", IsoCode = "ALG", FlagPath = "/flags/alg.svg", OppCode = 2 },
            ["Jordan"] = new() { Name = "Jordan", IsoCode = "JOR", FlagPath = "/flags/jor.svg", OppCode = 107 },
            // Group K
            ["Colombia"] = new() { Name = "Colombia", IsoCode = "COL", FlagPath = "/flags/col.svg", OppCode = 44 },
            ["Portugal"] = new() { Name = "Portugal", IsoCode = "POR", FlagPath = "/flags/por.svg", OppCode = 172 },
            ["DR Congo"] = new() { Name = "DR Congo", IsoCode = "COD", FlagPath = "/flags/cod.svg", OppCode = 54 },
            ["Uzbekistan"] = new() { Name = "Uzbekistan", IsoCode = "UZB", FlagPath = "/flags/uzb.svg", OppCode = 234 },
            // Group L
            ["England"] = new() { Name = "England", IsoCode = "ENG", FlagPath = "/flags/eng.svg", OppCode = 64 },
            ["Croatia"] = new() { Name = "Croatia", IsoCode = "CRO", FlagPath = "/flags/cro.svg", OppCode = 49 },
            ["Ghana"] = new() { Name = "Ghana", IsoCode = "GHA", FlagPath = "/flags/gha.svg", OppCode = 81 },
            ["Panama"] = new() { Name = "Panama", IsoCode = "PAN", FlagPath = "/flags/pan.svg", OppCode = 166 },
        };

        db.Countries.AddRange(countries.Values);
        db.SaveChanges();

        var groupDefs = new Dictionary<string, string[]>
        {
            ["A"] = ["Mexico", "South Africa", "South Korea", "Czechia"],
            ["B"] = ["Switzerland", "Canada", "Bosnia and Herzegovina", "Qatar"],
            ["C"] = ["Brazil", "Morocco", "Scotland", "Haiti"],
            ["D"] = ["United States", "Australia", "Paraguay", "Turkey"],
            ["E"] = ["Germany", "Ivory Coast", "Ecuador", "Curaçao"],
            ["F"] = ["Netherlands", "Japan", "Sweden", "Tunisia"],
            ["G"] = ["Belgium", "Egypt", "Iran", "New Zealand"],
            ["H"] = ["Spain", "Cape Verde", "Uruguay", "Saudi Arabia"],
            ["I"] = ["France", "Norway", "Senegal", "Iraq"],
            ["J"] = ["Argentina", "Austria", "Algeria", "Jordan"],
            ["K"] = ["Colombia", "Portugal", "DR Congo", "Uzbekistan"],
            ["L"] = ["England", "Croatia", "Ghana", "Panama"],
        };

        foreach (var (groupName, teamNames) in groupDefs)
        {
            var group = new Group { TournamentId = tournament.Id, Name = groupName };
            db.Groups.Add(group);
            db.SaveChanges();
            foreach (var teamName in teamNames)
            {
                db.GroupTeams.Add(new GroupTeam
                {
                    GroupId = group.Id,
                    CountryId = countries[teamName].Id
                });
            }
        }

        db.SaveChanges();
    }
}