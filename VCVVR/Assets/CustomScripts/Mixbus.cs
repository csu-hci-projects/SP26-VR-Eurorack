using UnityEngine;

public class Mixbus : MonoBehaviour
{
    [Header("Inputs")]
    public AuxSocketSlot[] inputSlots = new AuxSocketSlot[8];

    [Header("Knobs")]
    public KnobReporter[] channelKnobs = new KnobReporter[8];

    [Header("Master")]
    public SliderReporter masterSlider;

    [Header("Module References (assign in Inspector)")]
    public VCO       vco;
    public Drum      drum;
    public VCF       vcf;
    public Distorter distorter;

    // DSP objects
    private VCO_DSP       vcoDsp;
    private Drum_DSP      drumDsp;
    private VCF_DSP       vcfDsp;
    private Distorter_DSP distDsp;
    private float         sampleRate;

    // Control-rate params (Update → audio thread)
    private volatile float vcoFreq  = 440f;
    private volatile float vcoPW    = 0.5f;
    private volatile float vcoShape = 0f;
    private volatile float vcoVol   = 0f;   // 0 until volKnob turned
    private volatile float vcoEnv   = 1f;

    private volatile bool drum0Trig = false;
    private volatile bool drum1Trig = false;
    private volatile bool drum2Trig = false;
    private volatile bool drum3Trig = false;
    private volatile bool drum4Trig = false;
    private bool[] lastDrumTrig = new bool[5];

    private volatile float vcfCutoff = 1000f;
    private volatile float vcfRes    = 0f;
    private volatile float vcfMode   = 0f;

    private volatile float distDrive    = 1f;
    private volatile float distGain     = 0f;   // 0 until gainKnob turned
    private volatile float distGain2    = 0f;   // 0 until gain2Knob turned
    private volatile float distFeedback = 0f;

    private volatile bool  vcoActive  = false;
    private volatile bool  drumActive = false;
    private volatile bool  vcfActive  = false;
    private volatile bool  distActive = false;
    private volatile float masterVol  = 0f;    // 0 until master slider moved

    private AudioSource audioSource;
    private int debugFrame = 0;

    bool SlotConnectedTo(AuxSocketSlot mixSlot, AuxSocketSlot sourceSlot)
    {
        if (mixSlot == null || sourceSlot == null) return false;
        if (mixSlot.OccupiedBy == null) return false;
        return PatchRouter.Instance.GetRoot(mixSlot.SlotId) ==
               PatchRouter.Instance.GetRoot(sourceSlot.SlotId);
    }

    void Awake()
    {
        sampleRate = AudioSettings.outputSampleRate;
        vcoDsp  = new VCO_DSP(sampleRate);
        drumDsp = new Drum_DSP(sampleRate);
        vcfDsp  = new VCF_DSP(sampleRate);
        distDsp = new Distorter_DSP();

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.spatialBlend = 0f;
        audioSource.loop        = true;
        audioSource.playOnAwake = true;
        audioSource.volume      = 1f;
        audioSource.mute        = false;
        audioSource.Play();

        Debug.Log($"[Mixbus] Awake. sampleRate={sampleRate} isPlaying={audioSource.isPlaying}");
    }

