
from supabase import create_client, ClientOptions

from core.config import (
    SUPABASE_URL,
    SUPABASE_KEY,
    SUPABASE_SERVICE_ROLE_KEY,
)

# ใช้กับ Auth ปกติ
auth_client = create_client(
    SUPABASE_URL,
    SUPABASE_KEY,
)

# ใช้ตรวจบัญชีผ่าน Admin API และจัดการข้อมูลฝั่ง Server
admin_client = create_client(
    SUPABASE_URL,
    SUPABASE_SERVICE_ROLE_KEY,
    options=ClientOptions(
        auto_refresh_token=False,
        persist_session=False,
    ),
)

# ใช้ติดต่อ Database
db_client = create_client(
    SUPABASE_URL,
    SUPABASE_KEY,
)

# กำหนด client หลักสำหรับโมดูลที่ import ไปใช้งาน
supabase = auth_client


print("SUPABASE_URL:", SUPABASE_URL)
print("SUPABASE_KEY loaded:", bool(SUPABASE_KEY))
print(
    "SUPABASE_SERVICE_ROLE_KEY loaded:",
    bool(SUPABASE_SERVICE_ROLE_KEY)
)
print(
    "Keys are identical:",
    SUPABASE_KEY == SUPABASE_SERVICE_ROLE_KEY
)