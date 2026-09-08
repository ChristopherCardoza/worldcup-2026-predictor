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



    }
}
