using UnityEngine;

public class VCF : MonoBehaviour
{
    public AuxSocketSlot inputSlot;
    public AuxSocketSlot outputSlot;
    public KnobReporter cutoffKnob;

    private float prev = 0f;

    void Update()
    {
        float input = PatchRouter.Instance.ReadValue(inputSlot.SlotId);
        float cutoff = Mathf.Lerp(200f, 8000f, cutoffKnob.Value01);

        float filtered = LowPass(input, cutoff);

        if (outputSlot.OccupiedBy != null)
            PatchRouter.Instance.SendValue(outputSlot.SlotId, filtered);
    }

    float LowPass(float x, float cutoff)
    {
        float RC = 1f / (cutoff * 2f * Mathf.PI);
        float dt = Time.deltaTime;
        float alpha = dt / (RC + dt);

        prev = prev + alpha * (x - prev);
        return prev;
    }
}
