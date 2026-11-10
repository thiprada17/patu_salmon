"""Opt-in end-to-end tests for the Supabase -> FastAPI player flow.

These tests create a real Supabase Auth user and write player data. Run only
against a dedicated test project with RUN_SUPABASE_INTEGRATION_TESTS=1.
"""

import os
import uuid

import pytest
from fastapi.testclient import TestClient

from infrastructure.supabase_client import admin_client, supabase
from main import app


client = TestClient(app)


def _require_supabase_integration():
    if os.getenv("RUN_SUPABASE_INTEGRATION_TESTS") != "1":
        pytest.skip(
            "Set RUN_SUPABASE_INTEGRATION_TESTS=1 to enable Supabase integration tests"
        )
    if admin_client is None or supabase is None:
        pytest.skip("Configure Supabase URL, anon key, and service-role key first")


def _create_user():
    email = f"platu-integration-{uuid.uuid4().hex}@example.com"
    password = f"T{uuid.uuid4().hex}!a9"
    created = admin_client.auth.admin.create_user(
        {"email": email, "password": password, "email_confirm": True}
    )
    user_id = str(created.user.id)
    signed_in = supabase.auth.sign_in_with_password(
        {"email": email, "password": password}
    )
    return user_id, email, password, signed_in


def _cleanup_user(user_id):
    if not user_id:
        return
    user_management = admin_client.schema("user_management")
    try:
        user_management.table("player_stats").delete().eq("user_id", user_id).execute()
        user_management.table("users").delete().eq("user_id", user_id).execute()
    finally:
        admin_client.auth.admin.delete_user(user_id)


def test_supabase_player_lifecycle_through_backend_api():
    _require_supabase_integration()

    email = f"platu-integration-{uuid.uuid4().hex}@example.invalid"
    password = f"T{uuid.uuid4().hex}!a9"
    user_id = None
    user_management = admin_client.schema("user_management")

    try:
        created = admin_client.auth.admin.create_user(
            {"email": email, "password": password, "email_confirm": True}
        )
        user_id = str(created.user.id)

        # Authenticate through Supabase, then exercise the same HTTP contract
        # used by Unity instead of calling services or repositories directly.
        signed_in = supabase.auth.sign_in_with_password(
            {"email": email, "password": password}
        )
        access_token = signed_in.session.access_token
        headers = {"Authorization": f"Bearer {access_token}"}

        me = client.get("/auth/me", headers=headers)
        assert me.status_code == 200, me.text
        assert me.json()["user_id"] == user_id

        refreshed = client.post(
            "/auth/refresh",
            json={"refresh_token": signed_in.session.refresh_token},
        )
        assert refreshed.status_code == 200, refreshed.text
        assert refreshed.json()["user_id"] == user_id
        headers = {
            "Authorization": f"Bearer {refreshed.json()['access_token']}"
        }

        starter = client.post(
            "/player/starter",
            headers=headers,
            json={
                "user_id": user_id,
                "username": "Integration Salmon",
                "character": "salmon",
                "language": "english",
            },
        )
        assert starter.status_code == 200, starter.text
        assert starter.json()["username"] == "Integration Salmon"

        initial = client.get(f"/player/stats/{user_id}", headers=headers)
        assert initial.status_code == 200, initial.text
        assert initial.json() == {
            "message": "Player stats loaded successfully",
            "user_id": user_id,
            "coin": 0,
            "work_days": 0,
            "work_streak": 0,
            "total_customers_served": 0,
        }

        saved = client.post(
            "/player/stats/day",
            headers=headers,
            json={
                "user_id": user_id,
                "coin": 125,
                "work_days": 1,
                "total_customers_served": 8,
            },
        )
        assert saved.status_code == 200, saved.text
        assert saved.json()["coin"] == 125
        assert saved.json()["work_days"] == 1
        assert saved.json()["total_customers_served"] == 8

        reloaded = client.get(f"/player/stats/{user_id}", headers=headers)
        assert reloaded.status_code == 200, reloaded.text
        assert reloaded.json()["coin"] == 125
        assert reloaded.json()["work_days"] == 1
        assert reloaded.json()["total_customers_served"] == 8

        # Verify persistence independently from the API response.
        profile_rows = (
            user_management.table("users")
            .select("user_id, username, character, language")
            .eq("user_id", user_id)
            .execute()
            .data
        )
        stats_rows = (
            user_management.table("player_stats")
            .select("user_id, coin, work_days, total_customers_served")
            .eq("user_id", user_id)
            .execute()
            .data
        )
        assert profile_rows == [{
            "user_id": user_id,
            "username": "Integration Salmon",
            "character": "salmon",
            "language": "english",
        }]
        assert stats_rows == [{
            "user_id": user_id,
            "coin": 125,
            "work_days": 1,
            "total_customers_served": 8,
        }]

    finally:
        _cleanup_user(user_id)


def test_supabase_email_continue_login_and_invalid_credentials():
    _require_supabase_integration()
    user_id = None
    try:
        user_id, email, password, _ = _create_user()

        login = client.post(
            "/auth/continue",
            json={"email": email.upper(), "password": password},
        )
        assert login.status_code == 200, login.text
        assert login.json()["user_id"] == user_id
        assert login.json()["is_new_user"] is False
        assert login.json()["access_token"]
        assert login.json()["refresh_token"]

        rejected = client.post(
            "/auth/continue",
            json={"email": email, "password": "wrong-password"},
        )
        assert rejected.status_code == 401, rejected.text
    finally:
        _cleanup_user(user_id)


