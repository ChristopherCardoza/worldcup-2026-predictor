namespace WorldCupPredictor.API.Models
{
    public class GroupStanding
    {
        public int SimulationRunId { get; set; }
        public int GroupId { get; set; }
        public int CountryId { get; set; }
        public int Played { get; set; }
        public int Won { get; set; }
        public int Drawn { get; set; }
        public int Lost { get; set; }
        public int GoalsFor { get; set; }
        public int GoalsAgainst { get; set; }
        public int Points { get; set; }
        public int Rank { get; set; }
        public SimulationRun SimulationRun { get; set; } = null!;
        public Group Group { get; set; } = null!;
        public Country Country { get; set; } = null!;
    }
}
