from uuid import UUID
from pydantic import BaseModel, Field


class StarterPlayerRequest(BaseModel):
    userID: UUID
    username: str = Field(min_length=1)
    character: str = Field(min_length=1)


class StarterPlayerResponse(BaseModel):
    message: str
    userID: str
    username: str
    character: str