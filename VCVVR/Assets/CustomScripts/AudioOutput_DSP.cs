public class AudioOutput_DSP {
    public float leftIn;
    public float rightIn;

    public float volume = 1f;

    public void SetInput(int id, float value) {
        if (id == 0) leftIn = value;
        if (id == 1) rightIn = value;
    }

    public void SetParam(int id, float value) {
        if (id == 0) volume = value;
    }

    public float GetLeft() => leftIn * volume;
    public float GetRight() => rightIn * volume;
}
