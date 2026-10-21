using UnityEngine;

public static class MachineShakeManager
{
    public static void Shake(GameObject machine)
    {
        if (machine == null) return;

        MachineShake shaker = machine.GetComponent<MachineShake>();

        if (shaker == null)
            shaker = machine.AddComponent<MachineShake>();

        shaker.Shake();
    }
}