import hashlib
import time
from types import SimpleNamespace

import jwt
import pytest
from fastapi import HTTPException

from core.settings import Settings
from services.apple_auth_service import apple_login, verify_apple_token


def _mock_token(audience="com.platusalmon.game", nonce="raw-nonce"):
    now = int(time.time())
    return jwt.encode(
        {
            "iss": "https://appleid.apple.com.mock",
            "aud": audience,
            "exp": now + 300,
            "iat": now,
            "sub": "editor-test-user",
            "nonce": hashlib.sha256(nonce.encode()).hexdigest(),
            "email": "editor@example.invalid",
        },
        key="",
        algorithm="none",
    )


def test_mock_flow_accepts_only_dev_issuer_and_returns_dev_session(monkeypatch):
    settings = Settings(environment="dev", apple_client_id="com.platusalmon.game")
    monkeypatch.setattr("services.apple_auth_service.get_settings", lambda: settings)

    response = apple_login(_mock_token(), nonce="raw-nonce")

    assert response["mock_mode"] is True
    assert response["is_new_user"] is True
    assert response["access_token"].count(".") == 2
    assert response["refresh_token"] == ""


def test_valid_apple_token_uses_jwks_and_checks_claims():
    from cryptography.hazmat.primitives.asymmetric import rsa

    private_key = rsa.generate_private_key(public_exponent=65537, key_size=2048)
    public_key = private_key.public_key()
    raw_nonce = "ios-random-nonce"
    now = int(time.time())
    token = jwt.encode(
        {
            "iss": "https://appleid.apple.com",
            "aud": "com.platusalmon.game",
            "exp": now + 300,
            "iat": now,
            "sub": "apple-subject-123",
            "nonce": hashlib.sha256(raw_nonce.encode()).hexdigest(),
        },
        private_key,
        algorithm="RS256",
        headers={"kid": "test-key"},
    )

    class FakeSigningKey:
        key = public_key

    class FakeJwksClient:
        def get_signing_key_from_jwt(self, received_token):
            assert received_token == token
            return FakeSigningKey()

    settings = Settings(
        environment="production",
        apple_client_id="com.platusalmon.game",
        apple_team_id="TEAM123",
        apple_key_id="KEY123",
        apple_private_key="test-private-key-configured",
    )
    claims = verify_apple_token(token, raw_nonce, settings, FakeJwksClient())

    assert claims["sub"] == "apple-subject-123"
    assert claims["mock_mode"] is False


def test_production_login_exchanges_verified_token_for_supabase_session(monkeypatch):
    settings = Settings(
        environment="production",
        apple_client_id="com.platusalmon.game",
        apple_team_id="TEAM123",
        apple_key_id="KEY123",
        apple_private_key="test-private-key-configured",
    )
    token = "verified-apple-identity-token"
    session = SimpleNamespace(access_token="supabase-access", refresh_token="supabase-refresh")
    user = SimpleNamespace(id="supabase-user-id", email="player@example.com")
    supabase_result = SimpleNamespace(session=session, user=user)
    passed_tokens = []

    monkeypatch.setattr("services.apple_auth_service.get_settings", lambda: settings)
    monkeypatch.setattr(
        "services.apple_auth_service.verify_apple_token",
        lambda identity_token, nonce, settings: {"sub": "apple-subject", "mock_mode": False},
    )
    monkeypatch.setattr(
        "services.apple_auth_service._supabase_sign_in",
        lambda identity_token, nonce: passed_tokens.append((identity_token, nonce)) or supabase_result,
    )
    monkeypatch.setattr("services.apple_auth_service._has_completed_profile", lambda user_id: True)

    response = apple_login(token, nonce="raw-nonce")

    assert passed_tokens == [(token, "raw-nonce")]
    assert response["user_id"] == "supabase-user-id"
    assert response["access_token"] == "supabase-access"
    assert response["refresh_token"] == "supabase-refresh"
    assert response["has_profile"] is True
    assert response["is_new_user"] is False
    assert response["mock_mode"] is False


def test_invalid_audience_returns_401():
    from cryptography.hazmat.primitives.asymmetric import rsa

    private_key = rsa.generate_private_key(public_exponent=65537, key_size=2048)
    now = int(time.time())
    token = jwt.encode(
        {
            "iss": "https://appleid.apple.com",
            "aud": "another.app.id",
            "exp": now + 300,
            "iat": now,
            "sub": "apple-subject-123",
        },
        private_key,
        algorithm="RS256",
        headers={"kid": "test-key"},
    )

    class FakeSigningKey:
        key = private_key.public_key()

    class FakeJwksClient:
        def get_signing_key_from_jwt(self, received_token):
            return FakeSigningKey()

    settings = Settings(
        environment="production",
        apple_client_id="com.platusalmon.game",
        apple_team_id="TEAM123",
        apple_key_id="KEY123",
        apple_private_key="test-private-key-configured",
    )
    with pytest.raises(HTTPException) as error:
        verify_apple_token(token, settings=settings, jwks_client=FakeJwksClient())

    assert error.value.status_code == 401


def test_production_missing_apple_credentials_returns_500_before_token_validation():
    settings = Settings(environment="production", apple_client_id="com.platusalmon.game")

    with pytest.raises(HTTPException) as error:
        verify_apple_token("", settings=settings)

    assert error.value.status_code == 500
    assert "APPLE_TEAM_ID" in error.value.detail
    assert "APPLE_PRIVATE_KEY" in error.value.detail
