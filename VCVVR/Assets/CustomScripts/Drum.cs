using UnityEngine;

public class Drum : MonoBehaviour
{
    [Header("Triggers")]
    public AuxSocketSlot trig1;
    public AuxSocketSlot trig2;
    public AuxSocketSlot trig3;
    public AuxSocketSlot trig4;
    public AuxSocketSlot trig5;

    [Header("Outputs")]
    public AuxSocketSlot out1;
    public AuxSocketSlot out2;
    public AuxSocketSlot out3;
    public AuxSocketSlot out4;
    public AuxSocketSlot out5;
    public AuxSocketSlot outL;
    public AuxSocketSlot outR;

    [Header("Self Trigger (when no trigger patched)")]
    [Tooltip("If true, drums fire on their own internal timer when no trigger is patched")]
    public bool selfTrigger = true;
    public float selfTriggerBPM = 120f;

    private float[] amp      = new float[5];
    private float[] freq     = new float[] { 60f, 120f, 200f, 400f, 800f };
    private float[] phase    = new float[5];
    private bool[]  lastTrig = new bool[5];
    private float[] selfTrigTimer = new float[5];
    private int debugFrame = 0;

    void Update()
    {
        AuxSocketSlot[] trigs = { trig1, trig2, trig3, trig4, trig5 };
        AuxSocketSlot[] outs  = { out1,  out2,  out3,  out4,  out5  };

        float mixL = 0f;
        float mixR = 0f;

        float selfTrigInterval = 60f / selfTriggerBPM;

        for (int i = 0; i < 5; i++)
        {
            if (trigs[i] != null && trigs[i].OccupiedBy != null)
            {
                // Normal trigger from patch
                bool trig = PatchRouter.Instance.ReadValue(trigs[i].SlotId) > 0.5f;
                if (trig && !lastTrig[i]) amp[i] = 1f;
                lastTrig[i] = trig;
            }
            else if (selfTrigger && i == 0)
            {
                // Self trigger only on drum 1 when nothing patched
                selfTrigTimer[i] += Time.deltaTime;
                if (selfTrigTimer[i] >= selfTrigInterval)
                {
                    selfTrigTimer[i] = 0f;
                    amp[i] = 1f;
                }
            }

            amp[i]   = Mathf.Max(0f, amp[i] - Time.deltaTime * 8f);
            phase[i] += freq[i] * Time.deltaTime;
            phase[i] %= 1f;

            float sample = amp[i] * Mathf.Sin(phase[i] * 2f * Mathf.PI);

            // Always write individual outputs
            if (outs[i] != null)
                PatchRouter.Instance.SendValue(outs[i].SlotId, sample);

            mixL += sample;
            mixR += sample;
        }

        mixL = Mathf.Clamp(mixL * 0.2f, -1f, 1f);
        mixR = Mathf.Clamp(mixR * 0.2f, -1f, 1f);

        if (debugFrame % 60 == 0)
            Debug.Log($"[Drum] mixL={mixL:F4} outL slot={outL?.SlotId} occupied={outL?.OccupiedBy != null}");

        // Always write mix outputs
        if (outL != null)
            PatchRouter.Instance.SendValue(outL.SlotId, mixL);
        if (outR != null)
            PatchRouter.Instance.SendValue(outR.SlotId, mixR);

        debugFrame++;
    }
}