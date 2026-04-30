using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class AuxSocketSlot : MonoBehaviour
{
    [SerializeField] private string slotId = "SlotA";

    public AuxEnd? OccupiedBy { get; private set; }

    private UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor socket;

    void Awake()
    {
        socket = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor>();
        if (socket.attachTransform != null)
        {
            socket.attachTransform.localRotation = Quaternion.Euler(0, 0, 0);
        }

        socket.selectEntered.AddListener(OnSelectEntered);
        socket.selectExited.AddListener(OnSelectExited);
    }

    private void OnSelectEntered(SelectEnterEventArgs args)
    {
        var id = args.interactableObject.transform.GetComponentInChildren<AuxPlugEndId>();
        if (id == null) return;

        OccupiedBy = id.end;

        var cable = args.interactableObject.transform.GetComponentInParent<AuxCable>();
        if (cable != null)
            cable.NotifyPlugged(id.end, this);
            AuxSocketSlotEvents.OnPlugConnected?.Invoke(cable, this, id.end);

        Debug.Log($"{id.end} plug inserted into {slotId}");
    }

    private void OnSelectExited(SelectExitEventArgs args)
    {
        var id = args.interactableObject.transform.GetComponentInChildren<AuxPlugEndId>();
        if (id != null)
        {
            var cable = args.interactableObject.transform.GetComponentInParent<AuxCable>();
            if (cable != null)
                cable.NotifyUnplugged(id.end);
                AuxSocketSlotEvents.OnPlugDisconnected?.Invoke(cable, this, id.end);    
        }

        OccupiedBy = null;
        Debug.Log($"Plug removed from {slotId}");
    }

    public string SlotId => slotId;
}
