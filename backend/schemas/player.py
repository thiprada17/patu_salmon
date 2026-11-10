
from uuid import UUID

from typing import Optional

from pydantic import BaseModel, Field


# Starter Player
class StarterPlayerRequest(BaseModel):
    user_id: UUID
    username: str = Field(min_length=1)
    character: str = Field(min_length=1)
    language: str = Field(min_length=1)


class StarterPlayerResponse(BaseModel):
    message: str
    user_id: str
    username: str
    character: str
    language: str


# Player Stats
class StatsPlayerRequest(BaseModel):
    user_id: UUID


class StatsPlayerResponse(BaseModel):
    message: str
    user_id: str
    username: Optional[str] = None
    language: Optional[str] = None
    coin: Optional[int] = None
    work_days: Optional[int] = None
    work_streak: Optional[int] = None
    total_customers_served: Optional[int] = None


# ข้อมูลรายได้ที่ Unity ส่งมาเมื่อจบวัน
class FinishWorkDayRequest(BaseModel):
    user_id: UUID
    coin: int = Field(ge=0)
    work_days: int = Field(ge=0)
    total_customers_served: int = Field(ge=0)
    
# ข้อมูลสถิติหลังบันทึกสำเร็จ
class FinishWorkDayResponse(BaseModel):
    message: str
    user_id: str
    coin: int
    work_days: int
    total_customers_served: int
