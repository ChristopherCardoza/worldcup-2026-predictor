namespace WorldCupPredictor.API.Services
{
    public class MatchSimulator
    {
        // For group stage
        private const double DrawRate = 0.25;

        public static (string Outcome, int GoalsA, int GoalsB) Simulate(
            double teamAWinProbability)
        {
            // Leftover probability after draw
            var pDraw = DrawRate;
            var pA = teamAWinProbability * (1.0 - pDraw);
            var pB = (1.0 - teamAWinProbability) * (1.0 - pDraw);

            var r = Random.Shared.NextDouble();

            if (r < pA)
            {
                var (ga, gb) = RandomScore(winnerIsA: true, isDraw: false);
                return ("TeamAWin", ga, gb);
            }
            else if (r < pA + pDraw)
            {
                var (ga, gb) = RandomScore(winnerIsA: false, isDraw: true);
                return ("Draw", ga, gb);
            }
            else
            {
                var (ga, gb) = RandomScore(winnerIsA: false, isDraw: false);
                return ("TeamBWin", ga, gb);
            }
        }

        private static (int GoalsA, int GoalsB) RandomScore(bool winnerIsA, bool isDraw)
        {
            if (isDraw)
            {
                var g = Random.Shared.Next(0, 3); // produces results like 0-0, 1-1, 2-2
                return (g, g);
            }

            var winnerGoals = Random.Shared.Next(1, 4);
            var loserGoals = Random.Shared.Next(0, winnerGoals);

            return winnerIsA ? (winnerGoals, loserGoals) : (loserGoals, winnerGoals);
        }

        public static (string Outcome, int GoalsA, int GoalsB) SimulateKnockout(
            double teamAWinProbability)
        {
            var r = Random.Shared.NextDouble();
            bool aWins = r < teamAWinProbability;
            var (ga, gb) = RandomScore(winnerIsA: aWins, isDraw: false);
            return (aWins ? "TeamAWin" : "TeamBWin", ga, gb);
        }

    }
}
