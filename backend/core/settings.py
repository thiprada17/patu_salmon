from functools import lru_cache
import os
from typing import Literal

from pydantic import AliasChoices, Field, SecretStr
from pydantic_settings import BaseSettings, SettingsConfigDict


class Settings(BaseSettings):
    """Environment configuration shared by API and auth services."""

    model_config = SettingsConfigDict(
        env_file=(".env", "backend/.env"),
        env_file_encoding="utf-8",
        extra="ignore",
        case_sensitive=False,
    )

    environment: Literal["dev", "production"] = Field(
        default_factory=lambda: os.getenv("ENVIRONMENT", "dev")
    )

    # Use a comma-separated list when Apple issues tokens for multiple app IDs,
    # for example the iOS Bundle ID and an Apple Services ID.
    apple_client_id: str = Field(
        default_factory=lambda: os.getenv("APPLE_CLIENT_ID", "")
    )
    # Apple production credentials are optional during development.
    # Explicit None defaults keep a dev server runnable when no Apple keys exist.
    apple_team_id: str | None = Field(
        default_factory=lambda: os.getenv("APPLE_TEAM_ID", None)
    )
    apple_key_id: str | None = Field(
        default_factory=lambda: os.getenv("APPLE_KEY_ID", None)
    )
    apple_private_key: SecretStr | None = Field(
        default_factory=lambda: (
            SecretStr(os.getenv("APPLE_PRIVATE_KEY"))
            if os.getenv("APPLE_PRIVATE_KEY")
            else None
        )
    )

    supabase_url: str = ""
    backend_public_url: str = Field(
        default="https://patu-salmon.onrender.com",
        validation_alias=AliasChoices("BACKEND_PUBLIC_URL", "BACKEND_URL"),
    )
    supabase_anon_key: SecretStr = Field(
        default=SecretStr(""),
        validation_alias=AliasChoices("SUPABASE_ANON_KEY", "SUPABASE_KEY"),
    )
    supabase_service_role_key: SecretStr = SecretStr("")

    @property
    def apple_client_ids(self) -> list[str]:
        return [value.strip() for value in self.apple_client_id.split(",") if value.strip()]

    @property
    def normalized_apple_private_key(self) -> str:
        if self.apple_private_key is None:
            return ""
        return self.apple_private_key.get_secret_value().replace("\\n", "\n")


@lru_cache(maxsize=1)
def get_settings() -> Settings:
    return Settings()
