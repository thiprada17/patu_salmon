from fastapi import HTTPException
from repositories.user_repository import UserRepository


class PlayerService:
    def __init__(self):
        self.users = UserRepository()

    def create_starter_player(
        self,
        user_id: str,
        username: str,
        character: str,
    ):
        username = username.strip()
        character = character.strip().lower()

        if not username:
            raise HTTPException(status_code=400, detail="Username is required")

        if character not in ("salmon", "platu"):
            raise HTTPException(status_code=400, detail="Invalid character")

        try:

            existing = self.users.get_by_user_id(user_id)

            if existing:
                saved = self.users.update_player(
                    user_id=user_id,
                    username=username,
                    character=character,
                )
            else:
                try:
                    self.users.create_player(user_id)
                except Exception:
                    # อาจมี trigger สร้างแถวระหว่างที่กำลังทำงาน
                    existing = self.users.get_by_user_id(user_id)
                    if not existing:
                        raise

                saved = self.users.update_player(
                    user_id=user_id,
                    username=username,
                    character=character,
                )

            if not saved:
                raise HTTPException(
                    status_code=500,
                    detail="Player row exists but update failed",
                )


            return {
                "message": "Player data saved successfully",
                "userID": user_id,
                "username": username,
                "character": character,
            }

        except HTTPException:
            raise
        except Exception as exc:
            print(f"[PlayerService] Save player failed: {type(exc).__name__}: {exc}")
            raise HTTPException(
                status_code=500,
                detail="Failed to save player data",
            )