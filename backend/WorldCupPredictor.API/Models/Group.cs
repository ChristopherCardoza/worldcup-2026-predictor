using System;

namespace WorldCupPredictor.API.Models
{
    /// <summary>
    /// Represents a group in the World Cup
    /// </summary>
    public class Group
    {
        public int Id { get; set; }
        public int TournamentId { get; set; }
        public string Name { get; set; } = "";
        public Tournament tournament { get; set; } = null!;
        public List<GroupTeam> GroupTeams { get; set; } = [];

    }

}

