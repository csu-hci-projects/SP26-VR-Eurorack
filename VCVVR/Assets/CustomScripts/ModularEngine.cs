using UnityEngine;
using System.Collections.Generic;

public class ModularEngine : MonoBehaviour {

    public struct Connection {
        public int fromModule;
        public int fromPort;
        public int toModule;
        public int toPort;
    }

    private List<Connection> connections = new List<Connection>();

    private VCO_DSP vco;
    private AudioOutput_DSP audioOut;

    void Start() {
        float sr = AudioSettings.outputSampleRate;

        vco = new VCO_DSP(sr);
        audioOut = new AudioOutput_DSP();
    }

    public void Connect(int fromModule, int fromPort, int toModule, int toPort) {
        connections.Add(new Connection {
            fromModule = fromModule,
            fromPort = fromPort,
            toModule = toModule,
            toPort = toPort
        });
    }

    float GetModuleOutput(int moduleId, int portId) {
        if (moduleId == 0) return vco.GetOutput(portId);
        return 0f;
    }

    void SetModuleInput(int moduleId, int portId, float value) {
        if (moduleId == 1) audioOut.SetInput(portId, value);
    }

    void OnAudioFilterRead(float[] data, int channels) {
        int frames = data.Length / channels;

        vco.ProcessBlock(frames);

        audioOut.SetInput(0, 0f);
        audioOut.SetInput(1, 0f);

        foreach (var c in connections) {
            float v = GetModuleOutput(c.fromModule, c.fromPort);
            SetModuleInput(c.toModule, c.toPort, v);
        }

        for (int i = 0; i < frames; i++) {
            float L = audioOut.GetLeft();
            float R = audioOut.GetRight();

            data[i * channels] = L;
            if (channels > 1)
                data[i * channels + 1] = R;
        }
    }

    public void SetVCOFreq(float hz) => vco.SetParam(0, hz);
    public void SetVCOPW(float pw) => vco.SetParam(1, pw);
    public void SetAudioVolume(float v) => audioOut.SetParam(0, v);
}
