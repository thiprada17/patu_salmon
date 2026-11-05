from core.settings import get_settings

settings = get_settings()

SUPABASE_URL = settings.supabase_url or None
SUPABASE_ANON_KEY = settings.supabase_anon_key.get_secret_value() or None
SUPABASE_KEY = SUPABASE_ANON_KEY
SUPABASE_SERVICE_ROLE_KEY = settings.supabase_service_role_key.get_secret_value() or None
DEEPLINK_URL = "platusalmon://login-callback"
