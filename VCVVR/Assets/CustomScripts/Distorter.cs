using UnityEngine;

public class Distorter : MonoBehaviour
{
    public AuxSocketSlot inputSlot;
    public AuxSocketSlot outputSlot;
    public KnobReporter driveKnob;

    void Update()
    {
        float input = PatchRouter.Instance.ReadValue(inputSlot.SlotId);
        float drive = Mathf.Lerp(1f, 20f, driveKnob.Value01);

        float x = input * drive;
        float distorted = Tanh(x);

        PatchRouter.Instance.SendValue(outputSlot.SlotId, distorted);
    }

    float Tanh(float x)
    {
        float e1 = Mathf.Exp(x);
        float e2 = Mathf.Exp(-x);
        return (e1 - e2) / (e1 + e2);
    }

}
