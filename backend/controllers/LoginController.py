from urllib.parse import urlencode

from fastapi.responses import RedirectResponse

from services.SupabaseService import (
    create_google_login_url,
    exchange_code,
    get_user
)


def google_login():
    login_url = create_google_login_url()

    return {
        "login_url": login_url
    }


def google_callback(code: str):
    try:
        print("Google callback code received")

        response = exchange_code(code)

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
            "platusalmon://login-callback?" + query
        )

        print("Redirecting back to Unity")
        print("platusalmon://login-callback?...")

        return RedirectResponse(
            url=deep_link,
            status_code=302
        )

    except Exception as e:
        print("Google callback error")
        print(str(e))

        return {
            "message": "Google login failed",
            "error": str(e)
        }


def get_current_user(access_token: str):
    user = get_user(access_token)

    return {
        "user": {
            "id": user.id,
            "email": user.email
        }
    }