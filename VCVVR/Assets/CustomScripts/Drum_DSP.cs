using UnityEngine;

public class Drum_DSP
{
    private float sampleRate;

    // 5 drum voices
    private float[] amp   = new float[5];
    private float[] freq  = new float[] { 60f, 120f, 200f, 400f, 800f };
    private float[] phase = new float[5];

    // Decay rate per second
    private float decayRate = 8f;

    public Drum_DSP(float sr)
    {
        sampleRate = sr;
    }

    // Call from Update() on rising trigger edge for voice i
    public void Trigger(int voice)
    {
        if (voice >= 0 && voice < 5)
            amp[voice] = 1f;
    }

    // Process one sample, returns stereo mix
    public void ProcessSample(out float left, out float right)
    {
        float mixL = 0f;
        float mixR = 0f;
        float dt   = 1f / sampleRate;

        for (int i = 0; i < 5; i++)
        {
            amp[i]    = Mathf.Max(0f, amp[i] - dt * decayRate);
            phase[i] += freq[i] * dt;
            if (phase[i] >= 1f) phase[i] -= 1f;

            float sample = amp[i] * Mathf.Sin(phase[i] * 2f * Mathf.PI);
            mixL += sample;
            mixR += sample;
        }

        left  = Mathf.Clamp(mixL * 0.2f, -1f, 1f);
        right = Mathf.Clamp(mixR * 0.2f, -1f, 1f);
    }
}