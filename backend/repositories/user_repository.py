
from datetime import datetime, timezone

from infrastructure.supabase_client import db_client, admin_client


class UserRepository:
    def _users(self):
        return admin_client.schema("user_management").table("users")

    def get_by_user_id(self, user_id: str):
        result = (
            self._users()
            .select("user_id, username, character", "language")
            .eq("user_id", user_id)
            .limit(1)
            .execute()
        )
        return result.data[0] if result.data else None

    def create_player(self, user_id: str):
        now = datetime.now(timezone.utc).isoformat()

        self._users(use_admin=True).insert({
            "user_id": user_id,
            "username": f"Player_{user_id[:8]}",
            "character": "salmon",
            "language": "english",
            "created_at": now,
            "last_login_at": now,
        }).execute()


    def update_last_login(self, user_id: str):
        now = datetime.now(timezone.utc).isoformat()

        self._users().update({
            "last_login_at": now,
        }).eq("user_id", user_id).execute()

    def update_player(
        self,
        user_id: str,
        username: str,
        character: str,
        language: str
    ) -> bool:
        result = (
            self._users()
            .update({
                "username": username,
                "character": character,
                "language": language
            })
            .eq("user_id", user_id)
            .execute()
        )
        return bool(result.data)
