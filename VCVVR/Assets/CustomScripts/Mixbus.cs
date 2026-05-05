using UnityEngine;

public class Mixbus : MonoBehaviour
{
    public AuxSocketSlot inputSlot;

    void OnAudioFilterRead(float[] data, int channels)
    {
        float v = PatchRouter.Instance.ReadValue(inputSlot.SlotId);

        for (int i = 0; i < data.Length; i += channels)
        {
            data[i] = v;
            if (channels > 1)
                data[i + 1] = v;
        }
    }
}