def test_supabase_email_continue_signs_up_a_new_user():
    _require_supabase_integration()
    user_id = None
    try:
        email = f"platu-signup-integration-{uuid.uuid4().hex}@example.com"
        password = f"T{uuid.uuid4().hex}!a9"
        signup = client.post(
            "/auth/continue",
            json={"email": email, "password": password},
        )
        assert signup.status_code == 200, signup.text
        result = signup.json()
        user_id = result["user_id"]
        assert result["is_new_user"] is True
        assert result["email"] == email

        # Supabase may require email confirmation, in which case no session is
        # returned; otherwise the new session must authenticate at /auth/me.
        if result.get("needs_email_confirmation"):
            assert not result.get("access_token")
        else:
            me = client.get(
                "/auth/me",
                headers={"Authorization": f"Bearer {result['access_token']}"},
            )
            assert me.status_code == 200, me.text
            assert me.json()["user_id"] == user_id
    finally:
        _cleanup_user(user_id)


def test_supabase_auth_rejects_invalid_tokens_and_refresh_requests():
    _require_supabase_integration()
    assert client.get(
        "/auth/me", headers={"Authorization": "Bearer invalid-token"}
    ).status_code == 401
    assert client.post("/auth/refresh", json={"refresh_token": "invalid-refresh-token"}).status_code == 401
    assert client.post("/auth/refresh", json={}).status_code == 422


def test_supabase_player_api_rejects_invalid_or_foreign_requests():
    _require_supabase_integration()
    user_id = None
    try:
        user_id, _, _, signed_in = _create_user()
        headers = {"Authorization": f"Bearer {signed_in.session.access_token}"}

        assert client.get("/player/stats/00000000-0000-0000-0000-000000000001").status_code == 401

        foreign = client.get(
            "/player/stats/00000000-0000-0000-0000-000000000001",
            headers=headers,
        )
        assert foreign.status_code == 403, foreign.text

        invalid_starter = client.post(
            "/player/starter",
            headers=headers,
            json={"user_id": user_id, "username": " ", "character": "dragon", "language": "english"},
        )
        assert invalid_starter.status_code == 400, invalid_starter.text

        invalid_day = client.post(
            "/player/stats/day",
            headers=headers,
            json={"user_id": user_id, "coin": -1, "work_days": 1, "total_customers_served": 0},
        )
        assert invalid_day.status_code == 422, invalid_day.text
    finally:
        _cleanup_user(user_id)


def test_supabase_stats_endpoint_is_idempotent_and_persists_workday():
    _require_supabase_integration()
    user_id = None
    try:
        user_id, _, _, signed_in = _create_user()
        headers = {"Authorization": f"Bearer {signed_in.session.access_token}"}

        created = client.post("/player/stats", headers=headers, json={"user_id": user_id})
        assert created.status_code == 200, created.text
        repeated = client.post("/player/stats", headers=headers, json={"user_id": user_id})
        assert repeated.status_code == 200, repeated.text
        assert repeated.json()["coin"] == 0

        saved = client.post(
            "/player/stats/day",
            headers=headers,
            json={"user_id": user_id, "coin": 42, "work_days": 3, "total_customers_served": 7},
        )
        assert saved.status_code == 200, saved.text
        loaded = client.get(f"/player/stats/{user_id}", headers=headers)
        assert loaded.status_code == 200, loaded.text
        assert loaded.json()["coin"] == 42
        assert loaded.json()["work_days"] == 3
        assert loaded.json()["total_customers_served"] == 7

        persisted = (
            admin_client.schema("user_management")
            .table("player_stats")
            .select("coin, work_days, work_streak, total_customers_served")
            .eq("user_id", user_id)
            .single()
            .execute()
            .data
        )
        assert persisted == {
            "coin": 42,
            "work_days": 3,
            "work_streak": 0,
            "total_customers_served": 7,
        }
    finally:
        _cleanup_user(user_id)


def test_supabase_account_delete_endpoint_removes_auth_and_player_data():
    _require_supabase_integration()
    user_id = None
    try:
        user_id, _, _, signed_in = _create_user()
        headers = {"Authorization": f"Bearer {signed_in.session.access_token}"}
        user_management = admin_client.schema("user_management")
        user_management.table("users").upsert({
            "user_id": user_id,
            "username": "Delete Integration",
            "character": "salmon",
            "language": "english",
        }).execute()
        user_management.table("player_stats").upsert({
            "user_id": user_id,
            "coin": 10,
            "work_days": 1,
            "work_streak": 1,
            "total_customers_served": 2,
        }).execute()

        deleted = client.delete("/api/v1/user/account", headers=headers)
        assert deleted.status_code == 200, deleted.text
        assert deleted.json()["user_id"] == user_id
        user_id = None  # Endpoint deleted the account; skip fallback cleanup.

        assert not user_management.table("users").select("user_id").eq("user_id", deleted.json()["user_id"]).execute().data
        assert not user_management.table("player_stats").select("user_id").eq("user_id", deleted.json()["user_id"]).execute().data
        with pytest.raises(Exception):
            admin_client.auth.admin.get_user_by_id(deleted.json()["user_id"])
    finally:
        _cleanup_user(user_id)
