namespace WorldCupPredictor.API.Services.Ml
{
    public class MlPredictionClient
    {
        private readonly HttpClient _http;

        public MlPredictionClient(HttpClient http) => _http = http;

        public async Task<double> GetTeamWinProbabilityAsync(
            string teamA,
            string teamB,
            string venue = "Neutral",
            CancellationToken ct = default)
        {
            var body = new MlPredictRequest
            {
                team = teamA,
                opponent = teamB,
                venue = venue,
                match_date = "2026-06-11"
            };

            var response = await _http.PostAsJsonAsync("/predict", body, ct);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<MlPredictResponse>(cancellationToken: ct)
                ?? throw new InvalidOperationException("Empty ML response");

            return result.win_probability;
        }

    }
}
