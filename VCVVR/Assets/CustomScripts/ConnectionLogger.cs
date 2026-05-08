using UnityEngine;

public class ConnectionLogger : MonoBehaviour
{
    private void OnEnable()
    {
        AuxSocketSlotEvents.OnPlugConnected += HandleConnected;
        AuxSocketSlotEvents.OnPlugDisconnected += HandleDisconnected;
    }

    private void OnDisable()
    {
        AuxSocketSlotEvents.OnPlugConnected -= HandleConnected;
        AuxSocketSlotEvents.OnPlugDisconnected -= HandleDisconnected;
    }

    private void HandleConnected(AuxCable cable, AuxSocketSlot slot, AuxEnd end)
    {
        if (slot == null) return;
        Debug.Log($"[PATCH] {end} plug connected to {slot.SlotId}");

        if (cable != null && cable.leftSocket != null && cable.rightSocket != null)
            Debug.Log($"[PATCH COMPLETE] {cable.leftSocket.SlotId} → {cable.rightSocket.SlotId}");
    }

    private void HandleDisconnected(AuxCable cable, AuxSocketSlot slot, AuxEnd end)
    {
        if (slot == null) return;
        Debug.Log($"[UNPATCH] {end} plug removed from {slot.SlotId}");
    }
}