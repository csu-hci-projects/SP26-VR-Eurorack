using UnityEngine;

public class Drum : MonoBehaviour
{
    public AuxSocketSlot triggerInput;
    public AuxSocketSlot outputSlot;

    private float amp = 0f;

    void Update()
    {
        float trig = PatchRouter.Instance.ReadValue(triggerInput.SlotId);

        if (trig > 0.5f)
            amp = 1f;

        amp -= Time.deltaTime * 5f;
        amp = Mathf.Clamp01(amp);

        float sample = amp * Mathf.Sin(Time.time * 200f * Mathf.PI * 2f);

        PatchRouter.Instance.SendValue(outputSlot.SlotId, sample);
    }
}
