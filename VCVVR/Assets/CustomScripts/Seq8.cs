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

    [Header("Step CV1 (0-1)")]
    public KnobReporter[] stepCV1 = new KnobReporter[8];

    [Header("Step CV2 (0-1)")]
    public KnobReporter[] stepCV2 = new KnobReporter[8];

    [Header("Step CV3 (0-1)")]
    public KnobReporter[] stepCV3 = new KnobReporter[8];

    [Header("Step Gates (toggles)")]
    public bool[] stepGate = new bool[8];

    [Header("Internal Clock")]
    [Tooltip("BPM used when no clock cable is patched in")]
    public float internalBPM = 120f;
    [Tooltip("If true, sequences without needing a clock patch")]
    public bool useInternalClock = true;

    private int currentStep = 0;
    private bool lastClock = false;
    private float internalClockTimer = 0f;
    private bool internalClockState = false;

    // Short trigger pulse timer
    private float trigTimer = 0f;

    void Update()
    {
        bool clock = false;

        // If a clock cable is patched, use it — otherwise use internal clock
        if (clockIn != null && clockIn.OccupiedBy != null)
        {
            clock = PatchRouter.Instance.ReadValue(clockIn.SlotId) > 0.5f;
        }
        else if (useInternalClock)
        {
            // Internal clock: advance timer and toggle at BPM rate
            float secondsPerBeat = 60f / internalBPM;
            internalClockTimer += Time.deltaTime;
            if (internalClockTimer >= secondsPerBeat * 0.5f)
            {
                internalClockTimer = 0f;
                internalClockState = !internalClockState;
            }
            clock = internalClockState;
        }

        // Rising edge -> advance step
        if (clock && !lastClock)
        {
            currentStep = (currentStep + 1) % 8;
            trigTimer = 0.01f; // 10ms trigger pulse
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

        // --- GATE OUTPUT ---
        // Also output a gate signal matching the current step's gate bool.
        // This lets ADSR open without needing Seq8's trig patched separately.
        float gateValue = (stepGate[currentStep] && clock) ? 1f : 0f;

        // --- TRIGGER PULSE OUTPUT ---
        float trigValue = trigTimer > 0f ? 1f : 0f;
        trigTimer -= Time.deltaTime;

        if (trigOut != null && trigOut.OccupiedBy != null)
            PatchRouter.Instance.SendValue(trigOut.SlotId, trigValue);
    }
}