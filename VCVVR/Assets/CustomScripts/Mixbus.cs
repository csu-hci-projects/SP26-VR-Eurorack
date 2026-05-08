using UnityEngine;

public class Mixbus : MonoBehaviour
{
    [Header("Inputs")]
    public AuxSocketSlot[] inputSlots = new AuxSocketSlot[8];

    [Header("Knobs")]
    public KnobReporter[] channelKnobs = new KnobReporter[8];

    [Header("Master")]
    public SliderReporter masterSlider;

    private AudioSource audioSource;
    private float sampleRate;

    // Cached mix for audio thread
    private float mixedLeft = 0f;
    private float mixedRight = 0f;
    private bool anyPatched = false;

    void Awake()
    {
        sampleRate = AudioSettings.outputSampleRate;

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.spatialBlend = 0f;
        audioSource.loop = true;
        audioSource.playOnAwake = true;
        audioSource.volume = 1f;
        audioSource.mute = false;

        audioSource.Play();
    }

    void Update()
    {
        anyPatched = false;
        float mixL = 0f;
        float mixR = 0f;

        for (int i = 0; i < inputSlots.Length; i++)
        {
            var slot = inputSlots[i];
            if (slot == null || slot.OccupiedBy == null)
                continue;

            anyPatched = true;

            float v = PatchRouter.Instance.ReadValue(slot.SlotId);

            float gain = 1f;
            if (i < channelKnobs.Length && channelKnobs[i] != null)
                gain = channelKnobs[i].Value01;

            v *= gain;

            mixL += v;
            mixR += v;
        }

        float master = masterSlider != null ? masterSlider.Value01 : 1f;
        mixL *= master;
        mixR *= master;

        mixedLeft  = Mathf.Clamp(mixL, -1f, 1f);
        mixedRight = Mathf.Clamp(mixR, -1f, 1f);
    }

    void OnAudioFilterRead(float[] data, int channels)
    {
        if (!anyPatched)
        {
            for (int i = 0; i < data.Length; i++)
                data[i] = 0f;
            return;
        }

        for (int i = 0; i < data.Length; i += channels)
        {
            data[i] = mixedLeft;
            if (channels > 1)
                data[i + 1] = mixedRight;
        }
    }
}
