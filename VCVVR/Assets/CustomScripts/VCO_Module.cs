using UnityEngine;

public class VCO_Module : MonoBehaviour {
    public ModularEngine engine;
    public int moduleId = 0;

    public void OnFreqKnob(float value) {
        engine.SetVCOFreq(value);
    }

    public void OnPWKnob(float value) {
        engine.SetVCOPW(value);
    }

    public KnobReporter freqKnob;

    void Update()
    {
        if (outputSlot == null || outputSlot.OccupiedBy == null) return;

        if (freqKnob != null)
            frequency = Mathf.Lerp(20f, 2000f, freqKnob.Value01);

        float sample = Mathf.Sin(Time.time * frequency * 2f * Mathf.PI);
        PatchRouter.Instance.SendValue(outputSlot.SlotId, sample);
    }
}
