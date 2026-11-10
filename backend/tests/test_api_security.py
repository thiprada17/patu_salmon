from fastapi.testclient import TestClient

from api.v1.AuthRouter import get_authenticated_user, get_auth_service
from main import app
from services.player_service import PlayerService


client = TestClient(app)


def test_auth_dependency_rejects_missing_and_malformed_bearer_headers():
    assert client.get("/auth/me").status_code == 401
    assert client.get("/auth/me", headers={"Authorization": "Basic abc"}).status_code == 401
    assert client.get("/auth/me", headers={"Authorization": "Bearer "}).status_code == 401


def test_player_endpoint_rejects_other_players_data():
    app.dependency_overrides[get_authenticated_user] = lambda: {"user_id": "user-a"}
    try:
        response = client.get("/player/stats/00000000-0000-0000-0000-000000000002")
        assert response.status_code == 403
    finally:
        app.dependency_overrides.pop(get_authenticated_user, None)


def test_player_endpoint_rejects_invalid_uuid_and_missing_required_fields():
    app.dependency_overrides[get_authenticated_user] = lambda: {"user_id": "00000000-0000-0000-0000-000000000001"}
    try:
        assert client.get("/player/stats/not-a-uuid").status_code == 422
        response = client.post("/player/stats", json={})
        assert response.status_code == 422
    finally:
        app.dependency_overrides.pop(get_authenticated_user, None)


def test_player_service_does_not_expose_database_exception_in_response(monkeypatch):
    service = PlayerService()
    monkeypatch.setattr(service.stats, "create_if_not_exists", lambda user_id: (_ for _ in ()).throw(RuntimeError("SECRET stack trace")))
    try:
        service.create_player_stats("u1")
    except Exception as error:
        assert error.status_code == 500
        assert error.detail == "Failed to create player stats"
        assert "SECRET" not in error.detail
    else:
        raise AssertionError("Database failure should become a sanitized HTTP error")


def test_health_endpoint_and_legal_pages_are_served():
    assert client.get("/").json() == {"status": "ok"}
    for page in ("privacy-policy", "terms-of-use", "user-agreement"):
        response = client.get(f"/legal/{page}/")
        assert response.status_code == 200
        assert "text/html" in response.headers["content-type"]


def test_google_auth_provider_failure_returns_sanitized_error(monkeypatch):
    from services import auth_service

    monkeypatch.setattr(auth_service, "exchange_code", lambda code: (_ for _ in ()).throw(RuntimeError("SUPABASE_SECRET private trace")))
    try:
        auth_service.AuthService().google_callback("code")
    except Exception as error:
        assert error.status_code == 500
        assert error.detail == "Google login failed"
        assert "SECRET" not in error.detail
        assert "trace" not in error.detail.lower()
    else:
        raise AssertionError("Provider failure should be sanitized")
