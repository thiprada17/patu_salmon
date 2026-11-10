from types import SimpleNamespace

import pytest
from fastapi import HTTPException

from services import auth_service
from services.auth_service import AuthService


@pytest.mark.parametrize("has_profile, expected_new", [(True, False), (False, True)])
def test_google_callback_maps_existing_and_new_users(monkeypatch, has_profile, expected_new):
    result = SimpleNamespace(
        user=SimpleNamespace(id="user-1", email="player@example.test"),
        session=SimpleNamespace(access_token="access", refresh_token="refresh"),
    )
    monkeypatch.setattr(auth_service, "exchange_code", lambda code: result)
    monkeypatch.setattr(AuthService, "_ensure_profile", lambda self, user_id: has_profile)

    response = AuthService().google_callback("authorization-code")
    assert response["user_id"] == "user-1"
    assert response["access_token"] == "access"
    assert response["refresh_token"] == "refresh"
    assert response["has_profile"] is has_profile
    assert response["is_new_user"] is expected_new


def test_google_callback_rejects_missing_provider_session(monkeypatch):
    monkeypatch.setattr(auth_service, "exchange_code", lambda code: SimpleNamespace(user=None, session=None))
    with pytest.raises(HTTPException) as error:
        AuthService().google_callback("code")
    assert error.value.status_code == 401
    assert error.value.detail == "Google authentication failed"


def test_email_continue_rejects_empty_credentials():
    with pytest.raises(HTTPException) as error:
        AuthService().email_continue("  ", "")
    assert error.value.status_code == 400


def test_refresh_session_rejects_missing_refresh_token():
    with pytest.raises(HTTPException) as error:
        AuthService().refresh_session("")
    assert error.value.status_code == 400
