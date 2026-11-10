from fastapi import APIRouter, Depends, HTTPException

from api.v1.AuthRouter import get_authenticated_user
from services.account_service import AccountService


router = APIRouter(prefix="/user", tags=["User"])


@router.delete("/account")
def delete_account(
    current_user: dict = Depends(get_authenticated_user),
    service: AccountService = Depends(AccountService),
):
    if current_user.get("mock_mode"):
        raise HTTPException(
            status_code=400,
            detail="Mock development accounts cannot be deleted from Supabase",
        )

    user_id = str(current_user.get("user_id", ""))
    if not user_id:
        raise HTTPException(status_code=401, detail="Authenticated user ID is missing")

    providers = current_user.get("providers") or []
    return service.delete_account(user_id, revoke_apple="apple" in providers)
