from config import DEEPLINK_URL


def create_login_deep_link(access_token, refresh_token):

    return (
        f"{DEEPLINK_URL}"
        f"?access_token={access_token}"
        f"&refresh_token={refresh_token}"
    )