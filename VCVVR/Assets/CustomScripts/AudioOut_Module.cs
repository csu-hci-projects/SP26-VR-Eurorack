using UnityEngine;

public class AudioOut_Module : MonoBehaviour {
    public ModularEngine engine;
    public int moduleId = 1;

    public void OnVolumeKnob(float value) {
        engine.SetAudioVolume(value);
    }
}
