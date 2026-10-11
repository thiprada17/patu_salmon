from fastapi import APIRouter, Depends

from schemas.player import (
    StarterPlayerRequest,
    StarterPlayerResponse,
)
from services.player_service import PlayerService

router = APIRouter(
    prefix="/player",
    tags=["Player"],
)


@router.post(
    "/starter",
    response_model=StarterPlayerResponse,
)
def create_starter_player(
    data: StarterPlayerRequest,
    service: PlayerService = Depends(PlayerService),
):
    return service.create_starter_player(
        user_id=str(data.userID),
        username=data.username,
        character=data.character,
    )