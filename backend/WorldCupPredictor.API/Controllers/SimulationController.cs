using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WorldCupPredictor.API.Data;
using WorldCupPredictor.API.Models;

namespace WorldCupPredictor.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SimulationController : ControllerBase
    {
        private readonly AppDbContext _db;

        public SimulationController(AppDbContext db) => _db = db;

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



    }
}
