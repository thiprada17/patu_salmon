from supabase import create_client
from config import SUPABASE_URL, SUPABASE_KEY

supabase = create_client(SUPABASE_URL, SUPABASE_KEY)


def create_google_login_url():
    response = supabase.auth.sign_in_with_oauth({
        "provider": "google",
        "options": {
            "redirect_to": "http://10.0.2.2:8000/auth/callback",
            # "skip_http_redirect": True
        }
    })

    return response.url


def exchange_code(code: str):
    return supabase.auth.exchange_code_for_session({
        "auth_code": code
    })


def get_user(access_token: str):
    response = supabase.auth.get_user(access_token)

    return response.user