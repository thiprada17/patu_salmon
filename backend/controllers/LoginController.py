from urllib.parse import urlencode

from fastapi.responses import RedirectResponse
from supabase import create_client

from config import SUPABASE_URL, SUPABASE_KEY


supabase = create_client(
    SUPABASE_URL,
    SUPABASE_KEY
)


# =========================
# Google Login
# =========================

def google_login():

    response = supabase.auth.sign_in_with_oauth({
        "provider": "google",
        "options": {
            "redirect_to": "http://127.0.0.1:8000/auth/callback"
        }
    })

    return {
        "login_url": response.url
    }


# =========================
# Google Callback
# =========================

def google_callback(code: str):
    try:
        print("================================")
        print("Google callback code received")
        print("================================")

        response = supabase.auth.exchange_code_for_session({
            "auth_code": code
        })

        if response.session is None:
            raise Exception("Session is empty")

        access_token = response.session.access_token
        refresh_token = response.session.refresh_token

        if not access_token:
            raise Exception("Access token is empty")

        print("Google login successful")
        print("Access token received")

        query = urlencode({
            "access_token": access_token,
            "refresh_token": refresh_token
        })

        deep_link = (
            "platusalmon://login-callback?"
            + query
        )

        print("Redirecting back to Unity")
        print("platusalmon://login-callback?...")

        return RedirectResponse(
            url=deep_link,
            status_code=302
        )

    except Exception as e:
        print("================================")
        print("Google callback error")
        print("================================")
        print(str(e))

        return {
            "message": "Google login failed",
            "error": str(e)
        }


# =========================
# Get Current User
# =========================

def get_current_user(access_token: str):

    try:

        response = supabase.auth.get_user(
            access_token
        )

        user = response.user

        if user is None:
            raise Exception("User not found")

        return {
            "user": {
                "id": user.id,
                "email": user.email,
                "name": user.user_metadata.get(
                    "full_name"
                ),
                "avatar_url": user.user_metadata.get(
                    "avatar_url"
                )
            }
        }

    except Exception as e:

        print("Get current user error:")
        print(e)

        raise e