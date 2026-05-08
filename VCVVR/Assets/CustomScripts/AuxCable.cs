using UnityEngine;

public class AuxCable : MonoBehaviour
{
    public AuxSocketSlot leftSocket;
    public AuxSocketSlot rightSocket;

    public bool IsFullyPatched =>
        leftSocket != null && rightSocket != null;

    public void NotifyPlugged(AuxEnd end, AuxSocketSlot slot)
    {
        Debug.Log($"[AuxCable] NotifyPlugged end={end} slot={slot?.SlotId}");

        if (end == AuxEnd.Left)
            leftSocket = slot;
        else
            rightSocket = slot;

        if (IsFullyPatched && PatchRouter.Instance != null)
        {
            Debug.Log($"[AuxCable] Fully patched {leftSocket.SlotId} -> {rightSocket.SlotId}");
            PatchRouter.Instance.ConnectSlots(leftSocket.SlotId, rightSocket.SlotId);
        }
    }

    public void NotifyUnplugged(AuxEnd end)
    {
        if (IsFullyPatched && PatchRouter.Instance != null)
            PatchRouter.Instance.DisconnectSlots(leftSocket.SlotId, rightSocket.SlotId);

        if (end == AuxEnd.Left)
            leftSocket = null;
        else
            rightSocket = null;
    }
}