    void Update()
    {
        // ── VCO ─────────────────────────────────────────────────────────────
        if (vco != null)
        {
            float freq = vco.baseFrequency;
            if (vco.vOctSlot != null && vco.vOctSlot.OccupiedBy != null)
                freq *= Mathf.Pow(2f, PatchRouter.Instance.ReadValue(vco.vOctSlot.SlotId) * 5f);
            if (vco.fmSlot != null && vco.fmSlot.OccupiedBy != null)
            {
                float fmAmt = vco.fmKnob != null ? Mathf.Lerp(0f, 500f, vco.fmKnob.Value01) : 200f;
                freq += PatchRouter.Instance.ReadValue(vco.fmSlot.SlotId) * fmAmt;
            }
            vcoFreq  = Mathf.Clamp(freq, 20f, 20000f);
            vcoPW    = vco.pwKnob != null ? Mathf.Lerp(0.05f, 0.95f, vco.pwKnob.Value01) : 0.5f;
            vcoShape = vco.shapeKnob != null ? vco.shapeKnob.Value01 : 0f;

            // Vol straight from knob — 0 until turned
            vcoVol = vco.volKnob != null ? vco.volKnob.Value01 : 0f;
            if (vco.volSlot != null && vco.volSlot.OccupiedBy != null)
                vcoVol = PatchRouter.Instance.ReadValue(vco.volSlot.SlotId);

            vcoEnv = 1f;
        }

        // ── Drum triggers ────────────────────────────────────────────────────
        if (drum != null)
        {
            AuxSocketSlot[] trigs = {
                drum.trig1, drum.trig2, drum.trig3, drum.trig4, drum.trig5
            };
            bool[] newTrigs = new bool[5];
            for (int i = 0; i < 5; i++)
            {
                if (trigs[i] != null && trigs[i].OccupiedBy != null)
                    newTrigs[i] = PatchRouter.Instance.ReadValue(trigs[i].SlotId) > 0.5f;
            }
            if (newTrigs[0] && !lastDrumTrig[0]) drum0Trig = true;
            if (newTrigs[1] && !lastDrumTrig[1]) drum1Trig = true;
            if (newTrigs[2] && !lastDrumTrig[2]) drum2Trig = true;
            if (newTrigs[3] && !lastDrumTrig[3]) drum3Trig = true;
            if (newTrigs[4] && !lastDrumTrig[4]) drum4Trig = true;
            for (int i = 0; i < 5; i++) lastDrumTrig[i] = newTrigs[i];
        }

        // ── VCF ──────────────────────────────────────────────────────────────
        if (vcf != null)
        {
            float cutoff = vcf.hzKnob != null
                ? Mathf.Lerp(20f, 18000f, vcf.hzKnob.Value01) : 1000f;
            if (vcf.vOctSlot != null && vcf.vOctSlot.OccupiedBy != null)
                cutoff *= Mathf.Pow(2f, PatchRouter.Instance.ReadValue(vcf.vOctSlot.SlotId) * 5f);
            if (vcf.cvSlot != null && vcf.cvSlot.OccupiedBy != null)
            {
                float cvAmt = vcf.cvKnob != null ? vcf.cvKnob.Value01 : 1f;
                cutoff += PatchRouter.Instance.ReadValue(vcf.cvSlot.SlotId) * cutoff * cvAmt;
            }
            if (vcf.fmSlot != null && vcf.fmSlot.OccupiedBy != null)
                cutoff += PatchRouter.Instance.ReadValue(vcf.fmSlot.SlotId) * 2000f;

            vcfCutoff = Mathf.Clamp(cutoff, 20f, 20000f);
            vcfRes    = vcf.resBwKnob != null ? vcf.resBwKnob.Value01 : 0f;
            vcfMode   = vcf.modeKnob  != null ? vcf.modeKnob.Value01  : 0f;
        }

        // ── Distorter ────────────────────────────────────────────────────────
        if (distorter != null)
        {
            distDrive    = distorter.driveKnob    != null
                ? Mathf.Lerp(1f, 40f,  distorter.driveKnob.Value01)    : 1f;
            // gain and gain2 default to 0 — silent until knob turned
            distGain     = distorter.gainKnob     != null
                ? Mathf.Lerp(0f, 2f,   distorter.gainKnob.Value01)     : 0f;
            distGain2    = distorter.gain2Knob    != null
                ? Mathf.Lerp(0f, 2f,   distorter.gain2Knob.Value01)    : 0f;
            distFeedback = distorter.feedbackKnob != null
                ? Mathf.Lerp(0f, 0.9f, distorter.feedbackKnob.Value01) : 0f;
        }

        // ── Routing ──────────────────────────────────────────────────────────
        bool vcoSeen = false, drumSeen = false, vcfSeen = false, distSeen = false;

        for (int i = 0; i < inputSlots.Length; i++)
        {
            var slot = inputSlots[i];
            if (slot == null || slot.OccupiedBy == null) continue;

            if (vco != null && vco.outSlot != null &&
                SlotConnectedTo(slot, vco.outSlot))
                vcoSeen = true;

            if (drum != null)
            {
                if ((drum.outL != null && SlotConnectedTo(slot, drum.outL)) ||
                    (drum.outR != null && SlotConnectedTo(slot, drum.outR)))
                    drumSeen = true;
            }

            if (vcf != null && vcf.outputSlot != null &&
                SlotConnectedTo(slot, vcf.outputSlot))
                vcfSeen = true;

            if (distorter != null)
            {
                if ((distorter.leftOutSlot  != null && SlotConnectedTo(slot, distorter.leftOutSlot)) ||
                    (distorter.rightOutSlot != null && SlotConnectedTo(slot, distorter.rightOutSlot)))
                    distSeen = true;
            }
        }

        vcoActive  = vcoSeen;
        drumActive = drumSeen;
        vcfActive  = vcfSeen;
        distActive = distSeen;

        // Master straight from slider — 0 until moved
        masterVol = masterSlider != null ? masterSlider.Value01 : 0f;

        if (debugFrame % 60 == 0)
            Debug.Log($"[Mixbus] vco={vcoActive} drum={drumActive} vcf={vcfActive} dist={distActive} vol={vcoVol:F3} master={masterVol:F3}");

        debugFrame++;
    }

