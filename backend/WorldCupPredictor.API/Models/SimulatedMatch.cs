namespace WorldCupPredictor.API.Models
{
    public class SimulatedMatch
    {
        public int Id { get; set; }
        public int SimulationRunId { get; set; }
        public int FixtureId { get; set; }
        public double TeamAWinProbability { get; set; }
        public int GoalsA { get; set; }
        public int GoalsB { get; set; }
        public string Outcome { get; set; } = ""; // TeamAWin, Draw, TeamBWin
        public SimulationRun SimulationRun { get; set; } = null!;
        public Fixture Fixture { get; set; } = null!;
    }
}
