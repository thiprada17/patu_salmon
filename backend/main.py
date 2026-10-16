from fastapi import FastAPI

from api.v1.AuthRouter import router as auth_router
from api.v1.PlayerRouter import router as player_router
from fastapi.middleware.cors import CORSMiddleware
app = FastAPI(title="Platu Salmon หลังบ้าน")

app.include_router(auth_router)
app.include_router(player_router)
app.add_middleware(
    CORSMiddleware,
    allow_origins=[
        "https://thiprada17.itch.io/platu-salmon",
        "http://localhost:3000",
        "http://127.0.0.1:8000",
    ],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

@app.get("/")
def health_check():
    return {"status": "ok"}
