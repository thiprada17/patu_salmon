from infrastructure.supabase_client import admin_client

class StatsRepository:
    # อ้างอิงตาราง player_stats ใน schema user_management
    def _stats(self):
        return admin_client.schema("user_management").table("player_stats")

    # อ่านสถิติของผู้เล่นจาก user_id
    def get_by_user_id(self, user_id: str):
        result = (
            self._stats()
            .select(
                "user_id, coin, work_days, work_streak, "
                "total_customers_served"
            )
            .eq("user_id", user_id)
            .limit(1)
            .execute()
        )
        return result.data[0] if result.data else None

    # สร้างข้อมูลเริ่มต้นให้ผู้เล่น หากยังไม่มีข้อมูล
    def create_stats(self, user_id: str):
        self._stats().insert({
            "user_id": user_id,
            "coin": 0,
            "work_days": 0,
            "work_streak": 0,
            "total_customers_served": 0,
        }).execute()

    # ป้องกันการสร้าง player_stats ซ้ำ
    def create_if_not_exists(self, user_id: str):
        existing = self.get_by_user_id(user_id)

        if existing:
            return existing

        self.create_stats(user_id)
        return self.get_by_user_id(user_id)

    # บันทึกสถิติใหม่หลังจบวันทำงาน
    def update_after_workday(
        self,
        user_id: str,
        coin: int,
        work_days: int,
        total_customers_served: int,
    ):
        result = (
            self._stats()
            .update({
                "coin": coin,
                "work_days": work_days,
                "total_customers_served": total_customers_served,
            })
            .eq("user_id", user_id)
            .execute()
        )

        return result.data[0] if result.data else None
