using UnityEngine;

public class AudioModule : MonoBehaviour
{
    [Header("Inputs")]
    public AuxSocketSlot leftInSlot;
    public AuxSocketSlot rightInSlot;

    [Header("Outputs")]
    public AuxSocketSlot monLeftOutSlot;
    public AuxSocketSlot monRightOutSlot;

    [Header("Knobs")]
    public KnobReporter levelKnob;

    void Update()
    {
        float gain = levelKnob != null ? Mathf.Lerp(0f, 2f, levelKnob.Value01) : 1f;

        float left  = leftInSlot  != null && leftInSlot.OccupiedBy  != null
            ? PatchRouter.Instance.ReadValue(leftInSlot.SlotId)  * gain : 0f;
        float right = rightInSlot != null && rightInSlot.OccupiedBy != null
            ? PatchRouter.Instance.ReadValue(rightInSlot.SlotId) * gain : 0f;

        if (monLeftOutSlot  != null && monLeftOutSlot.OccupiedBy  != null)
            PatchRouter.Instance.SendValue(monLeftOutSlot.SlotId,  left);
        if (monRightOutSlot != null && monRightOutSlot.OccupiedBy != null)
            PatchRouter.Instance.SendValue(monRightOutSlot.SlotId, right);
    }
}