using UnityEngine;
using System.Collections.Generic;

public class PatchRouter : MonoBehaviour
{
    public static PatchRouter Instance;

    private Dictionary<string, float> audioValues = new();

    void Awake()
    {
        Instance = this;
    }

    public void SendValue(string slotId, float value)
    {
        audioValues[slotId] = value;
    }

    public float ReadValue(string slotId)
    {
        return audioValues.TryGetValue(slotId, out float v) ? v : 0f;
    }
}
