# Apple Sign In configuration

## Development without an Apple Developer account

Set `ENVIRONMENT=dev` and omit `APPLE_TEAM_ID`, `APPLE_KEY_ID`, and
`APPLE_PRIVATE_KEY`. In Unity
Editor, the `AppleAuthConfig` asset enables the mock button by default. The
client sends an unsigned token with issuer
`https://appleid.apple.com.mock`; the backend accepts that issuer only in dev,
checks its audience, expiry, issue time, subject, and nonce, and returns a
deterministic development identity. This is not a Supabase session and must
never be used in a production deployment.

## Endpoint paths

The versioned endpoint is `POST /api/v1/auth/apple`. The existing
`POST /auth/apple` route remains available for already released Unity builds.

## Production

1. In Apple Developer, create the Sign in with Apple App ID capability for the
   iOS Bundle ID. For web flows, create a Services ID associated with that App
   ID and configure its return URL/domain.
2. Set `ENVIRONMENT=production`. Requests return HTTP 500 with the missing
   variable names if any production Apple credentials are absent. Set
   `APPLE_CLIENT_ID` to the Services ID
   first, followed by any other accepted audience IDs separated by commas
   (including the iOS Bundle ID if both token types are used). Configure
   `APPLE_TEAM_ID`, `APPLE_KEY_ID`, and the `.p8` contents in
   `APPLE_PRIVATE_KEY`; store these as server secrets, never in Unity.
3. Configure the Apple provider in Supabase Auth with the matching Services ID
   and Apple key details. `create_apple_client_secret()` generates the ES256
   client-secret JWT when needed for provider setup; Apple client secrets
   expire, so regenerate and update Supabase before expiry.
4. Set `SUPABASE_URL` and `SUPABASE_ANON_KEY`. The backend validates Apple's
   RS256 token against Apple's JWKS, then exchanges it through Supabase Auth
   and returns the Supabase session. The `.p8` private key is for Apple client
   secret generation; token signatures are verified with Apple's public JWKS.
5. In Unity, set `AppleAuthConfig.appleClientId` to the matching app ID and
   build/sign for iOS with Sign in with Apple enabled for that App ID. The
   Editor mock switch is compiled only into Editor builds.

## Apple token revocation during account deletion

Set `APPLE_TOKEN_ENCRYPTION_KEY` on the backend to the URL-safe base64
encoding of 32 random bytes. Apply `migrations/20261110_apple_auth_tokens.sql`
to Supabase. The iOS sign-in flow sends Apple's authorization code to the
backend, which exchanges it and stores the refresh token encrypted in the
service-role-only `user_management.apple_auth_tokens` table. Account deletion
revokes that token through Apple's `/auth/revoke` endpoint before deleting the
Supabase account. Existing Apple users need to sign in once after this update
to capture a revocable token; deletion returns an error if the token is missing
or Apple cannot revoke it.

## Tests

Install test dependencies with `pip install -r requirements-dev.txt` from the
`backend` directory, then run `pytest tests/test_apple_auth.py` there. Tests
use a generated RSA key and a fake JWKS client; they do not call Apple or
Supabase.

`pytest tests/test_auth_system.py` covers the versioned HTTP endpoints and
works without Apple credentials. Its Supabase account deletion test is skipped
unless `RUN_SUPABASE_DESTRUCTIVE_TESTS=1` is set. Enable that flag only with a
dedicated Supabase test project: the test creates a fresh account, calls the
real deletion endpoint, and verifies the user, profile, and stats rows are gone.

In Unity Test Runner, run `AppleAuthEditorIntegrationTests` with the local
backend using `ENVIRONMENT=dev`. The test invokes the Editor mock submit path
and checks that the API response is stored as the Unity login session. The
project does not install a Supabase Unity SDK; Supabase token exchange happens
on the backend, while Unity stores the resulting access token in `PlayerPrefs`.
