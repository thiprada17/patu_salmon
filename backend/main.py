from fastapi import FastAPI

from routers.LoginRouter import router as login_router


app = FastAPI(
    title="Platu Salmon API",
    version="1.0.0"
)


app.include_router(login_router)


@app.get("/")
def root():
    return {
        "message": "Platu Salmon Backend is running"
    }