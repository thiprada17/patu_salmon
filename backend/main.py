from fastapi import FastAPI

from routers.auth import router as login_router


app = FastAPI(
    title="Platu Salmon API"
)


app.include_router(
    login_router
)


@app.get("/")
def root():

    return {
        "message": "Platu Salmon API is running"
    }


if __name__ == "__main__":

    import uvicorn

    uvicorn.run(
        "test:app",
        host="0.0.0.0",
        port=8000,
        reload=True
    )