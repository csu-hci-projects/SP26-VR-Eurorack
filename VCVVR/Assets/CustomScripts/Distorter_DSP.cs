using UnityEngine;

public class Distorter_DSP
{
    public float Drive    = 1f;
    public float Gain     = 1f;
    public float Gain2    = 1f;
    public float Feedback = 0f;

    private float feedbackL = 0f;
    private float feedbackR = 0f;

    public void ProcessSample(float inL, float inR, out float outL, out float outR)
    {
        inL += feedbackL * Feedback;
        inR += feedbackR * Feedback;

        float distL = Tanh(inL * Drive) * Gain;
        float distR = Tanh(inR * Drive) * Gain;

        feedbackL = distL;
        feedbackR = distR;

        outL = Mathf.Clamp(distL * Gain2, -1f, 1f);
        outR = Mathf.Clamp(distR * Gain2, -1f, 1f);
    }

    float Tanh(float x)
    {
        float e1 = Mathf.Exp(x);
        float e2 = Mathf.Exp(-x);
        return (e1 - e2) / (e1 + e2);
    }
}