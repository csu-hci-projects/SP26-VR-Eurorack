using UnityEngine;

public class Distorter : MonoBehaviour
{
    [Header("Inputs")]
    public AuxSocketSlot leftInSlot;
    public AuxSocketSlot rightInSlot;
    public AuxSocketSlot mod1Slot;
    public AuxSocketSlot mod2Slot;
    public AuxSocketSlot mod3Slot;
    public AuxSocketSlot mod4Slot;

    [Header("Outputs")]
    public AuxSocketSlot leftOutSlot;
    public AuxSocketSlot rightOutSlot;

    [Header("Knobs")]
    public KnobReporter driveKnob;
    public KnobReporter gainKnob;
    public KnobReporter gain2Knob;
    public KnobReporter freqKnob;
    public KnobReporter freq2Knob;
    public KnobReporter hiCutKnob;
    public KnobReporter hiCut2Knob;
    public KnobReporter bwKnob;
    public KnobReporter feedbackKnob;

    private float feedbackL = 0f;
    private float feedbackR = 0f;

    void Update()
    {
        float drive    = driveKnob    != null ? Mathf.Lerp(1f, 40f,  driveKnob.Value01)     : 1f;
        float gain     = gainKnob     != null ? Mathf.Lerp(0f, 2f,   gainKnob.Value01)      : 1f;
        float gain2    = gain2Knob    != null ? Mathf.Lerp(0f, 2f,   gain2Knob.Value01)     : 1f;
        float feedback = feedbackKnob != null ? Mathf.Lerp(0f, 0.9f, feedbackKnob.Value01)  : 0f;

        float leftIn  = leftInSlot  != null && leftInSlot.OccupiedBy  != null
            ? PatchRouter.Instance.ReadValue(leftInSlot.SlotId)  : 0f;
        float rightIn = rightInSlot != null && rightInSlot.OccupiedBy != null
            ? PatchRouter.Instance.ReadValue(rightInSlot.SlotId) : 0f;

        leftIn  += feedbackL * feedback;
        rightIn += feedbackR * feedback;

        float distL = Tanh(leftIn  * drive) * gain;
        float distR = Tanh(rightIn * drive) * gain;

        feedbackL = distL;
        feedbackR = distR;

        if (mod1Slot != null && mod1Slot.OccupiedBy != null)
        {
            float mod = PatchRouter.Instance.ReadValue(mod1Slot.SlotId);
            distL *= mod;
            distR *= mod;
        }

        if (mod2Slot != null && mod2Slot.OccupiedBy != null)
        {
            float mod = PatchRouter.Instance.ReadValue(mod2Slot.SlotId);
            distL *= mod;
            distR *= mod;
        }

        if (mod3Slot != null && mod3Slot.OccupiedBy != null)
        {
            float mod = PatchRouter.Instance.ReadValue(mod3Slot.SlotId);
            distL *= mod;
        }

        if (mod4Slot != null && mod4Slot.OccupiedBy != null)
        {
            float mod = PatchRouter.Instance.ReadValue(mod4Slot.SlotId);
            distR *= mod;
        }

        distL = Mathf.Clamp(distL * gain2, -1f, 1f);
        distR = Mathf.Clamp(distR * gain2, -1f, 1f);

        if (leftOutSlot  != null && leftOutSlot.OccupiedBy  != null)
            PatchRouter.Instance.SendValue(leftOutSlot.SlotId,  distL);
        if (rightOutSlot != null && rightOutSlot.OccupiedBy  != null)
            PatchRouter.Instance.SendValue(rightOutSlot.SlotId, distR);
    }

    float Tanh(float x)
    {
        float e1 = Mathf.Exp(x);
        float e2 = Mathf.Exp(-x);
        return (e1 - e2) / (e1 + e2);
    }
}