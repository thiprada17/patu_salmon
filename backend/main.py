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
    ],
    allow_methods=["*"],
    allow_headers=["*"],
)

@app.get("/")
def health_check():
    return {"status": "ok"}
