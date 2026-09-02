namespace WorldCupPredictor.API.Models
{
    /// <summary>
    /// Represents a country in a group
    /// </summary>
    public class GroupTeam
    {
        public int GroupId { get; set; }
        public int CountryId { get; set; }
        public Group Group { get; set; } = null!;
        public Country Country { get; set; } = null!;
    }
}
