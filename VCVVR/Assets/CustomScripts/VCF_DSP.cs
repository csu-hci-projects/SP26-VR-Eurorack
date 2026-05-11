using UnityEngine;

public class VCF_DSP
{
    private float sampleRate;
    private float prevLP = 0f;
    private float prevBP = 0f;

    public float Cutoff  = 1000f;
    public float Res     = 0f;
    public float Mode    = 0f;

    public VCF_DSP(float sr)
    {
        sampleRate = sr;
    }

    public float ProcessSample(float input)
    {
        float cutoff = Mathf.Clamp(Cutoff, 20f, 20000f);
        float RC     = 1f / (cutoff * 2f * Mathf.PI);
        float dt     = 1f / sampleRate;
        float alpha  = dt / (RC + dt);

        prevLP = prevLP + alpha * (input - prevLP);
        prevBP = prevBP + alpha * (input - prevLP - prevBP);
        float hp = input - prevLP - Res * prevBP;

        return Mathf.Lerp(prevLP,
            Mathf.Lerp(prevBP, hp, Mathf.Clamp01((Mode - 0.5f) * 2f)),
            Mathf.Clamp01(Mode * 2f));
    }
}