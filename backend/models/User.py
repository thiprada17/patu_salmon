from pydantic import BaseModel


class User(BaseModel):
    id: str
    email: str | None = None
    name: str | None = None
    avatar_url: str | None = None