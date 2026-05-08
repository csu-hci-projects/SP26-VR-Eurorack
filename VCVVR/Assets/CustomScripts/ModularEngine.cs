using UnityEngine;
using System.Collections.Generic;

public class ModularEngine : MonoBehaviour
{
    public struct Connection
    {
        public int fromModule, fromPort;
        public int toModule, toPort;
    }

    private VCO_DSP vco;
    private AudioOutput_DSP audioOut;

    public List<Connection> connections = new();

    void Start()
    {
        float sr = AudioSettings.outputSampleRate;
        vco = new VCO_DSP(sr);
        audioOut = new AudioOutput_DSP();
    }

    public void Connect(int fromModule, int fromPort, int toModule, int toPort)
    {
        foreach (var c in connections)
            if (c.fromModule == fromModule && c.fromPort == fromPort &&
                c.toModule == toModule && c.toPort == toPort)
                return;

        connections.Add(new Connection
        {
            fromModule = fromModule, fromPort = fromPort,
            toModule = toModule, toPort = toPort
        });
    }

    public void Disconnect(int fromModule, int fromPort, int toModule, int toPort)
    {
        connections.RemoveAll(c =>
            c.fromModule == fromModule && c.fromPort == fromPort &&
            c.toModule == toModule && c.toPort == toPort);
    }

    void OnAudioFilterRead(float[] data, int channels) { }

    public void SetVCOFreq(float hz) => vco.SetParam(0, hz);
    public void SetVCOPW(float pw) => vco.SetParam(1, pw);
    public void SetAudioVolume(float v) => audioOut.SetParam(0, v);
}