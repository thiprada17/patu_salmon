
from fastapi import APIRouter, Depends, Header, HTTPException, Query
from fastapi.responses import HTMLResponse, RedirectResponse
from pydantic import BaseModel, EmailStr

from schemas.auth import AuthRequest, RefreshRequest
from services.auth_service import AuthService

router = APIRouter(
    prefix="/auth",
    tags=["Authentication"]
)


auth_service = AuthService()



def get_auth_service():
    return AuthService()


def get_authenticated_user(
    authorization: str | None = Header(default=None),
    service: AuthService = Depends(get_auth_service),
):
    if not authorization:
        raise HTTPException(
            status_code=401,
            detail="Missing Authorization header",
        )

    scheme, _, access_token = authorization.partition(" ")
    if scheme.lower() != "bearer" or not access_token.strip():
        raise HTTPException(
            status_code=401,
            detail="Invalid Authorization header",
        )

    return service.get_current_user(access_token.strip())


class EmailLoginRequest(BaseModel):
    email: EmailStr
    password: str


@router.post("/signup")
def signup(
    data: EmailLoginRequest,
    service: AuthService = Depends(get_auth_service)
):
    return service.email_signup(
        email=str(data.email),
        password=data.password
    )


@router.get("/google")
def login_with_google(
    platform: str = Query(default="web", pattern="^(web|ios|android)$"),
    service: AuthService = Depends(get_auth_service)
):
    return service.google_login(platform)


@router.get("/google/redirect")
def login_with_google_redirect(
    service: AuthService = Depends(get_auth_service),
):
    return RedirectResponse(service.google_login("web")["login_url"], status_code=302)


@router.get("/web-callback", response_class=HTMLResponse)
def google_web_callback():
    # The OAuth popup exchanges the one-time code through this backend, then
    # posts the resulting session to the game iframe that opened the popup.
    return HTMLResponse("""<!doctype html>
<meta name="viewport" content="width=device-width,initial-scale=1">
<title>Returning to Platu Salmon</title>
<p id="status">Finishing Google sign-in…</p>
<script>
(async function () {
  const status = document.getElementById('status');
  const params = new URLSearchParams(location.search);
  const code = params.get('code');
  try {
    if (!code) throw new Error(params.get('error_description') || params.get('error') || 'Authorization code missing');
    const response = await fetch('/auth/callback?code=' + encodeURIComponent(code));
    const result = await response.json();
    if (!response.ok) throw new Error(result.detail || 'Unable to finish sign-in');
    if (!window.opener || window.opener.closed) throw new Error('Game window is no longer available');
    window.opener.postMessage({ type: 'platu-google-login', access_token: result.access_token, refresh_token: result.refresh_token, user_id: result.user_id, email: result.email }, '*');
    status.textContent = 'Signed in. You can close this tab.';
    window.close();
  } catch (error) {
    status.textContent = 'Sign-in failed: ' + error.message + '. Return to the game and try again.';
  }
})();
</script>""")


@router.get("/callback")
def login_callback(
    code: str,
    service: AuthService = Depends(get_auth_service)
):
    return service.google_callback(code)


@router.get("/me")
def current_user(
    user: dict = Depends(get_authenticated_user),
):
    return user


@router.post("/email")
def email_auth(
    data: EmailLoginRequest,
    service: AuthService = Depends(get_auth_service)
):
    return service.email_login(
        email=str(data.email),
        password=data.password
    )


@router.post("/continue")
def continue_auth(
    data: AuthRequest,
    service: AuthService = Depends(get_auth_service)
):
    return service.email_continue(
        email=str(data.email),
        password=data.password
    )


@router.post("/refresh")
def refresh_session(request: RefreshRequest):
    return auth_service.refresh_session(request.refresh_token)
