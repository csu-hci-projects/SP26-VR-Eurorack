using UnityEngine;

public class AuxCable : MonoBehaviour
{
    public AuxSocketSlot leftSocket;
    public AuxSocketSlot rightSocket;

    public bool IsFullyPatched =>
        leftSocket != null && rightSocket != null;

    public void NotifyPlugged(AuxEnd end, AuxSocketSlot slot)
    {
        if (end == AuxEnd.Left)
            leftSocket = slot;
        else
            rightSocket = slot;

        if (IsFullyPatched && PatchRouter.Instance != null)
            PatchRouter.Instance.ConnectSlots(leftSocket.SlotId, rightSocket.SlotId);
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
