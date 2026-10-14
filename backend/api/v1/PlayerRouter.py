
from uuid import UUID

from fastapi import APIRouter, Depends

from schemas.player import (
    StarterPlayerRequest,
    StarterPlayerResponse,
    StatsPlayerRequest,
    StatsPlayerResponse,
    FinishWorkDayRequest,
    FinishWorkDayResponse,
)
from services.player_service import PlayerService


router = APIRouter(prefix="/player", tags=["Player"])


# บันทึกข้อมูลผู้เล่นตอนเริ่มต้น
@router.post("/starter", response_model=StarterPlayerResponse)
def create_starter_player(
    data: StarterPlayerRequest,
    service: PlayerService = Depends(PlayerService),
):
    return service.create_starter_player(
        user_id=str(data.user_id),
        username=data.username,
        character=data.character,
        language=data.language,
    )


# สร้าง player_stats ถ้ายังไม่มี
@router.post("/stats", response_model=StatsPlayerResponse)
def create_stats_player(
    data: StatsPlayerRequest,
    service: PlayerService = Depends(PlayerService),
):
    return service.create_player_stats(user_id=str(data.user_id))


# โหลดเหรียญและสถิติเดิมของผู้เล่น
@router.get("/stats/{user_id}", response_model=StatsPlayerResponse)
def get_player_stats(
    user_id: UUID,
    service: PlayerService = Depends(PlayerService),
):
    return service.get_player_stats(str(user_id))


# บันทึกผลการทำงานเมื่อจบวัน
@router.post("/stats/day", response_model=FinishWorkDayResponse)
def finish_work_day(
    data: FinishWorkDayRequest,
    service: PlayerService = Depends(PlayerService),
):
    return service.finish_work_day(
        user_id=str(data.user_id),
        coin=data.coin,
        work_days= data.work_days,
        total_customers_served=data.total_customers_served,
    )

