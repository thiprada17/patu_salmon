"""Apple identity-token validation and Supabase sign-in.

Development mock tokens use an unsigned JWT with a dedicated issuer. The
mock issuer is accepted only when ENVIRONMENT=dev. Production verifies
Apple's RS256 signature and the token issuer, audience, time claims, subject,
and nonce before handing the token to Supabase Auth.
"""

from __future__ import annotations

import hashlib
import logging
import os
import time
import uuid
import base64
from typing import Any

import httpx
import jwt
from fastapi import HTTPException
from jwt import InvalidTokenError, PyJWKClientError

from core.settings import Settings, get_settings


APPLE_ISSUER = "https://appleid.apple.com"
APPLE_MOCK_ISSUER = "https://appleid.apple.com.mock"
APPLE_JWKS_URL = "https://appleid.apple.com/auth/keys"
APPLE_TOKEN_URL = "https://appleid.apple.com/auth/token"
APPLE_REVOKE_URL = "https://appleid.apple.com/auth/revoke"
MOCK_USER_NAMESPACE = uuid.UUID("a8f1b64d-2b12-4aa1-8dd5-65b9f21d9b66")
logger = logging.getLogger(__name__)


def _configured_audiences(settings: Settings) -> list[str]:
    if settings.apple_client_ids:
        return settings.apple_client_ids
    if settings.environment == "dev":
        return ["com.platusalmon.game"]
    raise HTTPException(status_code=503, detail="APPLE_CLIENT_ID is not configured")


def _verify_mock_token(token: str, settings: Settings, nonce: str | None) -> dict[str, Any]:
    try:
        claims = jwt.decode(
            token,
            options={
                "verify_signature": False,
                "verify_aud": False,
                "verify_iss": False,
                "verify_exp": False,
                "verify_iat": False,
                "require": ["iss", "aud", "exp", "iat", "sub", "nonce"],
            },
            algorithms=["none"],
        )
    except InvalidTokenError as exc:
        raise HTTPException(status_code=401, detail="Invalid Apple mock identity token") from exc

    if claims.get("iss") != APPLE_MOCK_ISSUER:
        raise HTTPException(status_code=401, detail="Invalid Apple mock issuer")

    audiences = _configured_audiences(settings)
    token_audiences = claims.get("aud")
    token_audiences = token_audiences if isinstance(token_audiences, list) else [token_audiences]
    if not any(aud in audiences for aud in token_audiences):
        raise HTTPException(status_code=401, detail="Apple identity token audience mismatch")

    now = int(time.time())
    if not isinstance(claims.get("exp"), int) or claims["exp"] <= now:
        raise HTTPException(status_code=401, detail="Apple identity token has expired")
    if not isinstance(claims.get("iat"), int) or claims["iat"] > now + 60:
        raise HTTPException(status_code=401, detail="Invalid Apple identity token issue time")
    if not isinstance(claims.get("sub"), str) or not claims["sub"].strip():
        raise HTTPException(status_code=401, detail="Apple identity token subject is missing")

    _verify_nonce(claims, nonce)
    claims["mock_mode"] = True
    return claims


def _verify_nonce(claims: dict[str, Any], nonce: str | None) -> None:
    if nonce is None:
        return
    if not nonce:
        raise HTTPException(status_code=401, detail="Apple nonce is required")

    expected_nonce = hashlib.sha256(nonce.encode("utf-8")).hexdigest()
    if claims.get("nonce") != expected_nonce:
        raise HTTPException(status_code=401, detail="Apple identity token nonce mismatch")


