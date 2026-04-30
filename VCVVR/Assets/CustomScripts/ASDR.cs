using UnityEngine;

public class ADSR : MonoBehaviour
{
    public AuxSocketSlot gateInput;
    public AuxSocketSlot outputSlot;

    public KnobReporter attackKnob;
    public KnobReporter decayKnob;
    public KnobReporter sustainKnob;
    public KnobReporter releaseKnob;

    private float env = 0f;
    private bool gateOn = false;

    void Update()
    {
        float gate = PatchRouter.Instance.ReadValue(gateInput.SlotId);
        bool newGate = gate > 0.5f;

        if (newGate && !gateOn)
            gateOn = true;

        if (!newGate && gateOn)
            gateOn = false;

        float a = Mathf.Lerp(0.001f, 1f, attackKnob.Value01);
        float d = Mathf.Lerp(0.001f, 1f, decayKnob.Value01);
        float s = sustainKnob.Value01;
        float r = Mathf.Lerp(0.001f, 1f, releaseKnob.Value01);

        if (gateOn)
        {
            if (env < 1f)
                env += Time.deltaTime / a;
            else
                env -= Time.deltaTime * (1f - s) / d;
        }
        else
        {
            env -= Time.deltaTime / r;
        }

        env = Mathf.Clamp01(env);

        PatchRouter.Instance.SendValue(outputSlot.SlotId, env);
    }
}
