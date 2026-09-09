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
                .ThenByDescending(S => S.GoalsFor - S.GoalsAgainst)
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
            Dictionary<string, string[]> allowed = new Dictionary<string, string[]>
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

            string[] winners = ["A", "B", "D", "E", "G", "I", "K", "L"];
            Dictionary<string, string>? solution = null;

            void Search(int index, HashSet<string> unused, Dictionary<string, string> current)
            {
                if (solution is not null)
                {
                    return;
                }

                if (index == winners.Length)
                {
                    solution = new Dictionary<string, string>(current);
                    return;
                }

                string winner = winners[index];
                foreach (string group in allowed[winner])
                {
                    if (!unused.Contains(group))
                    {
                        continue;
                    }

                    unused.Remove(group);
                    current[winner] = group;
                    Search(index + 1, unused, current);
                    current.Remove(winner);
                    unused.Add(group);
                }
            }

            Search(0, new HashSet<string>(advancingThirdGroups), new Dictionary<string, string>());

            if (solution is null)
            {
                throw new InvalidOperationException(
                    "Could not assign third-place teams. Check that standings have ranks 1-4 for every group.");
            }

            return solution;
        }

    }
}
