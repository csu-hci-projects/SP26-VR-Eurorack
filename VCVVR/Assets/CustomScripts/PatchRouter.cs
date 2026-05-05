using UnityEngine;
using System.Collections.Generic;

public class PatchRouter : MonoBehaviour
{
    public static PatchRouter Instance;

    // Wire this in the Inspector to your ModularEngine GameObject
    public ModularEngine engine;

    private Dictionary<string, float> audioValues = new();

    void Awake()
    {
        Instance = this;
    }

    public void SendValue(string slotId, float value)
    {
        audioValues[slotId] = value;

        // Bridge into DSP engine — slotId drives known parameters
        if (engine == null) return;
        switch (slotId)
        {
            case "VCO_FREQ": engine.SetVCOFreq(value * 2000f + 20f); break; // 0‑1 → 20‑2020 Hz
            case "VCO_PW":   engine.SetVCOPW(value);                 break;
            case "OUT_VOL":  engine.SetAudioVolume(value);            break;
        }
    }

    public float ReadValue(string slotId)
    {
        return audioValues.TryGetValue(slotId, out float v) ? v : 0f;
    }
}