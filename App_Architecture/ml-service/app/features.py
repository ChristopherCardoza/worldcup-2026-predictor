# Same columns from Finalized_Model_Script.ipynb

FEATURE_COLUMNS = [
    "venue_code",
    "opp_code",
    "day_code",
    "h2h_win_rate",
    "gf_rolling",
    "ga_rolling",
]

# Pandas category codes for sorted venues:
# Away=0, Home=1, Neutral=2
VENUE_CODES = {
    "Away": 0,
    "Home": 1,
    "Neutral": 2,
}

def build_matchup(team: str, opponent: str) -> str:
    return "-".join(sorted([team, opponent]))

def build_feature_row(
    team: str,
    opponent: str,
    venue: str,
    day_code: int,
    team_mapping: dict,
    latest_form: dict,
    h2h_lookup: dict,
) -> list:
    if venue not in VENUE_CODES:
        raise ValueError(f"venue must be one of {list(VENUE_CODES.keys())}")

    if opponent not in team_mapping:
        raise ValueError(f"Unknown opponent: {opponent}")

    if team not in latest_form:
        raise ValueError(f"Unknown team: {team}")

    matchup = build_matchup(team, opponent)
    h2h = h2h_lookup.get(matchup, 0.5)  # default .5 when there is no match history

    form = latest_form[team]

    return [[
        VENUE_CODES[venue],
        team_mapping[opponent],
        day_code,
        h2h,
        form["gf_rolling"],
        form["ga_rolling"],
    ]]