using Microsoft.EntityFrameworkCore;
using WorldCupPredictor.API.Data;
using WorldCupPredictor.API.Models;
using WorldCupPredictor.API.Services.Ml;

namespace WorldCupPredictor.API.Services
{
    public class KnockoutSimulator
    {
        public static async Task<object> RunAsync(
            AppDbContext db,
            MlPredictionClient ml,
            int runId,
            CancellationToken ct)
        {
            SimulationRun? run = await db.SimulationRuns.FindAsync([runId], ct);
            if (run is null)
            {
                throw new InvalidOperationException($"SimulationRun {runId} not found.");
            }

            if (run.Status != "GroupComplete" && run.Status != "Complete")
            {
                throw new InvalidOperationException("Finish matchdays 1-3 first.");
            }

            List<GroupStanding> standings = await db.GroupStandings
                .Include(s => s.Group)
                .Where(s => s.SimulationRunId == runId)
                .ToListAsync(ct);

            if (standings.Count == 0)
            {
                throw new InvalidOperationException("No standings. Simulate matchday 3 first.");
            }

            List<SimulatedMatch> already = await db.SimulatedMatches
                .Include(m => m.Fixture)
                .Where(m => m.SimulationRunId == runId && m.Fixture.Stage != "Group")
                .ToListAsync(ct);

            if (already.Count > 0)
            {
                throw new InvalidOperationException("Knockouts already simulated for this run. Start a new simulation.");
            }

            List<KnockoutBracket.R32Match> r32 = KnockoutBracket.BuildRoundOf32(standings);
            Dictionary<int, int> winners = new Dictionary<int, int>();
            List<object> created = new List<object>();

            foreach (KnockoutBracket.R32Match match in r32)
            {
                int winnerId = await SimulateOneAsync(db, ml, run, "RoundOf32", match.Slot, match.TeamAId, match.TeamBId, created, ct);
                winners[match.Slot] = winnerId;
            }

            int[][] r16Pairs =
            [
                [73, 74], [75, 76], [77, 78], [79, 80],
                [81, 82], [83, 84], [85, 86], [87, 88]
            ];

            int[] r16Slots = [89, 90, 91, 92, 93, 94, 95, 96];

            for (int i = 0; i < r16Pairs.Length; i++)
            {
                winners[r16Slots[i]] = await SimulateOneAsync(db, ml, run, "RoundOf16", r16Slots[i], winners[r16Pairs[i][0]], winners[r16Pairs[i][1]], created, ct);
            }

            int[][] qfPairs = [[89, 90], [91, 92], [93, 94], [95, 96]];
            int[] qfSlots = [97, 98, 99, 100];

            for (int i = 0; i < qfPairs.Length; i++)
            {
                winners[qfSlots[i]] = await SimulateOneAsync(db, ml, run, "QuarterFinal", qfSlots[i], winners[qfPairs[i][0]], winners[qfPairs[i][1]], created, ct);
            }

            int sf1 = await SimulateOneAsync(db, ml, run, "SemiFinal", 101, winners[97], winners[98], created, ct);
            int sf2 = await SimulateOneAsync(db, ml, run, "SemiFinal", 102, winners[99], winners[100], created, ct);
            winners[101] = sf1;
            winners[102] = sf2;
            int loser101 = sf1 == winners[97] ? winners[98] : winners[97];
            int loser102 = sf2 == winners[99] ? winners[100] : winners[99];

            await SimulateOneAsync(db, ml, run, "ThirdPlace", 103, loser101, loser102, created, ct);
            int championId = await SimulateOneAsync(db, ml, run, "Final", 104, sf1, sf2, created, ct);
            run.ChampionCountryId = championId;
            run.Status = "Complete";
            await db.SaveChangesAsync(ct);

            Country? champion = await db.Countries.FindAsync([championId], ct);
            return new
            {
                SimulationRunId = runId,
                Status = run.Status,
                Champion = champion?.Name,
                MatchesSimulated = created.Count,
                Matches = created
            };

        }

        private static async Task<int> SimulateOneAsync(
            AppDbContext db,
            MlPredictionClient ml,
            SimulationRun run,
            string stage,
            int slot,
            int teamAId,
            int teamBId,
            List<object> created,
            CancellationToken ct)
        {
            Country teamA = await db.Countries.FindAsync([teamAId], ct) ?? throw new InvalidOperationException($"Team {teamAId} not found.");
            Country teamB = await db.Countries.FindAsync([teamBId], ct) ?? throw new InvalidOperationException($"Team {teamBId} not found.");
            Fixture fixture = new Fixture
            {
                TournamentId = run.TournamentId,
                Stage = stage,
                BracketSlot = slot,
                TeamAId = teamAId,
                TeamBId = teamBId,
                Venue = "Neutral"
            };

            db.Fixtures.Add(fixture);
            await db.SaveChangesAsync(ct);

            double pA = await ml.GetTeamWinProbabilityAsync(teamA.Name, teamB.Name, "Neutral", ct);
            (string Outcome, int GoalsA, int GoalsB) result = MatchSimulator.SimulateKnockout(pA);

            SimulatedMatch simulatedMatch = new SimulatedMatch
            {
                SimulationRunId = run.Id,
                FixtureId = fixture.Id,
                TeamAWinProbability = pA,
                GoalsA = result.GoalsA,
                GoalsB = result.GoalsB,
                Outcome = result.Outcome
            };

            db.SimulatedMatches.Add(simulatedMatch);
            await db.SaveChangesAsync(ct);

            created.Add(new
            {
                Stage = stage,
                Slot = slot,
                TeamA = teamA.Name,
                TeamB = teamB.Name,
                GoalsA = result.GoalsA,
                GoalsB = result.GoalsB,
                Outcome = result.Outcome
            });

            return result.Outcome == "TeamAWin" ? teamAId : teamBId;
        }



    }
}
