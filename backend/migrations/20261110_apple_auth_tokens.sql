-- Stores only the encrypted Apple refresh token needed to revoke Sign in with
-- Apple consent when a user deletes their account. Only service_role can use it.
create table if not exists user_management.apple_auth_tokens (
    user_id uuid primary key references auth.users (id) on delete cascade,
    refresh_token_encrypted text not null,
    client_id text not null,
    updated_at timestamptz not null default now()
);

alter table user_management.apple_auth_tokens enable row level security;
revoke all on table user_management.apple_auth_tokens from anon, authenticated;
grant all on table user_management.apple_auth_tokens to service_role;
