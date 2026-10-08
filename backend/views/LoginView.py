from pydantic import BaseModel


class LoginResponse(BaseModel):
    message: str
    user: dict | None = None
    access_token: str | None = None
    refresh_token: str | None = None