void OnAudioFilterRead(float[] data, int channels)
{
    if (inputSlot == null || inputSlot.OccupiedBy == null)
    {
        for (int i = 0; i < data.Length; i++) data[i] = 0f;
        return;
    }

    float v = PatchRouter.Instance.ReadValue(inputSlot.SlotId);

    for (int i = 0; i < data.Length; i += channels)
    {
        data[i] = v;
        if (channels > 1)
            data[i + 1] = v;
    }
}