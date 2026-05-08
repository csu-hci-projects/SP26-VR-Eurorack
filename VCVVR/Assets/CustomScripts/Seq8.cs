using UnityEngine;

public class Seq8 : MonoBehaviour
{
    [Header("Inputs")]
    public AuxSocketSlot clockIn;
    public AuxSocketSlot resetIn;

    [Header("Outputs")]
    public AuxSocketSlot cvOut;
    public AuxSocketSlot gateOut;

    [Header("Step Knobs (0–1)")]
    public KnobReporter[] stepCV = new KnobReporter[8];

    [Header("Step Gates (toggles)")]
    public bool[] stepGate = new bool[8];

    private int currentStep = 0;
    private bool lastClock = false;

    void Update()
    {
        // --- READ CLOCK ---
        bool clock = false;
        if (clockIn != null && clockIn.OccupiedBy != null)
            clock = PatchRouter.Instance.ReadValue(clockIn.SlotId) > 0.5f;

        // Rising edge → advance step
        if (clock && !lastClock)
        {
            currentStep = (currentStep + 1) % 8;
        }
        lastClock = clock;

        // --- RESET ---
        if (resetIn != null && resetIn.OccupiedBy != null)
        {
            if (PatchRouter.Instance.ReadValue(resetIn.SlotId) > 0.5f)
                currentStep = 0;
        }

        // --- OUTPUT CV ---
        if (cvOut != null && cvOut.OccupiedBy != null)
        {
            float cv = stepCV[currentStep] != null ? stepCV[currentStep].Value01 : 0f;
            PatchRouter.Instance.SendValue(cvOut.SlotId, cv);
        }

        // --- OUTPUT GATE ---
        if (gateOut != null && gateOut.OccupiedBy != null)
        {
            float g = stepGate[currentStep] ? 1f : 0f;
            PatchRouter.Instance.SendValue(gateOut.SlotId, g);
        }
    }
}
