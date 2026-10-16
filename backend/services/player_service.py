from fastapi import HTTPException
from repositories.user_repository import UserRepository
from repositories.stats_repository import StatsRepository


class PlayerService:
    def __init__(self):
        self.users = UserRepository()
        self.stats = StatsRepository()

    def create_starter_player(
        self,
        user_id: str,
        username: str,
        character: str,
        language: str
    ):
        username = username.strip()
        character = character.strip().lower()
        language = language.strip().lower()

        if not username:
            raise HTTPException(
                status_code=400,
                detail="Username is required"
            )

        if character not in ("salmon", "platu"):
            raise HTTPException(
                status_code=400,
                detail="Invalid character"
            )

        if language not in ("english", "thai", "japanese"):
            raise HTTPException(
                status_code=400,
                detail="Invalid language"
            )

        try:
            existing = self.users.get_by_user_id(user_id)

            if not existing:
                try:
                    self.users.create_player(user_id)
                except Exception:
                    existing = self.users.get_by_user_id(user_id)
                    if not existing:
                        raise

            saved = self.users.update_player(
                user_id=user_id,
                username=username,
                character=character,
                language=language
            )

            if not saved:
                raise HTTPException(
                    status_code=500,
                    detail="Player row exists but update failed"
                )

            # สร้าง player_stats หลัง Starter สำเร็จ
            self.create_player_stats(user_id)

            return {
                "message": "Player data saved successfully",
                "user_id": user_id,
                "username": username,
                "character": character,
                "language": language,
            }

        except HTTPException:
            raise
        except Exception as exc:
            print(
                f"[PlayerService] Save player failed: "
                f"{type(exc).__name__}: {exc}"
            )
            raise HTTPException(
                status_code=500,
                detail="Failed to save player data"
            ) from exc


    def create_player_stats(self, user_id: str):
        try:
            stats = self.stats.create_if_not_exists(user_id)

            return {
                "message": "Player stats created successfully",
                **(stats or {})
            }

        except Exception as exc:
            print(
                f"[PlayerService] Create stats failed: "
                f"{type(exc).__name__}: {exc}"
            )
            raise HTTPException(
                status_code=500,
                detail=f"Failed to create player stats: {exc}"
            ) from exc

    
    def get_player_stats(self, user_id: str):
        try:
            stats = self.stats.create_if_not_exists(user_id)

            if not stats:
                raise HTTPException(
                    status_code=500,
                    detail="Could not load player stats"
                )

            return {
                "message": "Player stats loaded successfully",
                **stats,
            }

        except HTTPException:
            raise
        except Exception as exc:
            print(f"[PlayerService] Load stats failed: {exc}")
            raise HTTPException(
                status_code=500,
                detail="Failed to load player stats"
            ) from exc

  
    def finish_work_day(
        self,
        user_id: str,
        coin: int,
        work_days: int,
        total_customers_served: int,
    ):
        try:
            stats = self.stats.create_if_not_exists(user_id)

            if not stats:
                raise HTTPException(
                    status_code=500,
                    detail="Could not load player stats"
                )

            # ใช้ค่าที่ Router ส่งมาเพื่ออัปเดตสถิติ
            saved = self.stats.update_after_workday(
                user_id=user_id,
                coin=coin,
                work_days=work_days,
                total_customers_served=total_customers_served,
            )

            if not saved:
                raise HTTPException(
                    status_code=500,
                    detail="Failed to update player stats"
                )

            return {
                "message": "Work day saved successfully",
                "user_id": user_id,
                "coin": saved["coin"],
                "work_days": saved["work_days"],
                "total_customers_served": saved["total_customers_served"],
            }

        except HTTPException:
            raise
        except Exception as exc:
            print(f"[PlayerService] Finish work day failed: {exc}")
            raise HTTPException(
                status_code=500,
                detail="Failed to save work day"
            ) from exc
