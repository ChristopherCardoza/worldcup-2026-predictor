using System;

namespace WorldCupPredictor.API.Models
{
    /// <summary>
    /// Represents a fixture in the tournament
    /// </summary>
    public class Fixture
    {
        public int Id { get; set; }
        public int TournamentId { get; set; }
        public int? GroupId { get; set; }
        public int? Matchday { get; set; } 
        public string Stage { get; set; } = "Group"; // Can be Group, Round of 32, Qrt final, etc
        public int TeamAId { get; set; }
        public int TeamBId { get; set; }
        public string Venue { get; set; } = "Neutral";
        public int? BracketSlot { get; set; }
        public Tournament Tournament { get; set; } = null!;
        public Group? Group { get; set; }
        public Country TeamA { get; set; } = null!;
        public Country TeamB { get; set; } = null!;
    }

}


