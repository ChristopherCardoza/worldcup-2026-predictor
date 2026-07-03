# Implement FastAPI for model
from datetime import datetime
from pathlib import Path
import os

import pandas as pd
from dotenv import load_dotenv
from fastapi import FastAPI, HTTPException
from pydantic import BaseModel, Field

from app.features import FEATURE_COLUMNS, build_feature_row
from app.model_loader import load_model



load_dotenv()

DATA_DIR = Path(os.getenv("DATA_DIR", "data"))

app = FastAPI(title="World Cup 2026 Predictor API")

# Loading these once at startup 
model = load_model()

team_mapping_df = pd.read_csv(DATA_DIR / "team_mapping.csv")
latest_form_df = pd.read_csv(DATA_DIR / "latest_form.csv")
h2h_lookup_df = pd.read_csv(DATA_DIR / "h2h_lookup.csv")

team_mapping = dict(zip(team_mapping_df["opponent"], team_mapping_df["opp_code"]))
latest_form = latest_form_df.set_index("team")[["gf_rolling", "ga_rolling"]].to_dict("index")
h2h_lookup = dict(zip(h2h_lookup_df["matchup"], h2h_lookup_df["h2h_win_rate"]))


class PredictRequest(BaseModel):
    team: str = Field(..., examples=["Argentina"])
    opponent: str = Field(..., examples=["Brazil"])
    venue: str = Field(..., examples=["Neutral"])  # Home | Away | Neutral
    match_date: str | None = Field(
        default=None,
        examples=["2026-06-15"],
        description="ISO date; day_of_week becomes day_code. Defaults to today.",
    )


class PredictResponse(BaseModel):
    team: str
    opponent: str
    venue: str
    win_probability: float
    predicted_win: bool

#endpoint for if the server is up
@app.get("/health")
def health():
    return {"status": "ok"}


@app.post("/predict", response_model=PredictResponse)
def predict(body: PredictRequest):
    try:
        if body.match_date:
            day_code = datetime.fromisoformat(body.match_date).weekday()
        else:
            day_code = datetime.utcnow().weekday()

        features = build_feature_row(
            team=body.team,
            opponent=body.opponent,
            venue=body.venue,
            day_code=day_code,
            team_mapping=team_mapping,
            latest_form=latest_form,
            h2h_lookup=h2h_lookup,
        )

        proba = model.predict_proba(features)[0][1]  # class 1 = win
        predicted_win = proba >= 0.5

        return PredictResponse(
            team=body.team,
            opponent=body.opponent,
            venue=body.venue,
            win_probability=round(float(proba), 4),
            predicted_win=predicted_win,
        )

    except ValueError as exc:
        raise HTTPException(status_code=400, detail=str(exc)) from exc