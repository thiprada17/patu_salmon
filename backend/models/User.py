from pydantic import BaseModel, Field
from datetime import datetime, timezone

#User Class (OAuth)
class User(BaseModel):
    id: str
    email: str | None = None
    name: str | None = None
    avatar_url: str | None = None

#Player Class (Email)
class Player(BaseModel):

    user_id: str
    username: str | None = None
    character: str | None = None
    language: str | None = None
    created_at: datetime = Field(default_factory=lambda: datetime.now(timezone.utc))
    last_login_at: datetime = Field(default_factory=lambda: datetime.now(timezone.utc))