using WorldCupPredictor.API.Models;
using WorldCupPredictor.API.Data;
using Microsoft.EntityFrameworkCore;
namespace WorldCupPredictor.API.Services
{
    public static class StandingsCalculator
    {
        public static async Task RecalculateAsync(AppDbContext db, int runId, CancellationToken ct = default)
        {
            var run = await db.SimulationRuns.FindAsync([runId], ct) ?? throw new InvalidOperationException($"SimulationRun {runId} not found.");

            var matches = await db.SimulatedMatches
                .Include(m => m.Fixture)
                .Where(m => m.SimulationRunId == runId && m.Fixture.Stage == "Group")
                .ToListAsync(ct);

            var groupTeams = await db.GroupTeams
                .Include(gt => gt.Group)
                .Where(gt => gt.Group.TournamentId == run.TournamentId)
                .ToListAsync(ct);

            var rows = groupTeams.Select(gt => new GroupStanding
            {
                SimulationRunId = runId,
                GroupId = gt.GroupId,
                CountryId = gt.CountryId
            }).ToDictionary(s => (s.GroupId, s.CountryId));

            foreach (var m in matches)
            {
                var f = m.Fixture;
                if (f.GroupId is null)
                {
                    continue;
                }

                Apply(rows[(f.GroupId.Value, f.TeamAId)], m.GoalsA, m.GoalsB);
                Apply(rows[(f.GroupId.Value, f.TeamBId)], m.GoalsB, m.GoalsA);

            }

            foreach (var group in rows.Values.GroupBy(s => s.GroupId))
            {
                var ranked = group
                    .OrderByDescending(s => s.Points)
                    .ThenByDescending(s => s.GoalsFor - s.GoalsAgainst)
                    .ThenByDescending(s => s.GoalsFor)
                    .ToList();

                for (int i = 0; i < ranked.Count; i++)
                {
                    ranked[i].Rank = i + 1;
                }

                
            }

            var existing = db.GroupStandings.Where(s => s.SimulationRunId == runId);
            db.GroupStandings.RemoveRange(existing);
            db.GroupStandings.AddRange(rows.Values);
            await db.SaveChangesAsync(ct);



        }

        private static void Apply(GroupStanding row, int gf, int ga)
        {
            row.Played++;
            row.GoalsFor += gf;
            row.GoalsAgainst += ga;
            if (gf > ga)
            {
                row.Won++; row.Points += 3; 
            }
            else if (gf == ga)
            {
                row.Drawn++;
                row.Points += 1;
            }
            else
            {
                row.Lost++;
            }
        }
    }
}
