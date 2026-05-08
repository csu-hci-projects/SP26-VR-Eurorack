void Start()
{
    float sr = AudioSettings.outputSampleRate;
    vco      = new VCO_DSP(sr);
    audioOut = new AudioOutput_DSP();
}

void OnAudioFilterRead(float[] data, int channels)
{

}