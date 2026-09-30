from supabase import create_client

from config import SUPABASE_URL, SUPABASE_KEY


supabase = create_client(
    SUPABASE_URL,
    SUPABASE_KEY
)


def get_google_login_url(redirect_url):
    response = supabase.auth.sign_in_with_oauth({
        "provider": "google",
        "options": {
            "redirect_to": redirect_url
        }
    })

    return response.url


def exchange_code(code):
    response = supabase.auth.exchange_code_for_session({
        "auth_code": code
    })

    return response


def get_user(access_token):
    response = supabase.auth.get_user(
        access_token
    )

    return response.user