def verify_apple_token(
    identity_token: str,
    nonce: str | None = None,
    settings: Settings | None = None,
    jwks_client: Any | None = None,
) -> dict[str, Any]:
    """Validate an Apple ID token, or the dev-only mock issuer."""
    settings = settings or get_settings()

    # Fail with a clear server configuration error before trying to parse or
    # verify a production token. Development does not require Apple credentials.
    if settings.environment == "production":
        missing = [
            name
            for name, value in (
                ("APPLE_CLIENT_ID", settings.apple_client_id),
                ("APPLE_TEAM_ID", settings.apple_team_id),
                ("APPLE_KEY_ID", settings.apple_key_id),
                ("APPLE_PRIVATE_KEY", settings.normalized_apple_private_key),
            )
            if not value
        ]
        if missing:
            raise HTTPException(
                status_code=500,
                detail="Missing production Apple configuration: " + ", ".join(missing),
            )

    if not identity_token:
        raise HTTPException(status_code=401, detail="Apple identity token is required")

    try:
        unverified = jwt.decode(
            identity_token,
            options={"verify_signature": False, "verify_aud": False},
            algorithms=["RS256", "none"],
        )
    except InvalidTokenError as exc:
        raise HTTPException(status_code=401, detail="Malformed Apple identity token") from exc

    issuer = unverified.get("iss")
    if issuer == APPLE_MOCK_ISSUER:
        if settings.environment != "dev":
            raise HTTPException(status_code=401, detail="Mock Apple tokens are disabled")
        return _verify_mock_token(identity_token, settings, nonce)

    audiences = _configured_audiences(settings)
    client = jwks_client or jwt.PyJWKClient(APPLE_JWKS_URL, cache_keys=True)

    try:
        signing_key = client.get_signing_key_from_jwt(identity_token)
        claims = jwt.decode(
            identity_token,
            signing_key.key,
            algorithms=["RS256"],
            issuer=APPLE_ISSUER,
            audience=audiences,
            leeway=30,
            options={"require": ["iss", "aud", "exp", "iat", "sub"]},
        )
    except (InvalidTokenError, PyJWKClientError, ValueError) as exc:
        logger.warning(
            "Apple identity token verification rejected: error_type=%s issuer=%r audience=%r",
            type(exc).__name__,
            issuer,
            unverified.get("aud"),
        )
        raise HTTPException(status_code=401, detail="Apple identity token verification failed") from exc

    if not isinstance(claims.get("sub"), str) or not claims["sub"].strip():
        raise HTTPException(status_code=401, detail="Apple identity token subject is missing")

    _verify_nonce(claims, nonce)
    claims["mock_mode"] = False
    return claims


def is_mock_issuer_token(token: str) -> bool:
    """Identify the dev token type without trusting any of its other claims."""
    try:
        claims = jwt.decode(
            token,
            options={"verify_signature": False, "verify_aud": False},
            algorithms=["RS256", "none"],
        )
        return claims.get("iss") == APPLE_MOCK_ISSUER
    except InvalidTokenError:
        return False


def mock_user_id(subject: str) -> str:
    return str(uuid.uuid5(MOCK_USER_NAMESPACE, subject))


def create_apple_client_secret(settings: Settings | None = None, client_id: str | None = None) -> str:
    """Create the Apple client-secret JWT used when configuring Apple in Supabase.

    This is for server-side provider setup only. Never ship the .p8 key or this
    secret in the Unity client. Native ID-token validation itself uses Apple's
    public JWKS.
    """
    settings = settings or get_settings()
    if not settings.apple_team_id or not settings.apple_key_id or not settings.normalized_apple_private_key:
        raise RuntimeError("APPLE_TEAM_ID, APPLE_KEY_ID, and APPLE_PRIVATE_KEY are required")
    client_ids = _configured_audiences(settings)
    client_id = client_id or client_ids[0]
    if client_id not in client_ids:
        raise RuntimeError("Apple client ID is not configured")
    now = int(time.time())
    return jwt.encode(
        {
            "iss": settings.apple_team_id,
            "iat": now,
            "exp": now + (60 * 60 * 24 * 150),
            "aud": "https://appleid.apple.com",
            "sub": client_id,
        },
        settings.normalized_apple_private_key,
        algorithm="ES256",
        headers={"kid": settings.apple_key_id},
    )


def _apple_client_id_for_claims(claims: dict[str, Any], settings: Settings) -> str:
    audiences = claims.get("aud")
    audiences = audiences if isinstance(audiences, list) else [audiences]
    for client_id in _configured_audiences(settings):
        if client_id in audiences:
            return client_id
    raise HTTPException(status_code=401, detail="Apple identity token audience mismatch")


def _token_cipher(settings: Settings):
    from cryptography.hazmat.primitives.ciphers.aead import AESGCM

    key_text = settings.normalized_apple_token_encryption_key
    if not key_text:
        raise HTTPException(status_code=503, detail="APPLE_TOKEN_ENCRYPTION_KEY is not configured")
    try:
        key = base64.urlsafe_b64decode(key_text.encode("ascii"))
    except (ValueError, UnicodeEncodeError) as exc:
        raise HTTPException(status_code=503, detail="APPLE_TOKEN_ENCRYPTION_KEY must be base64 encoded") from exc
    if len(key) != 32:
        raise HTTPException(status_code=503, detail="APPLE_TOKEN_ENCRYPTION_KEY must decode to 32 bytes")
    return AESGCM(key)


