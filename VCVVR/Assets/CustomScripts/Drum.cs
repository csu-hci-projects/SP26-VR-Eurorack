using UnityEngine;

public class Drum : MonoBehaviour
{
    [Header("Triggers (DRUM_T_01 .. DRUM_T_05)")]
    public AuxSocketSlot trig1;
    public AuxSocketSlot trig2;
    public AuxSocketSlot trig3;
    public AuxSocketSlot trig4;
    public AuxSocketSlot trig5;

    [Header("Outputs (DRUM_OUT_01 .. DRUM_OUT_05 + L/R)")]
    public AuxSocketSlot out1;
    public AuxSocketSlot out2;
    public AuxSocketSlot out3;
    public AuxSocketSlot out4;
    public AuxSocketSlot out5;
    public AuxSocketSlot outL;
    public AuxSocketSlot outR;

    private float[] amp   = new float[5];
    private float[] freq  = new float[] { 60f, 120f, 200f, 400f, 800f };
    private float[] phase = new float[5];
    private bool[]  lastTrig = new bool[5];

    void Update()
    {
        AuxSocketSlot[] trigs = { trig1, trig2, trig3, trig4, trig5 };
        AuxSocketSlot[] outs  = { out1,  out2,  out3,  out4,  out5  };

        float mixL = 0f;
        float mixR = 0f;

        for (int i = 0; i < 5; i++)
        {
            if (trigs[i] != null && trigs[i].OccupiedBy != null)
            {
                bool trig = PatchRouter.Instance.ReadValue(trigs[i].SlotId) > 0.5f;
                if (trig && !lastTrig[i]) amp[i] = 1f;
                lastTrig[i] = trig;
            }

            amp[i] = Mathf.Max(0f, amp[i] - Time.deltaTime * 8f);

            phase[i] += freq[i] * Time.deltaTime;
            phase[i] %= 1f;

            float sample = amp[i] * Mathf.Sin(phase[i] * 2f * Mathf.PI);

            if (outs[i] != null && outs[i].OccupiedBy != null)
                PatchRouter.Instance.SendValue(outs[i].SlotId, sample);

            mixL += sample;
            mixR += sample;
        }

        mixL = Mathf.Clamp(mixL * 0.2f, -1f, 1f);
        mixR = Mathf.Clamp(mixR * 0.2f, -1f, 1f);

        if (outL != null && outL.OccupiedBy != null)
            PatchRouter.Instance.SendValue(outL.SlotId, mixL);
        if (outR != null && outR.OccupiedBy != null)
            PatchRouter.Instance.SendValue(outR.SlotId, mixR);
    }
}