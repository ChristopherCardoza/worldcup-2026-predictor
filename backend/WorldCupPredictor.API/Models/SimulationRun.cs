namespace WorldCupPredictor.API.Models
{
    public class SimulationRun
    {
        public int Id { get; set; }
        public int TournamentId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string Status { get; set; } = "InProgress"; // going to be GroupComplete, etc
        public int? ChampionCountryId { get; set; }
        public Tournament Tournament { get; set; } = null!;
        public Country? ChampionCountry { get; set; }
        public List<SimulatedMatch> SimulatedMatches { get; set; } = [];
    }
}
