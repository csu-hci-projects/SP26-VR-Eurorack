using UnityEngine;

public class VCO_DSP {
    private float sampleRate;
    private float phase;

    private float freq = 440f;
    private float pulseWidth = 0.5f;

    public float OutSin { get; private set; }
    public float OutSaw { get; private set; }
    public float OutSqr { get; private set; }
    public float OutTri { get; private set; }

    public VCO_DSP(float sr) {
        Reset(sr);
    }

    public void Reset(float sr) {
        sampleRate = sr;
        phase = 0f;
    }

    public void SetParam(int id, float value) {
        switch (id) {
            case 0: freq = Mathf.Max(0f, value); break;
            case 1: pulseWidth = Mathf.Clamp01(value); break;
        }
    }

    public void ProcessBlock(int frames) {
        float dt = 1f / sampleRate;

        for (int i = 0; i < frames; i++) {
            phase += freq * dt;
            if (phase >= 1f)
                phase -= 1f;

            OutSin = Mathf.Sin(phase * 2f * Mathf.PI);
            OutSaw = 2f * phase - 1f;
            OutSqr = (phase < pulseWidth) ? 1f : -1f;
            OutTri = 1f - Mathf.Abs((phase * 4f + 3f) % 4f - 2f);
        }
    }

    public float GetOutput(int id) {
        switch (id) {
            case 0: return OutSin;
            case 1: return OutTri;
            case 2: return OutSaw;
            case 3: return OutSqr;
        }
        return 0f;
    }
}
