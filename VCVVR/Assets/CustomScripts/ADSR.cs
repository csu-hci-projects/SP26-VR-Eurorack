using UnityEngine;

public class ADSR : MonoBehaviour
{
    [Header("Inputs")]
    public AuxSocketSlot gateSlot;
    public AuxSocketSlot retrigSlot;
    public AuxSocketSlot cv01Slot;
    public AuxSocketSlot cv02Slot;
    public AuxSocketSlot cv03Slot;
    public AuxSocketSlot cv04Slot;

    [Header("Outputs")]
    public AuxSocketSlot outputSlot;

    [Header("Sliders")]
    public SliderReporter attackSlider;
    public SliderReporter decaySlider;
    public SliderReporter sustainSlider;
    public SliderReporter releaseSlider;

    private float env = 0f;
    private bool gateOn = false;
    private bool lastRetrig = false;

    void Update()
    {
        float gate = gateSlot != null && gateSlot.OccupiedBy != null
            ? PatchRouter.Instance.ReadValue(gateSlot.SlotId) : 0f;

        bool newGate = gate > 0.5f;
        if (newGate && !gateOn) gateOn = true;
        if (!newGate && gateOn) gateOn = false;

        if (retrigSlot != null && retrigSlot.OccupiedBy != null)
        {
            bool retrig = PatchRouter.Instance.ReadValue(retrigSlot.SlotId) > 0.5f;
            if (retrig && !lastRetrig) env = 0f;
            lastRetrig = retrig;
        }

        float a = attackSlider  != null ? Mathf.Lerp(0.001f, 4f, attackSlider.Value01)  : 0.01f;
        float d = decaySlider   != null ? Mathf.Lerp(0.001f, 4f, decaySlider.Value01)   : 0.1f;
        float s = sustainSlider != null ? sustainSlider.Value01 : 0.7f;
        float r = releaseSlider != null ? Mathf.Lerp(0.001f, 4f, releaseSlider.Value01) : 0.3f;

        if (gateOn)
        {
            if (env < 1f)
                env += Time.deltaTime / a;
            else
                env = Mathf.Max(env - Time.deltaTime * (1f - s) / d, s);
        }
        else
        {
            env -= Time.deltaTime / r;
        }

        env = Mathf.Clamp01(env);

        if (outputSlot != null && outputSlot.OccupiedBy != null)
            PatchRouter.Instance.SendValue(outputSlot.SlotId, env);

        if (cv01Slot != null && cv01Slot.OccupiedBy != null)
            PatchRouter.Instance.SendValue(cv01Slot.SlotId, env);
        if (cv02Slot != null && cv02Slot.OccupiedBy != null)
            PatchRouter.Instance.SendValue(cv02Slot.SlotId, env);
        if (cv03Slot != null && cv03Slot.OccupiedBy != null)
            PatchRouter.Instance.SendValue(cv03Slot.SlotId, env);
        if (cv04Slot != null && cv04Slot.OccupiedBy != null)
            PatchRouter.Instance.SendValue(cv04Slot.SlotId, env);
    }
}