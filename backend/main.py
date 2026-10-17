from fastapi import FastAPI

from api.v1.AuthRouter import router as auth_router
from api.v1.PlayerRouter import router as player_router

app = FastAPI(title="Platu Salmon หลังบ้าน")

app.include_router(auth_router)
app.include_router(player_router)


@app.get("/")
def health_check():
    return {"status": "ok"}