def _encrypt_apple_refresh_token(token: str, settings: Settings) -> str:
    cipher = _token_cipher(settings)
    nonce = os.urandom(12)
    encrypted = cipher.encrypt(nonce, token.encode("utf-8"), b"apple-refresh-token-v1")
    return base64.urlsafe_b64encode(nonce + encrypted).decode("ascii")


def _decrypt_apple_refresh_token(encrypted_token: str, settings: Settings) -> str:
    cipher = _token_cipher(settings)
    try:
        raw = base64.urlsafe_b64decode(encrypted_token.encode("ascii"))
        return cipher.decrypt(raw[:12], raw[12:], b"apple-refresh-token-v1").decode("utf-8")
    except Exception as exc:
        logger.exception("Stored Apple refresh token could not be decrypted")
        raise HTTPException(status_code=500, detail="Unable to read Apple account token") from exc


def _exchange_apple_authorization_code(code: str, client_id: str, settings: Settings) -> dict[str, Any]:
    if not code:
        raise HTTPException(status_code=400, detail="Apple authorization code is required")
    try:
        response = httpx.post(
            APPLE_TOKEN_URL,
            data={
                "client_id": client_id,
                "client_secret": create_apple_client_secret(settings, client_id),
                "code": code,
                "grant_type": "authorization_code",
            },
            headers={"Content-Type": "application/x-www-form-urlencoded"},
            timeout=15.0,
        )
        if response.is_error:
            # Apple returns a small JSON error (for example invalid_client or
            # invalid_grant). Log only that response, never submitted tokens or
            # the generated client secret.
            try:
                apple_error = response.json().get("error", "unknown_error")
            except (ValueError, AttributeError):
                apple_error = "non_json_error_response"
            logger.warning(
                "Apple authorization code exchange rejected: status=%s error=%s",
                response.status_code,
                apple_error,
            )
            raise HTTPException(status_code=401, detail="Apple authorization code exchange failed")
        try:
            result = response.json()
        except ValueError as exc:
            logger.warning("Apple token endpoint returned invalid JSON: status=%s", response.status_code)
            raise HTTPException(status_code=502, detail="Apple token endpoint returned an invalid response") from exc
        if not result.get("refresh_token"):
            logger.warning(
                "Apple token endpoint response omitted refresh_token: fields=%s",
                sorted(result.keys()),
            )
            raise HTTPException(status_code=401, detail="Apple did not return a refresh token")
        return result
    except HTTPException:
        raise
    except Exception as exc:
        logger.warning("Apple authorization code exchange failed: error_type=%s", type(exc).__name__)
        raise HTTPException(status_code=401, detail="Apple authorization code exchange failed") from exc


def _store_apple_refresh_token(user_id: str, refresh_token: str, client_id: str, settings: Settings) -> None:
    from infrastructure.supabase_client import admin_client

    if admin_client is None:
        raise HTTPException(status_code=503, detail="Supabase is not configured")
    encrypted = _encrypt_apple_refresh_token(refresh_token, settings)
    try:
        admin_client.schema("user_management").table("apple_auth_tokens").upsert(
            {
                "user_id": user_id,
                "refresh_token_encrypted": encrypted,
                "client_id": client_id,
                "updated_at": time.strftime("%Y-%m-%dT%H:%M:%SZ", time.gmtime()),
            },
            on_conflict="user_id",
        ).execute()
    except Exception as exc:
        logger.exception("Failed to store Apple token for user_id=%s", user_id)
        raise HTTPException(status_code=503, detail="Unable to securely store Apple account token") from exc


def revoke_apple_account_token(user_id: str, settings: Settings | None = None) -> None:
    """Revoke a user's Apple grant; must succeed before account deletion proceeds."""
    from infrastructure.supabase_client import admin_client

    settings = settings or get_settings()
    if admin_client is None:
        raise HTTPException(status_code=503, detail="Supabase is not configured")
    try:
        rows = (
            admin_client.schema("user_management")
            .table("apple_auth_tokens")
            .select("refresh_token_encrypted,client_id")
            .eq("user_id", user_id)
            .limit(1)
            .execute()
            .data
        )
    except Exception as exc:
        logger.exception("Failed to load Apple token for user_id=%s", user_id)
        raise HTTPException(status_code=503, detail="Unable to load Apple account token") from exc
    if not rows:
        raise HTTPException(
            status_code=409,
            detail="Apple sign-in must be refreshed before deleting this account",
        )

    token_row = rows[0]
    refresh_token = _decrypt_apple_refresh_token(token_row["refresh_token_encrypted"], settings)
    client_id = token_row["client_id"]
    try:
        response = httpx.post(
            APPLE_REVOKE_URL,
            data={
                "client_id": client_id,
                "client_secret": create_apple_client_secret(settings, client_id),
                "token": refresh_token,
                "token_type_hint": "refresh_token",
            },
            headers={"Content-Type": "application/x-www-form-urlencoded"},
            timeout=15.0,
        )
        response.raise_for_status()
    except Exception as exc:
        logger.warning("Apple token revocation failed for user_id=%s: error_type=%s", user_id, type(exc).__name__)
        raise HTTPException(status_code=503, detail="Apple token revocation failed; account was not deleted") from exc

    try:
        admin_client.schema("user_management").table("apple_auth_tokens").delete().eq("user_id", user_id).execute()
    except Exception as exc:
        logger.exception("Failed to remove revoked Apple token for user_id=%s", user_id)
        raise HTTPException(status_code=503, detail="Apple token was revoked but account cleanup could not continue") from exc


