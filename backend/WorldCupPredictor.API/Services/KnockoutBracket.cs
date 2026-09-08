using WorldCupPredictor.API.Models;

namespace WorldCupPredictor.API.Services
{
    public static class KnockoutBracket
    {
        public record R32Match(int Slot, int TeamAId, int TeamBId);

        public static List<R32Match> BuildRoundOf32(List<GroupStanding> standings)
        {
            var byGroup = standings
                .GroupBy(s => s.Group.Name)
                .ToDictionary(
                    g => g.Key,
                    g => g.ToDictionary(s => s.Rank, s => s.CountryId));

            int First(string g) => byGroup[g][1];
            int Second(string g) => byGroup[g][2];

            var bestThirds = standings
                .Where(s => s.Rank == 3)
                .OrderByDescending(s => s.Points)
                .ThenByDescending(S => S.GoalsFor = S.GoalsAgainst)
                .ThenByDescending(S => S.GoalsFor)
                .Take(8)
                .ToDictionary(s => s.Group.Name, s => s.CountryId);

            var thirdForWinner = AssignThirds(bestThirds.Keys.ToHashSet());

            return [
                new(73, Second("A"), Second("B")),
                new(74, First("E"), bestThirds[thirdForWinner["E"]]),
                new(75, First("F"), Second("C")),
                new(76, First("C"), Second("F")),
                new(77, First("I"), bestThirds[thirdForWinner["I"]]),
                new(78, Second("E"), Second("I")),
                new(79, First("A"), bestThirds[thirdForWinner["A"]]),
                new(80, First("L"), bestThirds[thirdForWinner["L"]]),
                new(81, First("D"), bestThirds[thirdForWinner["D"]]),
                new(82, First("G"), bestThirds[thirdForWinner["G"]]),
                new(83, Second("K"), Second("L")),
                new(84, First("H"), Second("J")),
                new(85, First("B"), bestThirds[thirdForWinner["B"]]),
                new(86, First("J"), Second("H")),
                new(87, First("K"), bestThirds[thirdForWinner["K"]]),
                new(88, Second("D"), Second("G")),
                ];
        }

        // Winner group letter to which 3rd place group to play
        private static Dictionary<string, string> AssignThirds(HashSet<string> advancingThirdGroups)
        {
            var allowed = new Dictionary<string, string[]>
            {
                ["A"] = ["C", "E", "F", "H", "I"],
                ["B"] = ["E", "F", "G", "I", "J"],
                ["D"] = ["B", "E", "F", "I", "J"],
                ["E"] = ["A", "B", "C", "D", "F"],
                ["G"] = ["A", "E", "H", "I", "J"],
                ["I"] = ["C", "D", "F", "G", "H"],
                ["K"] = ["D", "E", "I", "J", "L"],
                ["L"] = ["E", "H", "I", "J", "K"],
            };

            var unused = new HashSet<string>(advancingThirdGroups);
            var result = new Dictionary<string, string>();

            foreach (var winner in allowed.Keys)
            {
                var pick = allowed[winner].First(g => unused.Contains(g));
                result[winner] = pick;
                unused.Remove(pick);
            }

            return result;
        }

    }
}
