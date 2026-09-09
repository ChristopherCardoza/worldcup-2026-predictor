using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WorldCupPredictor.API.Data;
using WorldCupPredictor.API.Models;
using WorldCupPredictor.API.Services;
using WorldCupPredictor.API.Services.Ml;

namespace WorldCupPredictor.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SimulationController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly MlPredictionClient _ml;


        public SimulationController(AppDbContext db, MlPredictionClient ml)
        {
            _db = db;
            _ml = ml;
        }

        /// <summary>
        /// Starts a new tournament simulation
        /// </summary>
        /// <returns>Id for the tournament simulation</returns>
        [HttpPost]
        public async Task<ActionResult<object>> Start()
        {
            var tournament = await _db.Tournaments
                .OrderByDescending(t => t.Year)
                .FirstOrDefaultAsync();

            if (tournament is null)
            {
                return NotFound("No tournament found. Seed the database");
            }

            var run = new SimulationRun
            {
                TournamentId = tournament.Id,
                CreatedAt = DateTime.UtcNow,
                Status = "InProgress"
            };

            _db.SimulationRuns.Add(run);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new {id = run.Id}, new
            {
                run.Id, run.TournamentId, run.CreatedAt, run.Status
            });

        }

        /// <summary>
        /// Gets a simulation run by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id:int}")]
        public async Task<ActionResult<object>> GetById(int id)
        {
            var run = await _db.SimulationRuns
                .AsNoTracking()
                .Where(r => r.Id == id)
                .Select(r => new
                {
                    r.Id,
                    r.TournamentId,
                    r.CreatedAt,
                    r.Status,
                    r.ChampionCountryId,
                    MatchesSimulated = r.SimulatedMatches.Count
                })
                .FirstOrDefaultAsync();

            if (run is null)
            {
                return NotFound();
            }

            return Ok(run);

        }


        
        [HttpPost("{id:int}/matchday/{matchday:int}")]
        public async Task<IActionResult> SimulateMatchday (int id, int matchday, CancellationToken ct)
        {
            if (matchday is < 1 or > 3)
            {
                return BadRequest("Matchday must be 1,2, or 3.");
            }

            var run = await _db.SimulationRuns.FindAsync([id], ct);
            if (run is null)
            {
                return NotFound($"SimulationRun {id} not found.");
            }


            var fixtures = await _db.Fixtures
                .Include(f => f.TeamA)
                .Include(f => f.TeamB)
                .Where(f => f.TournamentId == run.TournamentId && f.Stage == "Group" && f.Matchday == matchday)
                .ToListAsync(ct);

            if (fixtures.Count == 0)
            {
                return NotFound($"No fixtures for matchday {matchday}.");
            }

            // Skip already simulated for the run
            var alreadyDone = await _db.SimulatedMatches
                .Where(m => m.SimulationRunId == id)
                .Select(m => m.FixtureId)
                .ToListAsync(ct);

            var created = new List<object>();

            foreach (var fixture in fixtures)
            {
                if (alreadyDone.Contains(fixture.Id))
                {
                    continue;
                }

                var pA = await _ml.GetTeamWinProbabilityAsync(
                    fixture.TeamA.Name,
                    fixture.TeamB.Name,
                    fixture.Venue,
                    ct);

                var (outcome, goalsA, goalsB) = MatchSimulator.Simulate(pA);

                var match = new SimulatedMatch
                {
                    SimulationRunId = id,
                    FixtureId = fixture.Id,
                    TeamAWinProbability = pA,
                    GoalsA = goalsA,
                    GoalsB = goalsB,
                    Outcome = outcome
                };

                _db.SimulatedMatches.Add(match);

                created.Add(new
                {
                    fixture.Id,
                    TeamA = fixture.TeamA.Name,
                    TeamB = fixture.TeamB.Name,
                    TeamAWinProbability = pA,
                    GoalsA = goalsA,
                    GoalsB = goalsB,
                    Outcome = outcome
                });



            }

            if (matchday == 3)
            {
                run.Status = "GroupComplete";
            }

            await _db.SaveChangesAsync(ct);

            // standings built after database is updated
            if (matchday == 3)
            {
                await StandingsCalculator.RecalculateAsync(_db, id, ct);
            }

            return Ok(new
            {
                SimulationRunId = id,
                Matchday = matchday,
                MatchesSimulated = created.Count,
                Matches = created
            });

        }

        [HttpGet("{id:int}/standings")]
        public async Task<IActionResult> GetStandings(int id, CancellationToken ct)
        {
            var rows = await _db.GroupStandings
                .AsNoTracking()
                .Where(s => s.SimulationRunId == id)
                .Include(s => s.Group)
                .Include(s => s.Country)
                .OrderBy(s => s.Group.Name)
                .ThenBy(s => s.Rank)
                .Select(s => new
                {
                    Group = s.Group.Name,
                    Team = s.Country.Name,
                    s.Rank,
                    s.Played,
                    s.Won,
                    s.Drawn,
                    s.Lost,
                    s.GoalsFor,
                    s.GoalsAgainst,
                    GoalDifference = s.GoalsFor - s.GoalsAgainst,
                    s.Points
                })
                .ToListAsync(ct);

            if (rows.Count == 0)
            {
                return NotFound("No standings yet. Simulate matchdays 1–3 first.");
            }

            return Ok(rows);
        }

        [HttpPost("{id:int}/knockouts")]
        public async Task<IActionResult> SimulateKnockouts(int id, CancellationToken ct)
        {
            try
            {
                var result = await KnockoutSimulator.RunAsync(_db, _ml, id, ct);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id:int}/export")]
        public async Task<IActionResult> Export(int id, CancellationToken ct)
        {
            var run = await _db.SimulationRuns
                .Include(r => r.ChampionCountry)
                .FirstOrDefaultAsync(r => r.Id == id, ct);

            if (run is null)
            {
                return NotFound();
            }
                
           var standings = await _db.GroupStandings
                .Include(s => s.Group)
                .Include(s => s.Country)
                .Where(s => s.SimulationRunId == id)
                .OrderBy(s => s.Group.Name)
                .ThenBy(s => s.Rank)
                .ToListAsync(ct);

            var ko = await _db.SimulatedMatches
                .Include(m => m.Fixture).ThenInclude(f => f.TeamA)
                .Include(m => m.Fixture).ThenInclude(f => f.TeamB)
                .Where(m => m.SimulationRunId == id && m.Fixture.Stage != "Group")
                .OrderBy(m => m.Fixture.BracketSlot)
                .ToListAsync(ct);

            var html = new System.Text.StringBuilder();
            html.Append("<html><head><title>WC 2026 Sim</title></head><body>");
            html.Append($"<h1>World Cup 2026 Simulation {id}</h1>");
            html.Append($"<p>Champion: {run.ChampionCountry?.Name ?? "(not finished)"}</p>");

            string? currentGroup = null;
            foreach (var s in standings)
            {
                if (s.Group.Name != currentGroup)
                {
                    if (currentGroup is not null) html.Append("</table>");
                    currentGroup = s.Group.Name;
                    html.Append($"<h2>Group {s.Group.Name}</h2>");
                    html.Append("<table border='1' cellpadding='4'><tr><th>Pos</th><th>Team</th><th>P</th><th>W</th><th>D</th><th>L</th><th>GF</th><th>GA</th><th>Pts</th></tr>");
                }
                html.Append($"<tr><td>{s.Rank}</td><td>{s.Country.Name}</td><td>{s.Played}</td><td>{s.Won}</td><td>{s.Drawn}</td><td>{s.Lost}</td><td>{s.GoalsFor}</td><td>{s.GoalsAgainst}</td><td>{s.Points}</td></tr>");
            }
            if (currentGroup is not null) html.Append("</table>");

            html.Append("<h2>Knockouts</h2><table border='1' cellpadding='4'><tr><th>Stage</th><th>Match</th><th>Score</th></tr>");
            foreach (var m in ko)
            {
                html.Append($"<tr><td>{m.Fixture.Stage}</td><td>{m.Fixture.TeamA.Name} vs {m.Fixture.TeamB.Name}</td><td>{m.GoalsA}-{m.GoalsB}</td></tr>");
            }
            html.Append("</table></body></html>");

            return Content(html.ToString(), "text/html");
        }

    }
}
