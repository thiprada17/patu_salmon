import logging 
from fastapi import HTTPException

from services.google_auth_service import (
    create_google_login_url,
    exchange_code,
    get_user,
)
from utils.deeplink import create_login_deep_link
from infrastructure.supabase_client import supabase, admin_client


logger = logging.getLogger(__name__)


class AuthService:
    def __init__(self):
        pass

    def google_login(self):
        return {"login_url": create_google_login_url()}

    def google_callback(self, code: str):
        try:
            result = exchange_code(code)

            if not result.user or not result.session:
                raise HTTPException(
                    status_code=401,
                    detail="Google authentication failed",
                )

            user = result.user
            session = result.session
            has_profile = self._ensure_profile(user.id)

            return {
                "user_id": user.id,
                "email": user.email,
                "access_token": session.access_token,
                "refresh_token": session.refresh_token,
                "has_profile": has_profile,
                "deep_link": create_login_deep_link(
                    session.access_token,
                    session.refresh_token,
                ),
            }

        except HTTPException:
            raise
        except Exception:
            logger.exception("Google callback failed")
            raise HTTPException(
                status_code=500,
                detail="Google login failed",
            )

    def get_current_user(self, access_token: str):
        try:
            user = get_user(access_token)

            if not user:
                raise HTTPException(
                    status_code=401,
                    detail="Invalid access token",
                )

            return {
                "user_id": user.id,
                "email": user.email,
            }

        except HTTPException:
            raise
        except Exception:
            logger.exception("Failed to verify current user")
            raise HTTPException(
                status_code=401,
                detail="Unable to verify user",
            )


    def email_continue(self, email: str, password: str):
        email = email.strip().lower()

        if not email or not password:
            raise HTTPException(
                status_code=400,
                detail="Email and password are required",
            )

        # ตรวจว่ามีบัญชีอยู่แล้วหรือไม่
        try:
            existing_user = self._find_auth_user(email)
        except Exception:
            logger.exception("Failed to find auth user")
            raise HTTPException(
                status_code=500,
                detail="Unable to check account",
            )

        if existing_user:
            # บัญชีเดิม: Login เท่านั้น
            try:
                result = supabase.auth.sign_in_with_password({
                    "email": email,
                    "password": password,
                })
            except Exception as exc:
                message = str(exc).lower()
                code = str(getattr(exc, "code", "")).lower()

                if (
                    code == "email_not_confirmed"
                    or "email not confirmed" in message
                ):
                    raise HTTPException(
                        status_code=403,
                        detail="Please confirm your email first",
                    )

                if (
                    code == "invalid_credentials"
                    or "invalid login credentials" in message
                ):
                    raise HTTPException(
                        status_code=401,
                        detail="Incorrect email or password",
                    )

                logger.exception("Email login failed")
                raise HTTPException(
                    status_code=502,
                    detail="Authentication service error",
                )

            is_new_user = False

        else:
            # บัญชีใหม่: Signup
            try:
                result = supabase.auth.sign_up({
                    "email": email,
                    "password": password,
                })
            except Exception:
                logger.exception("Supabase email signup failed")
                raise HTTPException(
                    status_code=502,
                    detail="Unable to create account",
                )

            if not result.user:
                raise HTTPException(
                    status_code=400,
                    detail="Unable to create account",
                )

            is_new_user = True

            # เมื่อเปิด Email Confirmation อาจยังไม่มี session
            # Trigger ในฐานข้อมูลจะสร้าง profile ให้เมื่อ signup สำเร็จ
            if not result.session:
                return {
                    "message": "Please confirm your email",
                    "user_id": result.user.id,
                    "email": result.user.email,
                    "access_token": None,
                    "refresh_token": None,
                    "is_new_user": True,
                    "has_profile": self._profile_exists(
                        result.user.id
                    ),
                    "needs_email_confirmation": True,
                }

        if not result.user or not result.session:
            raise HTTPException(
                status_code=401,
                detail="Authentication failed",
            )

        has_profile = self._ensure_profile(result.user.id)

        return {
            "message": (
                "Signup successful"
                if is_new_user
                else "Login successful"
            ),
            "user_id": result.user.id,
            "email": result.user.email,
            "access_token": result.session.access_token,
            "refresh_token": result.session.refresh_token,
            "is_new_user": is_new_user,
            "has_profile": has_profile,
            "needs_email_confirmation": False,
        }


    def _find_auth_user(self, email: str):
        page = 1
        per_page = 1000
        target = email.strip().casefold()

        while True:
            users = admin_client.auth.admin.list_users(
                page=page,
                per_page=per_page,
            )

            for user in users:
                if (user.email or "").strip().casefold() == target:
                    return user

            if len(users) < per_page:
                return None

            page += 1


    def _profile_exists(self, user_id: str) -> bool:
        result = (
            admin_client
            .schema("user_management")
            .table("users")
            .select("user_id")
            .eq("user_id", user_id)
            .limit(1)
            .execute()
        )

        return bool(result.data)

    def _ensure_profile(self, user_id: str) -> bool:
        users = (
            admin_client
            .schema("user_management")
            .table("users")
        )

        # ตรวจสอบก่อน โดยใช้ admin_client เพื่อหลีกเลี่ยงปัญหา RLS
        result = (
            users
            .select("user_id, character")
            .eq("user_id", user_id)
            .limit(1)
            .execute()
        )

        if not result.data:
            # ส่วนนี้เป็น fallback กรณีที่ไม่มีโปรไฟล์จริง ๆ
            try:
                users.insert({
                    "user_id": user_id,
                    "username": "Player",
                    "character": "salmon",
                    "language": "english",
                }).execute()
            except Exception:
                # อาจมี request อื่นสร้างแถวนี้พร้อมกัน
                logger.warning(
                    "Profile insert failed for user_id=%s; "
                    "checking whether it already exists",
                    user_id,
                    exc_info=True,
                )

            # อ่านใหม่หลัง insert หรือเมื่อ insert ล้มเหลว
            result = (
                users
                .select("user_id, character")
                .eq("user_id", user_id)
                .limit(1)
                .execute()
            )

        if not result.data:
            logger.error(
                "Profile does not exist after ensure: user_id=%s",
                user_id,
            )
            return False

        player = result.data[0]

        try:
            from datetime import datetime, timezone

            users.update({
                "last_login_at": datetime.now(
                    timezone.utc
                ).isoformat()
            }).eq("user_id", user_id).execute()
        except Exception:
            # ไม่ควรทำให้ Login ล้มเหลวเพียงเพราะอัปเดตเวลาไม่ได้
            logger.exception(
                "Failed to update last_login_at for user_id=%s",
                user_id,
            )

        return player.get("character") in ("salmon", "platu")