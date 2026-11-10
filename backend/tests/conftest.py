import sys
from pathlib import Path


BACKEND_ROOT = Path(__file__).resolve().parents[1]
if str(BACKEND_ROOT) not in sys.path:
    sys.path.insert(0, str(BACKEND_ROOT))


_TEST_TITLES = {
    "test_account_deletion_removes_dependent_rows_then_auth_identity": "ลบบัญชีและข้อมูลที่เกี่ยวข้องตามลำดับ",
    "test_account_deletion_fails_closed_when_supabase_unavailable": "ปฏิเสธการลบบัญชีเมื่อ Supabase ใช้งานไม่ได้",
    "test_account_deletion_database_error_is_sanitized": "ซ่อนรายละเอียด error ขณะลบบัญชี",
    "test_auth_dependency_rejects_missing_and_malformed_bearer_headers": "ปฏิเสธ Authorization header ที่หายหรือผิดรูปแบบ",
    "test_player_endpoint_rejects_other_players_data": "ป้องกันการอ่านข้อมูลผู้เล่นคนอื่น",
    "test_player_endpoint_rejects_invalid_uuid_and_missing_required_fields": "ตรวจ UUID และ field บังคับของ Player API",
    "test_player_service_does_not_expose_database_exception_in_response": "ซ่อน database exception จาก Player API",
    "test_health_endpoint_and_legal_pages_are_served": "ตรวจ Health check และหน้า Legal",
    "test_google_auth_provider_failure_returns_sanitized_error": "ซ่อนรายละเอียด error จาก Google provider",
    "test_mock_flow_accepts_only_dev_issuer_and_returns_dev_session": "Apple mock login ใช้ได้เฉพาะ Dev",
    "test_valid_apple_token_uses_jwks_and_checks_claims": "ตรวจ Apple signature และ token claims",
    "test_production_login_exchanges_verified_token_for_supabase_session": "แลก Apple token เป็น Supabase session",
    "test_invalid_audience_returns_401": "ปฏิเสธ Apple token ที่ audience ไม่ตรง",
    "test_production_missing_apple_credentials_returns_500_before_token_validation": "ปฏิเสธ Production เมื่อ Apple secrets ไม่ครบ",
    "test_google_callback_maps_existing_and_new_users": "Google callback รองรับผู้ใช้เดิมและผู้ใช้ใหม่",
    "test_google_callback_rejects_missing_provider_session": "ปฏิเสธ Google response ที่ไม่มี session",
    "test_email_continue_rejects_empty_credentials": "ปฏิเสธ email/password ที่ว่าง",
    "test_refresh_session_rejects_missing_refresh_token": "ปฏิเสธ refresh token ที่หายไป",
    "test_dev_mock_auth_endpoint_returns_mock_user": "Apple Dev endpoint ส่ง mock user/session",
    "test_production_without_apple_private_key_returns_configuration_500": "Apple endpoint แจ้ง config error ใน Production",
    "test_delete_account_removes_supabase_user_and_player_rows": "Integration: ลบบัญชีและแถว Supabase",
    "test_supabase_player_lifecycle_through_backend_api": "Integration: Auth, profile และ player stats ผ่าน Backend กับ Supabase จริง",
    "test_supabase_email_continue_login_and_invalid_credentials": "Integration: Email login จริงและรหัสผ่านไม่ถูกต้อง",
    "test_supabase_email_continue_signs_up_a_new_user": "Integration: สมัครผู้ใช้ใหม่ผ่าน Supabase Auth",
    "test_supabase_auth_rejects_invalid_tokens_and_refresh_requests": "Integration: ปฏิเสธ access/refresh token ที่ไม่ถูกต้อง",
    "test_supabase_player_api_rejects_invalid_or_foreign_requests": "Integration: ปฏิเสธข้อมูลผิดรูปแบบและข้อมูลผู้เล่นคนอื่น",
    "test_supabase_stats_endpoint_is_idempotent_and_persists_workday": "Integration: สร้าง stats ซ้ำและบันทึกวันทำงาน",
    "test_supabase_account_delete_endpoint_removes_auth_and_player_data": "Integration: ลบบัญชี Auth และข้อมูลผู้เล่น",
    "test_stats_repository_reads_expected_schema_table_and_user_row": "อ่าน stats จาก schema/table และ user ID ที่ถูกต้อง",
    "test_stats_repository_returns_none_when_player_row_is_missing": "คืนค่าเมื่อไม่พบแถว player stats",
    "test_stats_repository_creates_default_stats": "สร้าง player stats ค่าเริ่มต้น",
    "test_stats_repository_updates_after_workday": "บันทึก stats หลังจบวันทำงาน",
    "test_stats_repository_propagates_database_errors": "ส่งต่อ database error จาก repository",
    "test_stats_create_if_missing_fails_if_insert_cannot_be_confirmed": "แจ้ง error เมื่อสร้าง stats แล้วยืนยันแถวไม่ได้",
    "test_settings_default_to_dev_with_optional_apple_production_secrets": "ค่าเริ่มต้น Dev และ Apple secrets เป็น optional",
    "test_settings_load_environment_values": "โหลด Settings จาก environment",
    "test_dev_does_not_require_production_credentials": "Dev ทำงานได้โดยไม่ต้องมี Production secrets",
    "test_production_missing_secrets_fails_before_token_validation": "บล็อก Production เมื่อ secrets จำเป็นหายไป",
    "test_deep_link_encodes_tokens_and_preserves_query_values": "Deep link encode token และรักษาค่า query",
}


def pytest_collection_modifyitems(config, items):
    """Attach stable display codes and human-readable names to collected tests."""
    manifest = {}
    for index, item in enumerate(items, start=1):
        test_name = item.originalname or item.name
        title = _TEST_TITLES.get(test_name, test_name.replace("_", " ").capitalize())
        callspec = getattr(item, "callspec", None)
        if test_name == "test_google_callback_maps_existing_and_new_users" and callspec:
            title += " — " + ("profile ครบ" if callspec.params["has_profile"] else "ยังไม่มี profile")
        manifest[item.nodeid] = (f"UT-{index:02d}", title)
    config._platu_test_manifest = manifest


def pytest_terminal_summary(terminalreporter, exitstatus, config):
    """Print a compact, readable test catalog with outcome for every case."""
    if config.option.collectonly:
        return
    manifest = getattr(config, "_platu_test_manifest", {})
    if not manifest:
        return

    results = {}
    for reports in terminalreporter.stats.values():
        for report in reports:
            nodeid = getattr(report, "nodeid", None)
            if nodeid not in manifest:
                continue
            if getattr(report, "failed", False):
                results[nodeid] = ("FAIL", "red")
            elif getattr(report, "skipped", False):
                results.setdefault(nodeid, ("SKIP", "yellow"))
            elif getattr(report, "when", None) == "call" and getattr(report, "passed", False):
                results[nodeid] = ("PASS", "green")

    for nodeid, (test_id, title) in manifest.items():
        result, _ = results.get(nodeid, ("NOT RUN", "yellow"))
        terminalreporter.write_line(f"{test_id:<8} {result:<8} {title}")


def pytest_report_teststatus(report, config):
    """Hide pytest's default dots so output contains only named test rows."""
    if report.when == "call":
        if report.passed:
            return "passed", "", ""
        if report.failed:
            return "failed", "", ""
        if report.skipped:
            return "skipped", "", ""
