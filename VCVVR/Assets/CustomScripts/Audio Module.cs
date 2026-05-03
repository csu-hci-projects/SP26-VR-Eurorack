using UnityEngine;

public class AudioModule : MonoBehaviour
{
    public AuxSocketSlot inputSlot;
    public AuxSocketSlot outputSlot;
    public KnobReporter gainKnob;

    void Update()
    {
        float input = PatchRouter.Instance.ReadValue(inputSlot.SlotId);
        float gain = Mathf.Lerp(0f, 2f, gainKnob.Value01);

        float outVal = input * gain;

        PatchRouter.Instance.SendValue(outputSlot.SlotId, outVal);
    }
}
