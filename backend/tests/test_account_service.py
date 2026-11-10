from types import SimpleNamespace

import pytest
from fastapi import HTTPException

from services import account_service
from services.account_service import AccountService


class DeleteQuery:
    def __init__(self, table_name, events):
        self.table_name = table_name
        self.events = events

    def delete(self):
        self.events.append(("delete", self.table_name))
        return self

    def eq(self, key, value):
        self.events.append(("eq", self.table_name, key, value))
        return self

    def execute(self):
        self.events.append(("execute", self.table_name))


class FakeAdmin:
    def __init__(self):
        self.events = []
        self.auth = SimpleNamespace(admin=SimpleNamespace(delete_user=self.delete_user))

    def schema(self, name):
        self.events.append(("schema", name))
        return self

    def table(self, name):
        return DeleteQuery(name, self.events)

    def delete_user(self, user_id):
        self.events.append(("delete_user", user_id))


def test_account_deletion_removes_dependent_rows_then_auth_identity(monkeypatch):
    client = FakeAdmin()
    monkeypatch.setattr(account_service, "admin_client", client)
    response = AccountService().delete_account("user-1")
    assert response == {"message": "Account deleted", "user_id": "user-1"}
    assert client.events[-1] == ("delete_user", "user-1")
    assert [event[1] for event in client.events if event[0] == "delete"] == ["player_stats", "users"]


def test_account_deletion_fails_closed_when_supabase_unavailable(monkeypatch):
    monkeypatch.setattr(account_service, "admin_client", None)
    with pytest.raises(HTTPException) as error:
        AccountService().delete_account("user-1")
    assert error.value.status_code == 503


def test_account_deletion_database_error_is_sanitized(monkeypatch):
    client = FakeAdmin()
    client.schema = lambda name: (_ for _ in ()).throw(RuntimeError("SERVICE_ROLE_SECRET"))
    monkeypatch.setattr(account_service, "admin_client", client)
    with pytest.raises(HTTPException) as error:
        AccountService().delete_account("user-1")
    assert error.value.status_code == 500
    assert error.value.detail == "Unable to delete account data"
    assert "SECRET" not in error.value.detail
