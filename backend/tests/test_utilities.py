from urllib.parse import parse_qs, urlsplit

from utils.deeplink import create_login_deep_link


def test_deep_link_encodes_tokens_and_preserves_query_values():
    link = create_login_deep_link("access +/&=", "refresh +/&=")
    parsed = urlsplit(link)
    query = parse_qs(parsed.query)
    assert parsed.scheme == "platusalmon"
    assert parsed.netloc == "login-callback"
    assert query == {"access_token": ["access +/&="], "refresh_token": ["refresh +/&="]}
