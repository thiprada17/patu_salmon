from core.config import DEEPLINK_URL
from urllib.parse import urlencode


def create_login_deep_link(access_token, refresh_token):

    return f"{DEEPLINK_URL}?{urlencode({'access_token': access_token, 'refresh_token': refresh_token})}"
