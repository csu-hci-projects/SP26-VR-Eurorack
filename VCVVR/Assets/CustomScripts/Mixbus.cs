using UnityEngine;

public class Mixbus : MonoBehaviour
{
    [Header("Inputs")]
    public AuxSocketSlot[] inputSlots = new AuxSocketSlot[8];

    [Header("Knobs")]
    public KnobReporter[] channelKnobs = new KnobReporter[8];

    [Header("Master")]
    public SliderReporter masterSlider;

    [Header("Outputs")]
    public AuxSocketSlot leftOutSlot;
    public AuxSocketSlot rightOutSlot;

    private float mixedLeft = 0f;
    private float mixedRight = 0f;
    private AudioSource audioSource;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.playOnAwake = true;
        audioSource.loop = true;
        audioSource.clip = null;
        audioSource.Play();
    }

    void Update()
    {
        float master = masterSlider != null ? masterSlider.Value01 : 0.5f;
        float sum = 0f;

        for (int i = 0; i < inputSlots.Length; i++)
        {
            if (inputSlots[i] == null || inputSlots[i].OccupiedBy == null) continue;
            float ch = channelKnobs != null && i < channelKnobs.Length && channelKnobs[i] != null
                ? channelKnobs[i].Value01 : 1f;
            sum += PatchRouter.Instance.ReadValue(inputSlots[i].SlotId) * ch;
        }

        mixedLeft = Mathf.Clamp(sum * master, -1f, 1f);
        mixedRight = mixedLeft;

        if (leftOutSlot != null && leftOutSlot.OccupiedBy != null)
            PatchRouter.Instance.SendValue(leftOutSlot.SlotId, mixedLeft);
        if (rightOutSlot != null && rightOutSlot.OccupiedBy != null)
            PatchRouter.Instance.SendValue(rightOutSlot.SlotId, mixedRight);
    }

    void OnAudioFilterRead(float[] data, int channels)
    {
        for (int i = 0; i < data.Length; i += channels)
        {
            data[i] = mixedLeft;
            if (channels > 1) data[i + 1] = mixedRight;
        }
    }
}