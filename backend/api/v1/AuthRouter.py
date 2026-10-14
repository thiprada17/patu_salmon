
from fastapi import APIRouter, Depends, Header, HTTPException
from pydantic import BaseModel, EmailStr

from schemas.auth import AuthRequest, RefreshRequest
from services.auth_service import AuthService

router = APIRouter(
    prefix="/auth",
    tags=["Authentication"]
)


auth_service = AuthService()



def get_auth_service():
    return AuthService()


class EmailLoginRequest(BaseModel):
    email: EmailStr
    password: str


@router.post("/signup")
def signup(
    data: EmailLoginRequest,
    service: AuthService = Depends(get_auth_service)
):
    return service.email_signup(
        email=str(data.email),
        password=data.password
    )


@router.get("/google")
def login_with_google(
    service: AuthService = Depends(get_auth_service)
):
    return service.google_login()


@router.get("/callback")
def login_callback(
    code: str,
    service: AuthService = Depends(get_auth_service)
):
    return service.google_callback(code)


@router.get("/me")
def current_user(
    authorization: str | None = Header(default=None),
    service: AuthService = Depends(get_auth_service)
):
    if not authorization:
        raise HTTPException(
            status_code=401,
            detail="Missing Authorization header"
        )

    if not authorization.startswith("Bearer "):
        raise HTTPException(
            status_code=401,
            detail="Invalid Authorization header"
        )

    access_token = authorization.removeprefix("Bearer ").strip()

    return service.get_current_user(access_token)


@router.post("/email")
def email_auth(
    data: EmailLoginRequest,
    service: AuthService = Depends(get_auth_service)
):
    return service.email_login(
        email=str(data.email),
        password=data.password
    )


@router.post("/continue")
def continue_auth(
    data: AuthRequest,
    service: AuthService = Depends(get_auth_service)
):
    return service.email_continue(
        email=str(data.email),
        password=data.password
    )


@router.post("/refresh")
def refresh_session(request: RefreshRequest):
    return auth_service.refresh_session(request.refresh_token)