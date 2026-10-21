using UnityEngine;

public class PlaneFlight : MonoBehaviour
{
    [SerializeField] private RectTransform plane;   // ลาก plane_animation

    [Header("เอียงหัวทิ่ม (หมุนอย่างเดียว ไม่ยุ่งตำแหน่ง)")]
    [SerializeField] private float baseTilt = -15f;   // ติดลบ = ก้มหัว
    [SerializeField] private float tiltSwing = 8f;
    [SerializeField] private float tiltSpeed = 2.5f;

    private bool active;
    private float timer;

    public void Begin()
    {
        timer = 0f;
        active = true;
    }

    public void End()
    {
        active = false;

        if (plane != null)
            plane.localRotation = Quaternion.identity;
    }

    private void LateUpdate()
    {
        if (!active || plane == null) return;

        timer += Time.unscaledDeltaTime;

        float rot = baseTilt + Mathf.Sin(timer * tiltSpeed) * tiltSwing;
        plane.localRotation = Quaternion.Euler(0f, 0f, rot);
    }
}