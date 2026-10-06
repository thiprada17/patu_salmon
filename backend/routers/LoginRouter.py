from fastapi import APIRouter, Header, HTTPException

from controllers.LoginController import (
    google_login,
    google_callback,
    get_current_user
)


router = APIRouter(
    prefix="/auth",
    tags=["Authentication"]
)


@router.get("/google")
def login_with_google():
    return google_login()


@router.get("/callback")
def login_callback(code: str):
    return google_callback(code)


@router.get("/me")
def current_user(
    authorization: str | None = Header(default=None)
):
    if authorization is None:
        raise HTTPException(
            status_code=401,
            detail="Missing Authorization header"
        )

    if not authorization.startswith("Bearer "):
        raise HTTPException(
            status_code=401,
            detail="Invalid Authorization header"
        )

    access_token = authorization.replace(
        "Bearer ",
        "",
        1
    )

    return get_current_user(
        access_token
    )