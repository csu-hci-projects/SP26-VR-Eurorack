using UnityEngine;

public class VCO : MonoBehaviour
{
    public AuxSocketSlot outputSlot;
    public float frequency = 440f;

    void Update()
    {
        if (outputSlot == null || outputSlot.OccupiedBy == null) return;

        float sample = Mathf.Sin(Time.time * frequency * 2f * Mathf.PI);
        PatchRouter.Instance.SendValue(outputSlot.SlotId, sample);
    }
}
