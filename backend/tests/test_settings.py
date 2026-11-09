from pydantic import SecretStr
from fastapi import HTTPException
import pytest

from core.settings import Settings
from services.apple_auth_service import verify_apple_token


def test_settings_default_to_dev_with_optional_apple_production_secrets(monkeypatch):
    monkeypatch.delenv("ENVIRONMENT", raising=False)
    settings = Settings(_env_file=None)
    assert settings.environment == "dev"
    assert settings.apple_team_id is None
    assert settings.apple_key_id is None
    assert settings.apple_private_key is None


def test_settings_load_environment_values(monkeypatch):
    monkeypatch.setenv("ENVIRONMENT", "dev")
    monkeypatch.setenv("APPLE_CLIENT_ID", "game.id,service.id")
    monkeypatch.setenv("APPLE_TEAM_ID", "TEAM")
    monkeypatch.setenv("APPLE_KEY_ID", "KEY")
    monkeypatch.setenv("APPLE_PRIVATE_KEY", "line1\\nline2")
    settings = Settings(_env_file=None)
    assert settings.apple_client_ids == ["game.id", "service.id"]
    assert settings.normalized_apple_private_key == "line1\nline2"
    assert isinstance(settings.apple_private_key, SecretStr)


def test_dev_does_not_require_production_credentials():
    settings = Settings(environment="dev", apple_client_id="game.id")
    with pytest.raises(HTTPException) as error:
        verify_apple_token("not-a-token", settings=settings)
    assert error.value.status_code == 401


def test_production_missing_secrets_fails_before_token_validation():
    settings = Settings(environment="production", apple_client_id="game.id")
    try:
        verify_apple_token("malformed", settings=settings)
    except HTTPException as error:
        assert error.status_code == 500
        assert "APPLE_TEAM_ID" in error.detail
        assert "APPLE_PRIVATE_KEY" in error.detail
    else:
        raise AssertionError("Missing production credentials must be rejected")
