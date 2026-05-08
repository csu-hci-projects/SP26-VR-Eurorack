using UnityEngine;

public class VCO : MonoBehaviour
{
    [Header("Inputs")]
    public AuxSocketSlot vOctSlot;
    public AuxSocketSlot vOct2Slot;
    public AuxSocketSlot pwSlot;
    public AuxSocketSlot pw2Slot;
    public AuxSocketSlot fmSlot;
    public AuxSocketSlot rmSlot;
    public AuxSocketSlot syncSlot;
    public AuxSocketSlot shapeSlot;
    public AuxSocketSlot shape2Slot;
    public AuxSocketSlot volSlot;
    public AuxSocketSlot vol2Slot;

    [Header("Outputs")]
    public AuxSocketSlot outSlot;
    public AuxSocketSlot out2Slot;
    public AuxSocketSlot subSlot;

    [Header("Knobs")]
    public KnobReporter shapeKnob;
    public KnobReporter pwKnob;
    public KnobReporter volKnob;
    public KnobReporter subKnob;
    public KnobReporter rmKnob;
    public KnobReporter fmKnob;
    public KnobReporter shape2Knob;
    public KnobReporter pw2Knob;
    public KnobReporter vol2Knob;

    public float baseFrequency = 440f;

    private float phase = 0f;
    private float phase2 = 0f;
    private float subPhase = 0f;

    void Update()
    {
        float freq = baseFrequency;

        if (vOctSlot != null && vOctSlot.OccupiedBy != null)
            freq *= Mathf.Pow(2f, PatchRouter.Instance.ReadValue(vOctSlot.SlotId) * 5f);

        if (fmSlot != null && fmSlot.OccupiedBy != null)
        {
            float fmAmt = fmKnob != null ? Mathf.Lerp(0f, 500f, fmKnob.Value01) : 200f;
            freq += PatchRouter.Instance.ReadValue(fmSlot.SlotId) * fmAmt;
        }

        freq = Mathf.Clamp(freq, 20f, 20000f);

        float pw = pwKnob != null ? Mathf.Lerp(0.05f, 0.95f, pwKnob.Value01) : 0.5f;
        if (pwSlot != null && pwSlot.OccupiedBy != null)
            pw = Mathf.Clamp01(PatchRouter.Instance.ReadValue(pwSlot.SlotId));

        float vol = volKnob != null ? volKnob.Value01 : 1f;
        if (volSlot != null && volSlot.OccupiedBy != null)
            vol = Mathf.Clamp01(PatchRouter.Instance.ReadValue(volSlot.SlotId));

        float rm = rmKnob != null ? rmKnob.Value01 : 1f;
        if (rmSlot != null && rmSlot.OccupiedBy != null)
            rm = PatchRouter.Instance.ReadValue(rmSlot.SlotId);

        if (syncSlot != null && syncSlot.OccupiedBy != null)
            if (PatchRouter.Instance.ReadValue(syncSlot.SlotId) > 0.5f)
                phase = 0f;

        phase += freq * Time.deltaTime;
        phase %= 1f;
        subPhase += (freq * 0.5f) * Time.deltaTime;
        subPhase %= 1f;

        float sine   = Mathf.Sin(phase * 2f * Mathf.PI);
        float saw    = 2f * phase - 1f;
        float square = phase < pw ? 1f : -1f;
        float tri    = 1f - Mathf.Abs((phase * 4f + 3f) % 4f - 2f);

        float shapeAmt = shapeKnob != null ? shapeKnob.Value01 : 0f;
        if (shapeSlot != null && shapeSlot.OccupiedBy != null)
            shapeAmt = PatchRouter.Instance.ReadValue(shapeSlot.SlotId);

        float output = shapeAmt < 0.33f
            ? Mathf.Lerp(sine, tri, shapeAmt * 3f)
            : shapeAmt < 0.66f
                ? Mathf.Lerp(tri, square, (shapeAmt - 0.33f) * 3f)
                : Mathf.Lerp(square, saw, (shapeAmt - 0.66f) * 3f);

        output *= vol * rm;

        if (outSlot != null && outSlot.OccupiedBy != null)
            PatchRouter.Instance.SendValue(outSlot.SlotId, output);

        if (subSlot != null && subSlot.OccupiedBy != null)
            PatchRouter.Instance.SendValue(subSlot.SlotId, Mathf.Sin(subPhase * 2f * Mathf.PI) * vol);

        float freq2 = baseFrequency;
        if (vOct2Slot != null && vOct2Slot.OccupiedBy != null)
            freq2 *= Mathf.Pow(2f, PatchRouter.Instance.ReadValue(vOct2Slot.SlotId) * 5f);

        float pw2 = pw2Knob != null ? Mathf.Lerp(0.05f, 0.95f, pw2Knob.Value01) : 0.5f;
        if (pw2Slot != null && pw2Slot.OccupiedBy != null)
            pw2 = Mathf.Clamp01(PatchRouter.Instance.ReadValue(pw2Slot.SlotId));

        float vol2 = vol2Knob != null ? vol2Knob.Value01 : 1f;
        if (vol2Slot != null && vol2Slot.OccupiedBy != null)
            vol2 = Mathf.Clamp01(PatchRouter.Instance.ReadValue(vol2Slot.SlotId));

        float shape2Amt = shape2Knob != null ? shape2Knob.Value01 : 0f;
        if (shape2Slot != null && shape2Slot.OccupiedBy != null)
            shape2Amt = PatchRouter.Instance.ReadValue(shape2Slot.SlotId);

        phase2 += freq2 * Time.deltaTime;
        phase2 %= 1f;

        float sine2   = Mathf.Sin(phase2 * 2f * Mathf.PI);
        float saw2    = 2f * phase2 - 1f;
        float square2 = phase2 < pw2 ? 1f : -1f;
        float tri2    = 1f - Mathf.Abs((phase2 * 4f + 3f) % 4f - 2f);

        float output2 = shape2Amt < 0.33f
            ? Mathf.Lerp(sine2, tri2, shape2Amt * 3f)
            : shape2Amt < 0.66f
                ? Mathf.Lerp(tri2, square2, (shape2Amt - 0.33f) * 3f)
                : Mathf.Lerp(square2, saw2, (shape2Amt - 0.66f) * 3f);

        output2 *= vol2;

        if (out2Slot != null && out2Slot.OccupiedBy != null)
            PatchRouter.Instance.SendValue(out2Slot.SlotId, output2);

        // 🔊 AUDIO OUTPUT → write into the rolling buffer
        AudioBus.vcoBuffer[AudioBus.writeIndex] = output;
        AudioBus.writeIndex = (AudioBus.writeIndex + 1) % AudioBus.vcoBuffer.Length;
    }
}
