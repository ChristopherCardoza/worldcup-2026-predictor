using Microsoft.EntityFrameworkCore;
using WorldCupPredictor.API.Models;

namespace WorldCupPredictor.API.Data
{
    /// <summary>
    /// Entity Framework Core database context for the World Cup Predictor application.
    /// Provides access to SQLite tables and configures entity relationships, keys, and indexes.
    /// </summary>
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<Country> Countries => Set<Country>();
        public DbSet<Tournament> Tournaments => Set<Tournament>();
        public DbSet<Group> Groups => Set<Group>();
        public DbSet<GroupTeam> GroupTeams => Set<GroupTeam>();
        public DbSet<Fixture> Fixtures => Set<Fixture>();
        public DbSet<SimulationRun> SimulationRuns => Set<SimulationRun>();
        public DbSet<SimulatedMatch> SimulatedMatches => Set<SimulatedMatch>();
        public DbSet<GroupStanding> GroupStandings => Set<GroupStanding>();

        /// <summary>
        /// Configures entity keys, composite keys, unique indexes, and other database constraints.
        /// </summary>
        /// <param name="modelBuilder">The builder used to construct the EF Core model.</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Composite primary key: one country can appear once per group.
            modelBuilder.Entity<GroupTeam>()
                .HasKey(gt => new { gt.GroupId, gt.CountryId });
            // Composite primary key: one standing row per team per group per simulation
            modelBuilder.Entity<GroupStanding>()
                .HasKey(gs => new { gs.SimulationRunId, gs.GroupId, gs.CountryId });
            // Each fixture may only be simulated once per simulation run.
            modelBuilder.Entity<SimulatedMatch>()
                .HasIndex(sm => new { sm.SimulationRunId, sm.FixtureId })
                .IsUnique();
            // Country names must be unique and match ML service CSV values exactly.
            modelBuilder.Entity<Country>()
                .HasIndex(c => c.Name)
                .IsUnique();
        }


    }
}
