using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using System;
using System.Text.RegularExpressions;
using Unity.Mathematics;

public class AuxSocketSlot : MonoBehaviour
{
    [SerializeField] private string slotId = "SlotA";

    public AuxEnd? OccupiedBy { get; private set; }
    public Jack ConnectedJack { get; private set; }
    public static WireManager WireManager;

    private UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor socket;

    public static event Action<AuxSocketSlot, Jack> OnJackConnected;
    public static event Action<AuxSocketSlot, Jack> OnJackDisconnected;

    private static readonly Regex JackNamePattern =
        new Regex(@"Jack_M(\d+)_P(\d+)_(In|Out)", RegexOptions.IgnoreCase);

void Awake()
{
    socket = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor>();

    // Removed: do NOT touch attachTransform at runtime
    // The editor tool (AuxSocketSetup) sets this up correctly in the scene

    socket.selectEntered.AddListener(OnSelectEntered);
    socket.selectExited.AddListener(OnSelectExited);
}

    private void OnSelectEntered(SelectEnterEventArgs args)
    {
        var id = args.interactableObject.transform.GetComponentInChildren<AuxPlugEndId>();
        if (id == null) return;

        OccupiedBy = id.end;

        Debug.Log($"[AuxSocketSlot] {slotId} entered end={id.end} cable={id.cable != null}");

        if (id.cable != null)
            id.cable.NotifyPlugged(id.end, this);

        AuxSocketSlotEvents.OnPlugConnected?.Invoke(id.cable, this, id.end);

        Jack jack = TryGetOrParseJack(args.interactableObject.transform);
        if (jack != null)
        {
            ConnectedJack = jack;
            OnJackConnected?.Invoke(this, jack);
            WireManager?.ClickJack(jack);
        }

        Debug.Log($"{id.end} plug inserted into {slotId}");
    }

    private void OnSelectExited(SelectExitEventArgs args)
    {
        var id = args.interactableObject.transform.GetComponentInChildren<AuxPlugEndId>();
        if (id != null)
        {
            if (id.cable != null)
                id.cable.NotifyUnplugged(id.end);

            AuxSocketSlotEvents.OnPlugDisconnected?.Invoke(id.cable, this, id.end);
        }

        if (ConnectedJack != null)
        {
            OnJackDisconnected?.Invoke(this, ConnectedJack);
            ConnectedJack = null;
        }

        OccupiedBy = null;
        Debug.Log($"Plug removed from {slotId}");
    }

    private Jack TryGetOrParseJack(Transform interactableRoot)
    {
        Jack jack = interactableRoot.GetComponentInChildren<Jack>();
        if (jack != null) return jack;

        foreach (Transform t in interactableRoot.GetComponentsInChildren<Transform>())
        {
            Match m = JackNamePattern.Match(t.gameObject.name);
            if (!m.Success) continue;

            jack = t.gameObject.GetComponent<Jack>();
            if (jack == null)
                jack = t.gameObject.AddComponent<Jack>();

            jack.moduleId = int.Parse(m.Groups[1].Value);
            jack.portId = int.Parse(m.Groups[2].Value);
            jack.isOutput = m.Groups[3].Value.Equals("Out", StringComparison.OrdinalIgnoreCase);
            return jack;
        }
        return null;
    }

    public string SlotId => slotId;
}