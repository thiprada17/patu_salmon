
from pydantic import BaseModel, EmailStr, Field


class AuthRequest(BaseModel):
    email: EmailStr
    password: str = Field(min_length=1)


class AuthResponse(BaseModel):
    message: str
    user_id: str
    email: str | None = None
    access_token: str | None = None
    refresh_token: str | None = None
    is_new_user: bool
    has_profile: bool
    needs_email_confirmation: bool = False

class RefreshRequest(BaseModel):
    refresh_token: str