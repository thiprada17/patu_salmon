from urllib.parse import urlencode
from fastapi.responses import RedirectResponse
from supabase import create_client
from datetime import datetime, timezone

from config import SUPABASE_URL, SUPABASE_KEY

from services.SupabaseService import (
    create_google_login_url,
    exchange_code,
    get_user
)

supabase = create_client(SUPABASE_URL, SUPABASE_KEY)




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


def email_login(email: str, password: str):

    try:
        response = supabase.auth.sign_in_with_password({
            "email": email,
            "password": password
        })

        if response.user is None:
            raise Exception(
                "User not found"
            )


        if response.session is None:
            raise Exception("Session is empty")
        
        user = response.user

        print("================================")
        print("Email login successful")
        print("User ID:", user.id)
        print("Email:", user.email)
        print("================================")


        # ---------------------------------
        # 2. Check game users table
        # ---------------------------------

        player_response = (
            supabase
            .table("users")
            .select("*")
            .eq("user_id", user.id)
            .execute()
        )


        # ---------------------------------
        # 3. First time Login?
        # ---------------------------------

        if not player_response.data:

            print("First time login")
            print("Creating player...")


            now = datetime.now(
                timezone.utc
            ).isoformat()


            new_player = {
                "user_id": user.id,

                # Required because your
                # database columns are NOT NULL
                "username": "Player",
                "character": "default",
                "language": "en",

                "created_at": now,
                "last_login_at": now
            }


            insert_response = (
                supabase
                .table("users")
                .insert(new_player)
                .execute()
            )


            if not insert_response.data:
                raise Exception(
                    "Failed to create player"
                )


            print("Player created")


        # ---------------------------------
        # 4. Existing player
        # ---------------------------------

        else:

            print("Existing player")

            supabase.table("users").update({
                "last_login_at":
                    datetime.now(
                        timezone.utc
                    ).isoformat()
            }).eq(
                "user_id",
                user.id
            ).execute()


        # ---------------------------------
        # 5. Return tokens to Unity
        # ---------------------------------

        access_token = (
            response.session.access_token
        )

        refresh_token = (
            response.session.refresh_token
        )

        query = urlencode({
            "access_token": access_token,
            "refresh_token": refresh_token
        })

        deep_link = (
            "platusalmon://login-callback?"
            + query
        )


        return RedirectResponse(
            url=deep_link,
            status_code=302
        )

    except Exception as e:

        print("================================")
        print("Email login error")
        print("================================")
        print(str(e))


        return {
            "message": "Email login failed",
            "error": str(e)
        }


def email_signup(email: str, password: str):
    try:
        response = supabase.auth.sign_up({
            "email": email,
            "password": password
        })

        print("Supabase URL:", SUPABASE_URL)
        print("Signup returned user:", response.user is not None)

        if response.user:
         print("Signup user ID:", response.user.id)
         print("Signup email:", response.user.email)
        if response.user is None:
            raise Exception("Failed to create user")

        print("================================")
        print("Email signup successful")
        print("User ID:", response.user.id)
        print("Email:", response.user.email)
        print("================================")

        return {
            "message": "Signup successful",
            "user_id": response.user.id,
            "email": response.user.email,
            "session_created": response.session is not None
        }

    except Exception as e:
        print("Email signup error:", str(e))

        return {
            "message": "Email signup failed",
            "error": str(e)
        }


