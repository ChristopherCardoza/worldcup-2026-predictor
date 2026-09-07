namespace WorldCupPredictor.API.Services.Ml
{
    public class MlPredictRequest
    {
        public string team { get; set; } = "";
        public string opponent { get; set; } = "";
        public string venue { get; set; } = "Neutral";
        public string? match_date { get; set; }
    }

    public class MlPredictResponse
    {
        public string team { get; set; } = "";
        public string opponent { get; set; } = "";
        public string venue { get; set; } = "";
        public double win_probability { get; set; }
        public bool predicted_win { get; set; }
    }
}
