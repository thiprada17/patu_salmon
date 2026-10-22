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

    # สร้าง URL Login Google
    def google_login(self):
        return {"login_url": create_google_login_url()}

    # รับ code จาก Google แลกเป็น Token
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

    # verify token เพื่อดึงข้อมูลผู้ใช้ปจบ.
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


    # check ว่ามีบัญชีมั้ย ถ้ามีให้ Login ถ้าไม่มีให้ Signup
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
            logger.debug("Existing auth account found: %s", existing_user is not None)
        except Exception:
            logger.exception("Failed to find auth user")
            raise HTTPException(
                status_code=500,
                detail="Unable to check account",
            )

        if existing_user:
            # บัญชีเดิม: เข้าสู่ระบบด้วยอีเมลและรหัสผ่าน
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
                    code in ("invalid_credentials", "invalid_login_credentials")
                    or "invalid login credentials" in message
                    or "invalid password" in message
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

    # หาว่า email นี้อยู่ใน supabase auth อยู่แล้วมั้ย
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

    # หาว่า email นี้อยู่ใน user_management.users อยู่แล้วมั้ย
    def _profile_exists(self, user_id: str) -> dict | None:
        result = (
            admin_client
            .schema("user_management")
            .table("users")
            .select("user_id, character")
            .eq("user_id", user_id)
            .limit(1)
            .execute()
        )

        return result.data[0] if result.data else None


    # verify และ สร้างโปรไฟล์ถ้าไม่มี

    def _ensure_profile(self, user_id: str) -> bool:
        users = (
            admin_client
            .schema("user_management")
            .table("users")
        )

        # ตรวจสอบโปรไฟล์ผ่าน function ที่มีอยู่แล้ว
        player = self._profile_exists(user_id)

        if not player:
            # สร้างโปรไฟล์สำรองเมื่อยังไม่มี
            try:
                users.insert({
                    "user_id": user_id,
                    "username": f"Player_{user_id[:8]}",
                    "character": "salmon",
                    "language": "english",
                }).execute()
            except Exception:
                # อาจมี request อื่นสร้างโปรไฟล์พร้อมกัน
                logger.warning(
                    "Profile insert failed for user_id=%s; "
                    "checking whether it already exists",
                    user_id,
                    exc_info=True,
                )

            # ตรวจสอบอีกครั้งหลังพยายามสร้างโปรไฟล์
            player = self._profile_exists(user_id)

        if not player:
            logger.error(
                "Profile does not exist after ensure: user_id=%s",
                user_id,
            )
            return False

        # อัปเดตเวลา Login ล่าสุด
        try:
            from datetime import datetime, timezone

            users.update({
                "last_login_at": datetime.now(
                    timezone.utc
                ).isoformat()
            }).eq("user_id", user_id).execute()

        except Exception:
            logger.exception(
                "Failed to update last_login_at for user_id=%s",
                user_id,
            )

        # ตรวจสอบว่าตัวละครที่เลือกถูกต้องหรือไม่
        return player.get("character") in ("salmon", "platu")

    def refresh_session(self, refresh_token: str):
        if not refresh_token:
            raise HTTPException(
                status_code=400,
                detail="Refresh token is required",
            )

        try:
            result = supabase.auth.refresh_session(refresh_token)

            if not result.session or not result.user:
                raise HTTPException(
                    status_code=401,
                    detail="Unable to refresh session",
                )

            return {
                "user_id": result.user.id,
                "email": result.user.email,
                "access_token": result.session.access_token,
                "refresh_token": result.session.refresh_token,
            }

        except HTTPException:
            raise
        except Exception as exc:
            # Refresh token ที่ถูกใช้ไปแล้วหรือหมดอายุ ไม่ควรนำกลับมา Retry
            message = str(exc).lower()
            if "already used" in message or "invalid refresh token" in message:
                logger.info("Refresh token is no longer valid; user must sign in again")
            else:
                logger.exception("Failed to refresh session")

            raise HTTPException(
                status_code=401,
                detail="Session expired. Please login again",
            )