    void OnAudioFilterRead(float[] data, int channels)
    {
        bool anyActive = vcoActive || drumActive || vcfActive || distActive;

        if (!anyActive)
        {
            for (int i = 0; i < data.Length; i++) data[i] = 0f;
            return;
        }

        float freq   = vcoFreq;
        float pw     = vcoPW;
        float shape  = vcoShape;
        float vol    = vcoVol * vcoEnv;
        float master = masterVol;

        vcoDsp.SetParam(0, freq);
        vcoDsp.SetParam(1, pw);
        vcfDsp.Cutoff    = vcfCutoff;
        vcfDsp.Res       = vcfRes;
        vcfDsp.Mode      = vcfMode;
        distDsp.Drive    = distDrive;
        distDsp.Gain     = distGain;
        distDsp.Gain2    = distGain2;
        distDsp.Feedback = distFeedback;

        if (drum0Trig) { drumDsp.Trigger(0); drum0Trig = false; }
        if (drum1Trig) { drumDsp.Trigger(1); drum1Trig = false; }
        if (drum2Trig) { drumDsp.Trigger(2); drum2Trig = false; }
        if (drum3Trig) { drumDsp.Trigger(3); drum3Trig = false; }
        if (drum4Trig) { drumDsp.Trigger(4); drum4Trig = false; }

        bool useVco  = vcoActive;
        bool useDrum = drumActive;
        bool useVcf  = vcfActive;
        bool useDist = distActive;

        for (int i = 0; i < data.Length; i += channels)
        {
            float left  = 0f;
            float right = 0f;

            if (useVco)
            {
                vcoDsp.ProcessBlock(1);

                float osc = shape < 0.33f
                    ? Mathf.Lerp(vcoDsp.OutSin, vcoDsp.OutTri, shape * 3f)
                    : shape < 0.66f
                        ? Mathf.Lerp(vcoDsp.OutTri, vcoDsp.OutSqr, (shape - 0.33f) * 3f)
                        : Mathf.Lerp(vcoDsp.OutSqr, vcoDsp.OutSaw, (shape - 0.66f) * 3f);

                osc *= vol;

                if (useVcf)  osc = vcfDsp.ProcessSample(osc);

                if (useDist)
                {
                    distDsp.ProcessSample(osc, osc, out float dL, out float dR);
                    left  += dL;
                    right += dR;
                }
                else
                {
                    left  += osc;
                    right += osc;
                }
            }

            if (useDrum)
            {
                drumDsp.ProcessSample(out float dL, out float dR);
                left  += dL;
                right += dR;
            }

            data[i] = Mathf.Clamp(left  * master, -1f, 1f);
            if (channels > 1)
                data[i + 1] = Mathf.Clamp(right * master, -1f, 1f);
        }
    }
}