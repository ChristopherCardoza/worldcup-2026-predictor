from pathlib import Path
import os
import joblib
from huggingface_hub import hf_hub_download

MODEL_DIR = Path(os.getenv("MODEL_DIR", "model"))
MODEL_FILENAME = os.getenv("HF_FILENAME", "world_cup_model.joblib")
MODEL_PATH = MODEL_DIR / MODEL_FILENAME

def ensure_model_downloaded() -> Path:
    MODEL_DIR.mkdir(parents=True, exist_ok=True)

    if MODEL_PATH.exists():
        return MODEL_PATH

     downloaded = hf_hub_download(
        repo_id=os.getenv("HF_REPO_ID", "ChristopherCardoza/world-cup-simulator"),
        filename=MODEL_FILENAME,
        revision=os.getenv("HF_REVISION", "main"),
        local_dir=str(MODEL_DIR),
    )
    return Path(downloaded)

def load_model():
    path = ensure_model_downloaded()
    return joblib.load(path)