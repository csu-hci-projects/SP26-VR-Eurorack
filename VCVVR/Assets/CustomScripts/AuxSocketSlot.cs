using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using System;
using System.Text.RegularExpressions;

public class AuxSocketSlot : MonoBehaviour
{
    [SerializeField] private string slotId = "SlotA";

    public AuxEnd? OccupiedBy { get; private set; }
    public Jack ConnectedJack { get; private set; }
    public static WireManager WireManager;   // set once from WireManager.Awake()

    private UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor socket;

    // ── Jack events ──────────────────────────────────────────────────────────
    /// <summary>Fired when a Jack plugs in. Args: (slot, jack)</summary>
    public static event Action<AuxSocketSlot, Jack> OnJackConnected;

    /// <summary>Fired when a Jack unplugs. Args: (slot, jack)</summary>
    public static event Action<AuxSocketSlot, Jack> OnJackDisconnected;

    // Regex: Jack_M{moduleId}_P{portId}_{In|Out}
    // Example: "Jack_M1_P3_Out" → moduleId=1, portId=3, isOutput=true
    private static readonly Regex JackNamePattern =
        new Regex(@"Jack_M(\d+)_P(\d+)_(In|Out)", RegexOptions.IgnoreCase);

    void Awake()
    {
        socket = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor>();
        if (socket.attachTransform != null)
            socket.attachTransform.localRotation = Quaternion.Euler(0, 0, 0);

        socket.selectEntered.AddListener(OnSelectEntered);
        socket.selectExited.AddListener(OnSelectExited);
    }

    // ── Connect ───────────────────────────────────────────────────────────────
    private void OnSelectEntered(SelectEnterEventArgs args)
    {
        var id = args.interactableObject.transform.GetComponentInChildren<AuxPlugEndId>();
        if (id == null) return;

        OccupiedBy = id.end;

        var cable = args.interactableObject.transform.GetComponentInParent<AuxCable>();
        if (cable != null)
            cable.NotifyPlugged(id.end, this);

        AuxSocketSlotEvents.OnPlugConnected?.Invoke(cable, this, id.end);

        // ── Jack detection ────────────────────────────────────────────────────
        Jack jack = TryGetOrParseJack(args.interactableObject.transform);
        if (jack != null)
        {
            ConnectedJack = jack;
            Debug.Log($"[AuxSocketSlot] {slotId} ← Jack detected: " +
                      $"moduleId={jack.moduleId}, portId={jack.portId}, isOutput={jack.isOutput}");
            OnJackConnected?.Invoke(this, jack);
        }

            if (jack != null)
    {
        ConnectedJack = jack;
        OnJackConnected?.Invoke(this, jack);
        WireManager?.ClickJack(jack);         // ← this actually routes the signal
    }

        if (ConnectedJack != null)
{
    OnJackDisconnected?.Invoke(this, ConnectedJack);
    WireManager?.DisconnectJack(ConnectedJack);   // ← tears down the route
    ConnectedJack = null;
}

        Debug.Log($"{id.end} plug inserted into {slotId}");
    }

    // ── Disconnect ────────────────────────────────────────────────────────────
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

        // ── Jack disconnect ───────────────────────────────────────────────────
        if (ConnectedJack != null)
        {
            Debug.Log($"[AuxSocketSlot] {slotId} → Jack disconnected: " +
                      $"moduleId={ConnectedJack.moduleId}, portId={ConnectedJack.portId}");
            OnJackDisconnected?.Invoke(this, ConnectedJack);
            ConnectedJack = null;
        }

        OccupiedBy = null;
        Debug.Log($"Plug removed from {slotId}");
    }

    // ── Jack resolution ───────────────────────────────────────────────────────
    /// <summary>
    /// First checks for an existing Jack component anywhere on the interactable.
    /// If none found, tries to parse one from the GameObject name and auto-assigns it.
    /// </summary>
    private Jack TryGetOrParseJack(Transform interactableRoot)
    {
        // 1. Already has a Jack component?
        Jack jack = interactableRoot.GetComponentInChildren<Jack>();
        if (jack != null) return jack;

        // 2. Try to parse from name convention: Jack_M{n}_P{n}_{In|Out}
        //    Search the interactable and all its children for a matching name
        foreach (Transform t in interactableRoot.GetComponentsInChildren<Transform>())
        {
            Match m = JackNamePattern.Match(t.gameObject.name);
            if (!m.Success) continue;

            jack = t.gameObject.GetComponent<Jack>();
            if (jack == null)
                jack = t.gameObject.AddComponent<Jack>();

            jack.moduleId  = int.Parse(m.Groups[1].Value);
            jack.portId    = int.Parse(m.Groups[2].Value);
            jack.isOutput  = m.Groups[3].Value.Equals("Out", StringComparison.OrdinalIgnoreCase);

            Debug.Log($"[AuxSocketSlot] Auto-assigned Jack on '{t.gameObject.name}': " +
                      $"M{jack.moduleId} P{jack.portId} {(jack.isOutput ? "Out" : "In")}");
            return jack;
        }

        return null;
    }

    public string SlotId => slotId;
}