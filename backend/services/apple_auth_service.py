"""Apple identity-token validation and Supabase sign-in.

Development mock tokens use an unsigned JWT with a dedicated issuer. The
mock issuer is accepted only when ENVIRONMENT=dev. Production verifies
Apple's RS256 signature and the token issuer, audience, time claims, subject,
and nonce before handing the token to Supabase Auth.
"""

from __future__ import annotations

import hashlib
import time
import uuid
from typing import Any

import jwt
from fastapi import HTTPException
from jwt import InvalidTokenError, PyJWKClientError

from core.settings import Settings, get_settings


APPLE_ISSUER = "https://appleid.apple.com"
APPLE_MOCK_ISSUER = "https://appleid.apple.com.mock"
APPLE_JWKS_URL = "https://appleid.apple.com/auth/keys"
MOCK_USER_NAMESPACE = uuid.UUID("a8f1b64d-2b12-4aa1-8dd5-65b9f21d9b66")


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


def create_apple_client_secret(settings: Settings | None = None) -> str:
    """Create the Apple client-secret JWT used when configuring Apple in Supabase.

    This is for server-side provider setup only. Never ship the .p8 key or this
    secret in the Unity client. Native ID-token validation itself uses Apple's
    public JWKS.
    """
    settings = settings or get_settings()
    if not settings.apple_team_id or not settings.apple_key_id or not settings.normalized_apple_private_key:
        raise RuntimeError("APPLE_TEAM_ID, APPLE_KEY_ID, and APPLE_PRIVATE_KEY are required")
    client_ids = _configured_audiences(settings)
    now = int(time.time())
    return jwt.encode(
        {
            "iss": settings.apple_team_id,
            "iat": now,
            "exp": now + (60 * 60 * 24 * 150),
            "aud": "https://appleid.apple.com",
            "sub": client_ids[0],
        },
        settings.normalized_apple_private_key,
        algorithm="ES256",
        headers={"kid": settings.apple_key_id},
    )


def _supabase_sign_in(identity_token: str):
    from supabase import ClientOptions, create_client

    from core.config import SUPABASE_ANON_KEY, SUPABASE_URL

    if not SUPABASE_URL or not SUPABASE_ANON_KEY:
        raise HTTPException(status_code=503, detail="Supabase is not configured")

    client = create_client(
        SUPABASE_URL,
        SUPABASE_ANON_KEY,
        options=ClientOptions(auto_refresh_token=False, persist_session=False),
    )
    # Nonce is verified above. Omitting it here avoids re-hashing a nonce that
    # Supabase Auth may interpret differently across provider/client versions.
    return client.auth.sign_in_with_id_token({"provider": "apple", "token": identity_token})


def _has_completed_profile(user_id: str) -> bool:
    from services.auth_service import AuthService

    return AuthService()._ensure_profile(user_id)


def apple_login(identity_token: str, nonce: str | None = None) -> dict[str, Any]:
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

    try:
        result = _supabase_sign_in(identity_token)
        if not result.user or not result.session:
            raise HTTPException(status_code=401, detail="Supabase Apple sign-in failed")

        has_profile = _has_completed_profile(result.user.id)
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
        raise HTTPException(status_code=401, detail="Apple sign-in with Supabase failed") from exc
