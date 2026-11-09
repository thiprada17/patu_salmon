"""HTTP-level tests for dev Apple auth, production guards, and account deletion.

The Supabase deletion test is destructive by design, so it is opt-in and creates
and deletes only a fresh test account. Run it only against a dedicated test project.
"""

import hashlib
import os
import time
import uuid

import jwt
import pytest
from fastapi.testclient import TestClient

from core.settings import Settings
from main import app
from services import apple_auth_service


client = TestClient(app)


def make_mock_identity_token(
    audience: str = "com.platusalmon.game",
    nonce: str = "pytest-apple-nonce",
) -> str:
    now = int(time.time())
    return jwt.encode(
        {
            "iss": "https://appleid.apple.com.mock",
            "aud": audience,
            "sub": "pytest-dev-user",
            "email": "pytest@example.invalid",
            "iat": now,
            "exp": now + 300,
            "nonce": hashlib.sha256(nonce.encode("utf-8")).hexdigest(),
        },
        key="",
        algorithm="none",
    )


def test_dev_mock_auth_endpoint_returns_mock_user(monkeypatch):
    settings = Settings(environment="dev", apple_client_id="com.platusalmon.game")
    monkeypatch.setattr(apple_auth_service, "get_settings", lambda: settings)

    response = client.post(
        "/api/v1/auth/apple",
        json={"identity_token": make_mock_identity_token(), "nonce": "pytest-apple-nonce"},
    )

    assert response.status_code == 200, response.text
    body = response.json()
    assert body["mock_mode"] is True
    assert body["is_new_user"] is True
    assert body["user_id"]
    assert body["email"] == "pytest@example.invalid"
    assert body["access_token"].startswith("eyJ")
    assert body["refresh_token"] == ""


def test_production_without_apple_private_key_returns_configuration_500(monkeypatch):
    settings = Settings(
        environment="production",
        apple_client_id="com.platusalmon.game",
        apple_team_id="TESTTEAM",
        apple_key_id="TESTKEY",
        apple_private_key=None,
    )
    monkeypatch.setattr(apple_auth_service, "get_settings", lambda: settings)

    response = client.post(
        "/api/v1/auth/apple",
        json={"identity_token": "not-needed-for-config-guard", "nonce": "test"},
    )

    assert response.status_code == 500
    assert "APPLE_PRIVATE_KEY" in response.json()["detail"]


def test_delete_account_removes_supabase_user_and_player_rows():
    if os.getenv("RUN_SUPABASE_DESTRUCTIVE_TESTS") != "1":
        pytest.skip("Set RUN_SUPABASE_DESTRUCTIVE_TESTS=1 to enable destructive Supabase integration test")

    from infrastructure.supabase_client import admin_client, supabase

    if admin_client is None or supabase is None:
        pytest.skip("Configure Supabase URL, anon key, and service-role key first")

    email = f"platu-delete-test-{uuid.uuid4().hex}@example.invalid"
    password = f"T{uuid.uuid4().hex}!a9"
    user_id = None
    try:
        created = admin_client.auth.admin.create_user(
            {"email": email, "password": password, "email_confirm": True}
        )
        user_id = created.user.id

        user_management = admin_client.schema("user_management")
        user_management.table("users").upsert(
            {
                "user_id": user_id,
                "username": f"DeleteTest_{user_id[:8]}",
                "character": "salmon",
                "language": "english",
            }
        ).execute()
        user_management.table("player_stats").upsert(
            {
                "user_id": user_id,
                "coin": 0,
                "work_days": 0,
                "work_streak": 0,
                "total_customers_served": 0,
            }
        ).execute()

        signed_in = supabase.auth.sign_in_with_password(
            {"email": email, "password": password}
        )
        access_token = signed_in.session.access_token

        response = client.delete(
            "/api/v1/user/account",
            headers={"Authorization": f"Bearer {access_token}"},
        )

        assert response.status_code == 200, response.text
        assert response.json()["user_id"] == user_id
        assert not user_management.table("users").select("user_id").eq("user_id", user_id).execute().data
        assert not user_management.table("player_stats").select("user_id").eq("user_id", user_id).execute().data

        with pytest.raises(Exception):
            admin_client.auth.admin.get_user_by_id(user_id)
    finally:
        if user_id:
            try:
                user_management = admin_client.schema("user_management")
                user_management.table("player_stats").delete().eq("user_id", user_id).execute()
                user_management.table("users").delete().eq("user_id", user_id).execute()
                admin_client.auth.admin.delete_user(user_id)
            except Exception:
                # The assertion path may have deleted everything successfully.
                pass
