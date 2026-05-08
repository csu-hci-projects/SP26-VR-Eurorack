using UnityEngine;

public class VCF : MonoBehaviour
{
    [Header("Inputs")]
    public AuxSocketSlot inputSlot;
    public AuxSocketSlot vOctSlot;
    public AuxSocketSlot cvSlot;
    public AuxSocketSlot fmSlot;

    [Header("Outputs")]
    public AuxSocketSlot outputSlot;
    public AuxSocketSlot rBwSlot;
    public AuxSocketSlot slpSlot;

    [Header("Knobs")]
    public KnobReporter hzKnob;
    public KnobReporter resBwKnob;
    public KnobReporter slopeKnob;
    public KnobReporter cvKnob;
    public KnobReporter modeKnob;

    private float prevLP = 0f;
    private float prevBP = 0f;

    void Update()
    {
        float input = inputSlot != null && inputSlot.OccupiedBy != null
            ? PatchRouter.Instance.ReadValue(inputSlot.SlotId) : 0f;

        float cutoff = hzKnob != null ? Mathf.Lerp(20f, 18000f, hzKnob.Value01) : 1000f;

        if (vOctSlot != null && vOctSlot.OccupiedBy != null)
            cutoff *= Mathf.Pow(2f, PatchRouter.Instance.ReadValue(vOctSlot.SlotId) * 5f);

        if (cvSlot != null && cvSlot.OccupiedBy != null)
        {
            float cvAmt = cvKnob != null ? cvKnob.Value01 : 1f;
            cutoff += PatchRouter.Instance.ReadValue(cvSlot.SlotId) * cutoff * cvAmt;
        }

        if (fmSlot != null && fmSlot.OccupiedBy != null)
            cutoff += PatchRouter.Instance.ReadValue(fmSlot.SlotId) * 2000f;

        cutoff = Mathf.Clamp(cutoff, 20f, 20000f);

        float res = resBwKnob != null ? resBwKnob.Value01 : 0f;
        float mode = modeKnob != null ? modeKnob.Value01 : 0f;

        float RC = 1f / (cutoff * 2f * Mathf.PI);
        float dt = Time.deltaTime;
        float alpha = dt / (RC + dt);

        prevLP = prevLP + alpha * (input - prevLP);
        prevBP = prevBP + alpha * (input - prevLP - prevBP);
        float hp = input - prevLP - res * prevBP;

        float filtered = Mathf.Lerp(prevLP, Mathf.Lerp(prevBP, hp, Mathf.Clamp01((mode - 0.5f) * 2f)), Mathf.Clamp01(mode * 2f));

        if (outputSlot != null && outputSlot.OccupiedBy != null)
            PatchRouter.Instance.SendValue(outputSlot.SlotId, filtered);

        if (rBwSlot != null && rBwSlot.OccupiedBy != null)
            PatchRouter.Instance.SendValue(rBwSlot.SlotId, prevBP);

        if (slpSlot != null && slpSlot.OccupiedBy != null)
            PatchRouter.Instance.SendValue(slpSlot.SlotId, hp);
    }
}