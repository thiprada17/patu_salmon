from types import SimpleNamespace

import pytest

from repositories import stats_repository
from repositories.stats_repository import StatsRepository


class Query:
    def __init__(self, result=None, error=None, calls=None):
        self.result = result or []
        self.error = error
        self.calls = calls if calls is not None else []

    def __getattr__(self, method):
        def record(*args, **kwargs):
            self.calls.append((method, args, kwargs))
            return self
        return record

    def execute(self):
        self.calls.append(("execute", (), {}))
        if self.error:
            raise self.error
        return SimpleNamespace(data=self.result)


class FakeAdmin:
    def __init__(self, queries):
        self.queries = iter(queries)
        self.calls = []

    def schema(self, name):
        self.calls.append(("schema", name))
        return self

    def table(self, name):
        self.calls.append(("table", name))
        return next(self.queries)


def test_stats_repository_reads_expected_schema_table_and_user_row(monkeypatch):
    row = {"user_id": "u1", "coin": 2, "work_days": 1,
           "work_streak": 1, "total_customers_served": 3}
    query = Query([row])
    client = FakeAdmin([query])
    monkeypatch.setattr(stats_repository, "admin_client", client)

    assert StatsRepository().get_by_user_id("u1") == row
    assert client.calls == [("schema", "user_management"), ("table", "player_stats")]
    assert ("eq", ("user_id", "u1"), {}) in query.calls
    assert ("limit", (1,), {}) in query.calls


def test_stats_repository_returns_none_when_player_row_is_missing(monkeypatch):
    monkeypatch.setattr(stats_repository, "admin_client", FakeAdmin([Query([])]))
    assert StatsRepository().get_by_user_id("missing") is None


def test_stats_repository_creates_default_stats(monkeypatch):
    inserted = {"user_id": "u1", "coin": 0, "work_days": 0,
                "work_streak": 0, "total_customers_served": 0}
    insert_query = Query([inserted])
    read_query = Query([inserted])
    monkeypatch.setattr(stats_repository, "admin_client", FakeAdmin([insert_query, read_query]))
    result = StatsRepository().create_stats("u1")
    assert result == inserted
    assert ("insert", ({"user_id": "u1", "coin": 0, "work_days": 0,
                        "work_streak": 0, "total_customers_served": 0},), {}) in insert_query.calls


def test_stats_repository_updates_after_workday(monkeypatch):
    updated = {"user_id": "u1", "coin": 9, "work_days": 2,
               "total_customers_served": 4}
    query = Query([updated])
    monkeypatch.setattr(stats_repository, "admin_client", FakeAdmin([query]))
    assert StatsRepository().update_after_workday("u1", 9, 2, 4) == updated
    assert ("eq", ("user_id", "u1"), {}) in query.calls


def test_stats_repository_propagates_database_errors(monkeypatch):
    monkeypatch.setattr(stats_repository, "admin_client", FakeAdmin([Query(error=RuntimeError("db down"))]))
    with pytest.raises(RuntimeError, match="db down"):
        StatsRepository().get_by_user_id("u1")


def test_stats_create_if_missing_fails_if_insert_cannot_be_confirmed(monkeypatch):
    repository = StatsRepository()
    monkeypatch.setattr(repository, "get_by_user_id", lambda user_id: None)
    monkeypatch.setattr(repository, "create_stats", lambda user_id: None)
    with pytest.raises(RuntimeError, match="not found after creation"):
        repository.create_if_not_exists("u1")
