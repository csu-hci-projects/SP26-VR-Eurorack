using UnityEngine;

public class VCO : MonoBehaviour
{
    public AuxSocketSlot outputSlot;
    public float frequency = 440f;

    void Update()
    {
        float sample = Mathf.Sin(Time.time * frequency * 2f * Mathf.PI);

        if (outputSlot.OccupiedBy != null)
            PatchRouter.Instance.SendValue(outputSlot.SlotId, sample);
    }
}