def _supabase_sign_in(identity_token: str, nonce: str | None = None):
    from supabase import ClientOptions, create_client

    from core.config import SUPABASE_ANON_KEY, SUPABASE_URL

    if not SUPABASE_URL or not SUPABASE_ANON_KEY:
        raise HTTPException(status_code=503, detail="Supabase is not configured")

    client = create_client(
        SUPABASE_URL,
        SUPABASE_ANON_KEY,
        options=ClientOptions(auto_refresh_token=False, persist_session=False),
    )
    credentials = {"provider": "apple", "token": identity_token}
    if nonce is not None:
        # Supabase expects the raw nonce and hashes it when checking the
        # SHA-256 nonce claim returned by Apple's native sign-in flow.
        credentials["nonce"] = nonce
    return client.auth.sign_in_with_id_token(credentials)


def _has_completed_profile(user_id: str) -> bool:
    from services.auth_service import AuthService

    return AuthService()._ensure_profile(user_id)


def apple_login(
    identity_token: str,
    nonce: str | None = None,
    authorization_code: str | None = None,
) -> dict[str, Any]:
    settings = get_settings()
    claims = verify_apple_token(identity_token, nonce=nonce, settings=settings)

    if claims["mock_mode"]:
        # Mock tokens are test credentials only; they never create a Supabase
        # session and are rejected in production.
        return {
            "user_id": mock_user_id(claims["sub"]),
            "email": claims.get("email", "apple-mock@example.invalid"),
            "access_token": identity_token,
            "refresh_token": "",
            "has_profile": False,
            "is_new_user": True,
            "mock_mode": True,
        }

    client_id = _apple_client_id_for_claims(claims, settings)
    try:
        result = _supabase_sign_in(identity_token, nonce)
        if not result.user or not result.session:
            raise HTTPException(status_code=401, detail="Supabase Apple sign-in failed")

        has_profile = _has_completed_profile(result.user.id)

        # The ID token is sufficient to complete sign-in with Supabase. Apple
        # authorization codes are short-lived and single-use, so a failed
        # code exchange must not lock the user out of the app. Keep attempting
        # to capture a refresh token for later Apple grant revocation, but let
        # account deletion report the missing token if capture never succeeds.
        if authorization_code:
            try:
                token_response = _exchange_apple_authorization_code(
                    authorization_code, client_id, settings
                )
                _store_apple_refresh_token(
                    result.user.id,
                    token_response["refresh_token"],
                    client_id,
                    settings,
                )
                logger.info("Stored Apple revocation token for user_id=%s", result.user.id)
            except Exception as exc:
                if isinstance(exc, HTTPException):
                    logger.warning(
                        "Apple sign-in succeeded but revocation token capture failed: "
                        "user_id=%s status=%s detail=%s",
                        result.user.id,
                        exc.status_code,
                        exc.detail,
                    )
                else:
                    logger.exception(
                        "Apple sign-in succeeded but revocation token capture failed: "
                        "user_id=%s error_type=%s",
                        result.user.id,
                        type(exc).__name__,
                    )
        else:
            logger.warning(
                "Apple sign-in succeeded without an authorization code; revocation token not captured: user_id=%s",
                result.user.id,
            )

        return {
            "user_id": result.user.id,
            "email": result.user.email or claims.get("email"),
            "access_token": result.session.access_token,
            "refresh_token": result.session.refresh_token,
            "has_profile": has_profile,
            "is_new_user": not has_profile,
            "mock_mode": False,
        }
    except HTTPException:
        raise
    except Exception as exc:
        logger.exception(
            "Supabase Apple sign-in failed: error_type=%s",
            type(exc).__name__,
        )
        raise HTTPException(status_code=401, detail="Apple sign-in with Supabase failed") from exc
