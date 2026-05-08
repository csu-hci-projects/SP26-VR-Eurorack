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
}
