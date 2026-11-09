from pathlib import Path

from fastapi import FastAPI
from fastapi.staticfiles import StaticFiles

from api.v1.AuthRouter import login_with_apple, router as auth_router
from api.v1.PlayerRouter import router as player_router
from api.v1.UserRouter import delete_account, router as user_router
from fastapi.middleware.cors import CORSMiddleware
app = FastAPI(title="Platu Salmon หลังบ้าน")

LEGAL_PAGES_DIR = Path(__file__).resolve().parent / "static" / "legal"
app.mount("/legal", StaticFiles(directory=LEGAL_PAGES_DIR, html=True), name="legal")

app.include_router(auth_router)
app.include_router(player_router)
app.include_router(user_router)
# Versioned Apple endpoint requested by clients. Keep /auth/apple above as a
# backwards-compatible route for existing Unity builds.
app.add_api_route(
    "/api/v1/auth/apple",
    login_with_apple,
    methods=["POST"],
    tags=["Authentication"],
)
app.add_api_route(
    "/api/v1/user/account",
    delete_account,
    methods=["DELETE"],
    tags=["User"],
)

app.add_middleware(
    CORSMiddleware,
    allow_origins=[
        "https://thiprada17.itch.io",
        "https://html-classic.itch.zone",
    ],
    allow_origin_regex=r"https?://(localhost|127\.0\.0\.1)(:\d+)?",
    allow_methods=["*"],
    allow_headers=["*"],
)

@app.get("/")
def health_check():
    return {"status": "ok"}
