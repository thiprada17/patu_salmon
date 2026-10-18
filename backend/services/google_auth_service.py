from infrastructure.supabase_client import supabase
from core.config import DEEPLINK_URL


def create_google_login_url():
    response = supabase.auth.sign_in_with_oauth({
        "provider": "google",
        "options": {
            "redirect_to": "https://patu-salmon.onrender.com/auth/callback",
        },
    })
    return response.url


def exchange_code(code: str):
    return supabase.auth.exchange_code_for_session({
        "auth_code": code,
    })


def get_user(access_token: str):
    response = supabase.auth.get_user(access_token)
    return response.user


def create_login_deep_link(access_token, refresh_token):
    return (
        f"{DEEPLINK_URL}"
        f"?access_token={access_token}"
        f"&refresh_token={refresh_token}"
    )