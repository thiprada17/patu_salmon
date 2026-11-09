
import logging

from supabase import create_client, ClientOptions

from core.config import (
    SUPABASE_URL,
    SUPABASE_KEY,
    SUPABASE_SERVICE_ROLE_KEY,
)

logger = logging.getLogger(__name__)

if SUPABASE_URL and SUPABASE_KEY and SUPABASE_SERVICE_ROLE_KEY:
    # Create clients only when Supabase is configured. This lets mock/dev API
    # routes and test collection start without Apple or Supabase credentials.
    auth_client = create_client(SUPABASE_URL, SUPABASE_KEY)
    admin_client = create_client(
        SUPABASE_URL,
        SUPABASE_SERVICE_ROLE_KEY,
        options=ClientOptions(auto_refresh_token=False, persist_session=False),
    )
    db_client = create_client(SUPABASE_URL, SUPABASE_KEY)
else:
    auth_client = None
    admin_client = None
    db_client = None
    logger.warning("Supabase is not configured; database-backed routes will return unavailable errors.")

# กำหนด client หลักสำหรับโมดูลที่ import ไปใช้งาน
supabase = auth_client
