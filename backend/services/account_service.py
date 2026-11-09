import logging

from fastapi import HTTPException

from infrastructure.supabase_client import admin_client


logger = logging.getLogger(__name__)


class AccountService:
    """Delete a player's game data and their Supabase Auth identity."""

    def delete_account(self, user_id: str) -> dict[str, str]:
        if admin_client is None:
            raise HTTPException(status_code=503, detail="Supabase is not configured")

        try:
            schema = admin_client.schema("user_management")
            # Remove known dependent rows before removing the auth identity.
            schema.table("player_stats").delete().eq("user_id", user_id).execute()
            schema.table("users").delete().eq("user_id", user_id).execute()
            admin_client.auth.admin.delete_user(user_id)
        except Exception as exc:
            logger.exception("Account deletion failed for user_id=%s", user_id)
            raise HTTPException(
                status_code=500,
                detail="Unable to delete account data",
            ) from exc

        return {"message": "Account deleted", "user_id": user_id}
