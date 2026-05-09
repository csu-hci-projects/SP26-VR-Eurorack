using UnityEngine;

public class Seq8 : MonoBehaviour
{
    [Header("Inputs")]
    public AuxSocketSlot clockIn;
    public AuxSocketSlot resetIn;

    [Header("Outputs")]
    public AuxSocketSlot cvOut1;
    public AuxSocketSlot cvOut2;
    public AuxSocketSlot cvOut3;
    public AuxSocketSlot trigOut;

    [Header("Step CV1 (0–1)")]
    public KnobReporter[] stepCV1 = new KnobReporter[8];

    [Header("Step CV2 (0–1)")]
    public KnobReporter[] stepCV2 = new KnobReporter[8];

    [Header("Step CV3 (0–1)")]
    public KnobReporter[] stepCV3 = new KnobReporter[8];

    [Header("Step Gates (toggles)")]
    public bool[] stepGate = new bool[8];

    private int currentStep = 0;
    private bool lastClock = false;

    // Short trigger pulse timer
    private float trigTimer = 0f;

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

            // Start a short trigger pulse
            trigTimer = 0.01f; // 10ms pulse
        }
        lastClock = clock;

        // --- RESET ---
        if (resetIn != null && resetIn.OccupiedBy != null)
        {
            if (PatchRouter.Instance.ReadValue(resetIn.SlotId) > 0.5f)
                currentStep = 0;
        }

        // --- OUTPUT CV1 ---
        if (cvOut1 != null && cvOut1.OccupiedBy != null)
        {
            float cv = stepCV1[currentStep] != null ? stepCV1[currentStep].Value01 : 0f;
            PatchRouter.Instance.SendValue(cvOut1.SlotId, cv);
        }

        // --- OUTPUT CV2 ---
        if (cvOut2 != null && cvOut2.OccupiedBy != null)
        {
            float cv = stepCV2[currentStep] != null ? stepCV2[currentStep].Value01 : 0f;
            PatchRouter.Instance.SendValue(cvOut2.SlotId, cv);
        }

        // --- OUTPUT CV3 ---
        if (cvOut3 != null && cvOut3.OccupiedBy != null)
        {
            float cv = stepCV3[currentStep] != null ? stepCV3[currentStep].Value01 : 0f;
            PatchRouter.Instance.SendValue(cvOut3.SlotId, cv);
        }

        // --- OUTPUT SHORT TRIGGER PULSE ---
        float trigValue = trigTimer > 0f ? 1f : 0f;
        trigTimer -= Time.deltaTime;

        if (trigOut != null && trigOut.OccupiedBy != null)
            PatchRouter.Instance.SendValue(trigOut.SlotId, trigValue);
    }
}
