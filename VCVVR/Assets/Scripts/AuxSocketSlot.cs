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

        // XRI interactor events include Select Entered/Select Exited. 【9-008332】
        socket.selectEntered.AddListener(OnSelectEntered);
        socket.selectExited.AddListener(OnSelectExited);
    }

    private void OnSelectEntered(SelectEnterEventArgs args)
    {
        // Grab which plug-end was inserted
        var id = args.interactableObject.transform.GetComponentInChildren<AuxPlugEndId>();
        if (id == null) return;

        OccupiedBy = id.end;
        Debug.Log($"{id.end} plug inserted into {slotId}");
    }

    private void OnSelectExited(SelectExitEventArgs args)
    {
        OccupiedBy = null;
        Debug.Log($"Plug removed from {slotId}");
    }
}